import { Component, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnDestroy {
  private static readonly HTTP_STATUS_UNAUTHORIZED = 401;
  private static readonly HTTP_STATUS_TOO_MANY_REQUESTS = 429;

  username = '';
  password = '';
  error = '';
  isLoading = false;
  showPassword = false;

  private readonly destroy$ = new Subject<void>();
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  togglePasswordVisibility() {
    this.showPassword = !this.showPassword;
  }

  onSubmit() {
    this.isLoading = true;
    this.error = '';

    this.authService.login({ username: this.username, password: this.password }).pipe(
      takeUntil(this.destroy$)
    ).subscribe({
      next: () => {
        this.isLoading = false;
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        if (err.status === LoginComponent.HTTP_STATUS_UNAUTHORIZED) {
          this.error = 'Invalid username or password';
        } else if (err.status === LoginComponent.HTTP_STATUS_TOO_MANY_REQUESTS) {
          this.error = 'Too many login attempts. Please try again later.';
        } else {
          this.error = 'An error occurred. Please try again.';
        }
        this.isLoading = false;
      }
    });
  }
}
