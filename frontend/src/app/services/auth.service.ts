import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap, of } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiBase}/auth`;

  private authStatus = new BehaviorSubject<boolean>(false);
  isLoggedIn$ = this.authStatus.asObservable();
  private checkStatusPromise: Promise<void> | null = null;

  constructor() {
    this.checkAuthStatus();
  }

  private checkAuthStatus(): void {
    this.checkStatusPromise = new Promise((resolve) => {
      this.http.get(`${this.apiUrl}/me`, { withCredentials: true }).subscribe({
        next: () => {
          this.authStatus.next(true);
          resolve();
        },
        error: () => {
          this.authStatus.next(false);
          resolve();
        }
      });
    });
  }

  waitForAuthCheck(): Promise<void> {
    return this.checkStatusPromise || Promise.resolve();
  }

  login(credentials: { username: string; password: string }): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/login`, credentials, { withCredentials: true }).pipe(
      tap(() => {
        this.authStatus.next(true);
      })
    );
  }

  logout(): void {
    this.http.post(`${this.apiUrl}/logout`, {}, { withCredentials: true }).subscribe({
      complete: () => {
        this.authStatus.next(false);
      }
    });
  }

  isLoggedIn(): boolean {
    return this.authStatus.value;
  }
}
