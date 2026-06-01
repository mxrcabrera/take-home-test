/// <reference types="jasmine" />
import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';

describe('AuthService', () => {
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClientTesting()],
    });
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    const service = TestBed.inject(AuthService);
    httpMock.expectOne(`${environment.apiBase}/auth/me`).flush(null, { status: 401, statusText: 'Unauthorized' });
    expect(service).toBeTruthy();
  });

  it('checkAuthStatus sets authStatus to true when /me succeeds', async () => {
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [provideHttpClientTesting()],
    });
    const testService = TestBed.inject(AuthService);
    const testHttpMock = TestBed.inject(HttpTestingController);
    
    testHttpMock.expectOne(`${environment.apiBase}/auth/me`).flush({ username: 'admin' });
    
    await testService.waitForAuthCheck();
    expect(testService.isLoggedIn()).toBeTrue();
    
    testHttpMock.verify();
  });

  it('checkAuthStatus sets authStatus to false when /me fails', async () => {
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [provideHttpClientTesting()],
    });
    const testService = TestBed.inject(AuthService);
    const testHttpMock = TestBed.inject(HttpTestingController);
    
    testHttpMock.expectOne(`${environment.apiBase}/auth/me`).flush(null, { status: 401, statusText: 'Unauthorized' });
    
    await testService.waitForAuthCheck();
    expect(testService.isLoggedIn()).toBeFalse();
    
    testHttpMock.verify();
  });

  it('login updates state on success with cookies', async () => {
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [provideHttpClientTesting()],
    });
    const service = TestBed.inject(AuthService);
    const testHttpMock = TestBed.inject(HttpTestingController);
    
    testHttpMock.expectOne(`${environment.apiBase}/auth/me`).flush(null, { status: 401, statusText: 'Unauthorized' });
    await service.waitForAuthCheck();

    service.login({ username: 'admin', password: 'admin123' }).subscribe({
      next: (res) => {
        expect(res).toBeDefined();
      },
    });

    const req = testHttpMock.expectOne(`${environment.apiBase}/auth/login`);
    expect(req.request.method).toBe('POST');
    expect(req.request.withCredentials).toBeTrue();
    req.flush({ token: 'fake-jwt-token' });

    expect(service.isLoggedIn()).toBeTrue();
    
    testHttpMock.verify();
  });

  it('logout updates state with cookies', async () => {
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [provideHttpClientTesting()],
    });
    const service = TestBed.inject(AuthService);
    const testHttpMock = TestBed.inject(HttpTestingController);
    
    testHttpMock.expectOne(`${environment.apiBase}/auth/me`).flush(null, { status: 401, statusText: 'Unauthorized' });
    await service.waitForAuthCheck();

    service['authStatus'].next(true);

    service.logout();

    const req = testHttpMock.expectOne(`${environment.apiBase}/auth/logout`);
    expect(req.request.method).toBe('POST');
    expect(req.request.withCredentials).toBeTrue();
    req.flush({});

    expect(service.isLoggedIn()).toBeFalse();
    
    testHttpMock.verify();
  });
});