import { TestBed } from '@angular/core/testing';
import { HttpRequest, HttpHandlerFn } from '@angular/common/http';
import { of, throwError } from 'rxjs';
import { authInterceptor } from './auth.interceptor';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';

describe('authInterceptor', () => {
  let mockNext: HttpHandlerFn;
  let mockRequest: HttpRequest<any>;
  let authService: jasmine.SpyObj<AuthService>;
  let router: jasmine.SpyObj<Router>;

  beforeEach(() => {
    mockNext = jasmine.createSpy('handle').and.returnValue(of({})) as HttpHandlerFn;
    mockRequest = new HttpRequest('GET', '/api/test');
    authService = jasmine.createSpyObj('AuthService', ['logout']);
    router = jasmine.createSpyObj('Router', ['navigate']);

    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: authService },
        { provide: Router, useValue: router },
      ],
    });
  });

  it('should add withCredentials: true to request', (done) => {
    const interceptor = TestBed.runInInjectionContext(() => authInterceptor(mockRequest, mockNext));

    interceptor.subscribe(() => {
      expect(mockNext).toHaveBeenCalled();
      const handledRequest = (mockNext as jasmine.Spy).calls.mostRecent().args[0] as HttpRequest<any>;
      expect(handledRequest.withCredentials).toBeTrue();
      done();
    });
  });

  it('should pass through the request to next handler', (done) => {
    const interceptor = TestBed.runInInjectionContext(() => authInterceptor(mockRequest, mockNext));

    interceptor.subscribe(() => {
      expect(mockNext).toHaveBeenCalledWith(jasmine.objectContaining({
        url: '/api/test',
        method: 'GET',
      }));
      done();
    });
  });

  it('should preserve other request properties', (done) => {
    const requestWithHeaders = mockRequest.clone({
      headers: mockRequest.headers.set('Content-Type', 'application/json'),
    });

    const interceptor = TestBed.runInInjectionContext(() => authInterceptor(requestWithHeaders, mockNext));

    interceptor.subscribe(() => {
      const handledRequest = (mockNext as jasmine.Spy).calls.mostRecent().args[0] as HttpRequest<any>;
      expect(handledRequest.headers.get('Content-Type')).toBe('application/json');
      expect(handledRequest.withCredentials).toBeTrue();
      done();
    });
  });

  it('should handle errors from next handler', (done) => {
    const errorResponse = new Error('Network error');
    (mockNext as jasmine.Spy).and.returnValue(throwError(() => errorResponse));

    const interceptor = TestBed.runInInjectionContext(() => authInterceptor(mockRequest, mockNext));

    interceptor.subscribe({
      error: (err) => {
        expect(err).toBe(errorResponse);
        done();
      },
    });
  });

  it('should call authService.logout and navigate to login on 401 error', (done) => {
    const httpErrorResponse = { status: 401, statusText: 'Unauthorized' } as any;
    (mockNext as jasmine.Spy).and.returnValue(throwError(() => httpErrorResponse));

    const interceptor = TestBed.runInInjectionContext(() => authInterceptor(mockRequest, mockNext));

    interceptor.subscribe({
      error: (err) => {
        expect(authService.logout).toHaveBeenCalled();
        expect(router.navigate).toHaveBeenCalledWith(['/login']);
        done();
      },
    });
  });
});
