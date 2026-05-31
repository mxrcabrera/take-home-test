import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, throwError } from 'rxjs';
import { environment } from '../../environments/environment';

export interface Loan {
  id: number;
  amount: number;
  currentBalance: number;
  applicantName: string;
  status: string;
  createdAt: string;
  updatedAt?: string;
}

@Injectable({ providedIn: 'root' })
export class LoanService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiBase}/loans`;

  private handleError(error: any): Observable<never> {
    return throwError(() => error);
  }

  getLoans(): Observable<Loan[]> {
    return this.http.get<Loan[]>(this.apiUrl, { withCredentials: true }).pipe(
      catchError(this.handleError)
    );
  }

  makePayment(id: number, amount: number): Observable<Loan> {
    return this.http.post<Loan>(`${this.apiUrl}/${id}/payment`, { amount }, { withCredentials: true }).pipe(
      catchError(this.handleError)
    );
  }

  createLoan(loan: { amount: number; applicantName: string }): Observable<Loan> {
    return this.http.post<Loan>(this.apiUrl, loan, { withCredentials: true }).pipe(
      catchError(this.handleError)
    );
  }
}