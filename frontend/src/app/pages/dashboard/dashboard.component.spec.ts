import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter, Router } from '@angular/router';
import { DashboardComponent } from './dashboard.component';
import { of, throwError } from 'rxjs';
import { LoanService } from '../../services/loan.service';
import { AuthService } from '../../services/auth.service';

describe('DashboardComponent', () => {
  let component: DashboardComponent;
  let fixture: ComponentFixture<DashboardComponent>;
  let loanService: jasmine.SpyObj<LoanService>;
  let authService: jasmine.SpyObj<AuthService>;

  const mockLoans = [
    { id: 1, amount: 25000, currentBalance: 18750, applicantName: 'John Doe', status: 'active', createdAt: '2024-01-15' },
    { id: 2, amount: 15000, currentBalance: 0, applicantName: 'Jane Smith', status: 'paid', createdAt: '2024-01-20' },
    { id: 3, amount: 50000, currentBalance: 32500, applicantName: 'Robert Johnson', status: 'active', createdAt: '2024-02-01' },
  ];

  beforeEach(async () => {
    loanService = jasmine.createSpyObj('LoanService', ['getLoans']);
    authService = jasmine.createSpyObj('AuthService', ['logout']);

    await TestBed.configureTestingModule({
      imports: [DashboardComponent],
      providers: [
        provideHttpClient(),
        provideRouter([]),
        { provide: LoanService, useValue: loanService },
        { provide: AuthService, useValue: authService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(DashboardComponent);
    component = fixture.componentInstance;
  });

  it('should load loans on init', () => {
    loanService.getLoans.and.returnValue(of(mockLoans));
    fixture.detectChanges();
    expect(component.loans.length).toBe(3);
    expect(component.isLoading).toBeFalse();
  });

  it('should show loading state initially', () => {
    loanService.getLoans.and.returnValue(of(mockLoans));
    expect(component.isLoading).toBeTrue();
    fixture.detectChanges();
    expect(component.isLoading).toBeFalse();
  });

  it('should handle error state', () => {
    loanService.getLoans.and.returnValue(throwError(() => new Error('fail')));
    fixture.detectChanges();
    expect(component.error).toBe('Failed to load loans. Please try again later.');
    expect(component.isLoading).toBeFalse();
  });

  it('should compute totalAmount correctly', () => {
    loanService.getLoans.and.returnValue(of(mockLoans));
    fixture.detectChanges();
    expect(component.totalAmount).toBe(90000);
  });

  it('should compute totalBalance correctly', () => {
    loanService.getLoans.and.returnValue(of(mockLoans));
    fixture.detectChanges();
    expect(component.totalBalance).toBe(51250);
  });

  it('should compute activeCount correctly', () => {
    loanService.getLoans.and.returnValue(of(mockLoans));
    fixture.detectChanges();
    expect(component.activeCount).toBe(2);
  });

  it('getStatusClass returns badge--success for active', () => {
    expect(component.getStatusClass('active')).toBe('badge--success');
    expect(component.getStatusClass('approved')).toBe('badge--success');
  });

  it('getStatusClass returns badge--warning for pending', () => {
    expect(component.getStatusClass('pending')).toBe('badge--warning');
    expect(component.getStatusClass('review')).toBe('badge--warning');
  });

  it('getStatusClass returns badge--danger for defaulted', () => {
    expect(component.getStatusClass('defaulted')).toBe('badge--danger');
    expect(component.getStatusClass('rejected')).toBe('badge--danger');
  });

  it('getStatusClass returns badge--neutral for unknown', () => {
    expect(component.getStatusClass('unknown')).toBe('badge--neutral');
    expect(component.getStatusClass('')).toBe('badge--neutral');
  });

  it('getInitials extracts first letters', () => {
    expect(component.getInitials('John Doe')).toBe('JD');
    expect(component.getInitials('Maria Silva')).toBe('MS');
    expect(component.getInitials('')).toBe('?');
  });

  it('logout calls authService.logout and navigates to login', () => {
    const router = TestBed.inject(Router);
    spyOn(router, 'navigate');
    component.logout();
    expect(authService.logout).toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledWith(['/login']);
  });
});