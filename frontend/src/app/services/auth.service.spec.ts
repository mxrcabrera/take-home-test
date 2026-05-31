/// <reference types="jasmine" />
import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { AuthService } from './auth.service';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
    });
    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
    localStorage.clear();
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('isLoggedIn returns false when no token', () => {
    expect(service.isLoggedIn()).toBeFalse();
    expect(service.getToken()).toBeNull();
  });

  it('login stores token and updates state', () => {
    let receivedToken: any = null;
    service.login({ username: 'admin', password: 'admin123' }).subscribe({
      next: (res) => (receivedToken = res.token),
    });

    const req = httpMock.expectOne('http://localhost:5000/auth/login');
    expect(req.request.method).toBe('POST');
    req.flush({ token: 'fake-jwt-token' });

    expect(receivedToken).toEqual('fake-jwt-token');
    expect(localStorage.getItem('fundo_token')).toBe('fake-jwt-token');
    expect(service.isLoggedIn()).toBeTrue();
  });

  it('logout removes token and updates state', () => {
    localStorage.setItem('fundo_token', 'some-token');
    service['authStatus'].next(true);

    service.logout();

    expect(localStorage.getItem('fundo_token')).toBeNull();
    expect(service.isLoggedIn()).toBeFalse();
  });
});