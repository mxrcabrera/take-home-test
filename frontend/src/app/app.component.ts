import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { LoanService, Loan } from './services/loan.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatButtonModule],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
})
export class AppComponent implements OnInit, OnDestroy {
  displayedColumns: string[] = [
    'loanAmount',
    'currentBalance',
    'applicant',
    'status',
  ];
  loans: Loan[] = [];
  isLoading = true;
  error: string | null = null;
  private subscription: Subscription | null = null;

  constructor(private loanService: LoanService) {}

  ngOnInit(): void {
    this.loadLoans();
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
  }

  loadLoans(): void {
    this.isLoading = true;
    this.error = null;
    this.subscription = this.loanService.getLoans().subscribe({
      next: (data) => {
        this.loans = data.map(loan => ({
          loanAmount: loan.amount,
          currentBalance: loan.currentBalance,
          applicant: loan.applicantName,
          status: loan.status,
        }));
        this.isLoading = false;
      },
      error: () => {
        this.error = 'Failed to load loans. Please try again later.';
        this.isLoading = false;
      }
    });
  }
}
