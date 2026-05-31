import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { authGuard, loginGuard } from './auth.guard';

describe('authGuard', () => {
  let authService: jasmine.SpyObj<AuthService>;
  let router: jasmine.SpyObj<Router>;

  beforeEach(() => {
    authService = jasmine.createSpyObj('AuthService', ['isLoggedIn', 'waitForAuthCheck']);
    router = jasmine.createSpyObj('Router', ['createUrlTree']);

    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: authService },
        { provide: Router, useValue: router },
      ],
    });
  });

  it('should return true when user is logged in', async () => {
    authService.isLoggedIn.and.returnValue(true);
    authService.waitForAuthCheck.and.returnValue(Promise.resolve());

    const route = {} as ActivatedRouteSnapshot;
    const state = {} as RouterStateSnapshot;
    const result = await TestBed.runInInjectionContext(() => authGuard(route, state));

    expect(result).toBeTrue();
    expect(router.createUrlTree).not.toHaveBeenCalled();
  });

  it('should redirect to login when user is not logged in', async () => {
    authService.isLoggedIn.and.returnValue(false);
    authService.waitForAuthCheck.and.returnValue(Promise.resolve());

    const route = {} as ActivatedRouteSnapshot;
    const state = {} as RouterStateSnapshot;
    const result = await TestBed.runInInjectionContext(() => authGuard(route, state));

    expect(result).toEqual(router.createUrlTree(['/login']));
    expect(router.createUrlTree).toHaveBeenCalledWith(['/login']);
  });

  it('should wait for auth check before deciding', async () => {
    authService.isLoggedIn.and.returnValue(true);
    const authCheckPromise = Promise.resolve();
    authService.waitForAuthCheck.and.returnValue(authCheckPromise);

    const route = {} as ActivatedRouteSnapshot;
    const state = {} as RouterStateSnapshot;
    const result = await TestBed.runInInjectionContext(() => authGuard(route, state));

    expect(authService.waitForAuthCheck).toHaveBeenCalled();
    expect(result).toBeTrue();
  });
});

describe('loginGuard', () => {
  let authService: jasmine.SpyObj<AuthService>;
  let router: jasmine.SpyObj<Router>;

  beforeEach(() => {
    authService = jasmine.createSpyObj('AuthService', ['isLoggedIn', 'waitForAuthCheck']);
    router = jasmine.createSpyObj('Router', ['createUrlTree']);

    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: authService },
        { provide: Router, useValue: router },
      ],
    });
  });

  it('should redirect to dashboard when user is logged in', async () => {
    authService.isLoggedIn.and.returnValue(true);
    authService.waitForAuthCheck.and.returnValue(Promise.resolve());

    const route = {} as ActivatedRouteSnapshot;
    const state = {} as RouterStateSnapshot;
    const result = await TestBed.runInInjectionContext(() => loginGuard(route, state));

    expect(result).toEqual(router.createUrlTree(['/dashboard']));
    expect(router.createUrlTree).toHaveBeenCalledWith(['/dashboard']);
  });

  it('should return true when user is not logged in', async () => {
    authService.isLoggedIn.and.returnValue(false);
    authService.waitForAuthCheck.and.returnValue(Promise.resolve());

    const route = {} as ActivatedRouteSnapshot;
    const state = {} as RouterStateSnapshot;
    const result = await TestBed.runInInjectionContext(() => loginGuard(route, state));

    expect(result).toBeTrue();
    expect(router.createUrlTree).not.toHaveBeenCalled();
  });

  it('should wait for auth check before deciding', async () => {
    authService.isLoggedIn.and.returnValue(false);
    const authCheckPromise = Promise.resolve();
    authService.waitForAuthCheck.and.returnValue(authCheckPromise);

    const route = {} as ActivatedRouteSnapshot;
    const state = {} as RouterStateSnapshot;
    const result = await TestBed.runInInjectionContext(() => loginGuard(route, state));

    expect(authService.waitForAuthCheck).toHaveBeenCalled();
    expect(result).toBeTrue();
  });
});
