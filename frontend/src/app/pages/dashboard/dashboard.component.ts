import { Component, OnInit, OnDestroy, inject, ViewChild, TemplateRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { LoanService, Loan } from '../../services/loan.service';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { LoanConstants } from '../../constants/loan.constants';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, MatTableModule, FormsModule, MatDialogModule],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit, OnDestroy {
  private readonly loanService = inject(LoanService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly dialog = inject(MatDialog);

  @ViewChild('paymentDialog') paymentDialog!: TemplateRef<any>;

  displayedColumns: string[] = [
    'loanAmount',
    'currentBalance',
    'applicant',
    'status',
    'actions'
  ];

  loans: Loan[] = [];
  isLoading = true;
  error: string | null = null;
  processingLoanId: number | null = null;
  paymentAmount: number = 0;
  selectedLoan: Loan | null = null;

  private subscription: Subscription | null = null;

  constructor() {}

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

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
      next: (data: Loan[]) => {
        this.loans = data;
        this.isLoading = false;
      },
      error: (err: Error) => {
        this.error = 'Unable to load loans. Please try again later.';
        this.isLoading = false;
      },
    });
  }



  makePayment(loan: Loan): void {
    this.selectedLoan = loan;
    this.paymentAmount = 0;

    const dialogRef = this.dialog.open(this.paymentDialog, { 
      width: '320px',
      panelClass: 'custom-loan-dialog'
    });

    dialogRef.afterClosed().subscribe(amount => {
      if (amount === undefined) return;

      if (isNaN(amount) || amount <= 0 || amount > loan.currentBalance) {
        alert('Invalid amount.');
        return;
      }

      this.processingLoanId = loan.id;

      this.loanService.makePayment(loan.id, amount).subscribe({
        next: () => {
          this.loadLoans();
          this.processingLoanId = null; 
          alert('Payment processed successfully.');
        },
        error: (err) => {
          this.processingLoanId = null;
          alert(err.status === 409 ? 'Data conflict. Please refresh the page.' : 'Payment failed.');
        }
      });
    });
  }

  // ── Computed stats ─────────────────────────────────────────────────────────

  get totalAmount(): number {
    return this.loans.reduce((sum, l) => sum + l.amount, 0);
  }

  get totalBalance(): number {
    return this.loans.reduce((sum, l) => sum + l.currentBalance, 0);
  }

  get activeCount(): number {
    return this.loans.filter((l) =>
      [LoanConstants.StatusActive, LoanConstants.StatusApproved].includes(l.status?.toLowerCase())
    ).length;
  }

  // ── Status badge helper ────────────────────────────────────────────────────

  getStatusClass(status?: string): string {
    const s = (status ?? '').toLowerCase();
    if ([LoanConstants.StatusActive, LoanConstants.StatusApproved].includes(s)) return 'badge--success';
    if ([LoanConstants.StatusPending, LoanConstants.StatusReview, LoanConstants.StatusProcessing].includes(s)) return 'badge--warning';
    if ([LoanConstants.StatusDefaulted, LoanConstants.StatusOverdue, LoanConstants.StatusRejected].includes(s)) return 'badge--danger';
    return 'badge--neutral';
  }

  getInitials(name?: string): string {
    const safeName = name || '?';
    return safeName
      .split(' ')
      .map((n) => n[0])
      .join('')
      .slice(0, 2)
      .toUpperCase();
  }
}
