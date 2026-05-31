import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { LoanService } from '../../services/loan.service';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';

interface LoanDisplay {
  loanAmount: number;
  currentBalance: number;
  applicant: string;
  status: string;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, MatTableModule],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
})
export class DashboardComponent implements OnInit, OnDestroy {
  displayedColumns: string[] = [
    'loanAmount',
    'currentBalance',
    'applicant',
    'status',
  ];

  loans: LoanDisplay[] = [];
  isLoading = true;
  error: string | null = null;

  private subscription: Subscription | null = null;

  constructor(
    private loanService: LoanService,
    private authService: AuthService,
    private router: Router
  ) {}

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
      next: (data: any[]) => {
        this.loans = data.map((loan: any) => ({
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
      },
    });
  }

  // ── Computed stats ─────────────────────────────────────────────────────────

  get totalAmount(): number {
    return this.loans.reduce((sum, l) => sum + l.loanAmount, 0);
  }

  get totalBalance(): number {
    return this.loans.reduce((sum, l) => sum + l.currentBalance, 0);
  }

  get activeCount(): number {
    return this.loans.filter((l) =>
      ['active', 'approved'].includes(l.status?.toLowerCase())
    ).length;
  }

  // ── Status badge helper ────────────────────────────────────────────────────

  getStatusClass(status: string): string {
    const s = (status ?? '').toLowerCase();
    if (['active', 'approved'].includes(s)) return 'badge--success';
    if (['pending', 'review', 'processing'].includes(s)) return 'badge--warning';
    if (['defaulted', 'overdue', 'rejected'].includes(s)) return 'badge--danger';
    return 'badge--neutral';
  }

  getInitials(name: string): string {
    return (name ?? '?')
      .split(' ')
      .map((n) => n[0])
      .join('')
      .slice(0, 2)
      .toUpperCase();
  }
}
