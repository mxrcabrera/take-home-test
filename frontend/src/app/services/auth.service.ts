import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private apiUrl = 'http://localhost:5000/auth';
  
  private authStatus = new BehaviorSubject<boolean>(this.hasToken());
  isLoggedIn$ = this.authStatus.asObservable();

  login(credentials: { username: string; password: string }): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/login`, credentials).pipe(
      tap(response => {
        if (response && response.token) {
          localStorage.setItem('fundo_token', response.token);
          this.authStatus.next(true);
        }
      })
    );
  }

  logout(): void {
    localStorage.removeItem('fundo_token');
    this.authStatus.next(false);
  }

  getToken(): string | null {
    return localStorage.getItem('fundo_token');
  }

  isLoggedIn(): boolean {
    return this.hasToken();
  }

  private hasToken(): boolean {
    return !!localStorage.getItem('fundo_token');
  }
}
