import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { environment } from '../../../environments/environment';
import { SystemHealthResponse } from '../models/system-health-response.model';
import { SystemHealthService } from './system-health.service';

describe('SystemHealthService', () => {
  let service: SystemHealthService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        SystemHealthService,
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });

    service = TestBed.inject(SystemHealthService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('should request system health using GET', () => {
    const expectedResponse: SystemHealthResponse = {
      status: 'Healthy',
      service: 'ChangeGuard.Api',
      version: '1.0.0',
      timestampUtc: '2026-08-17T14:03:42+00:00',
    };

    service.getHealth().subscribe((response) => {
      expect(response).toEqual(expectedResponse);
    });

    const request = httpTesting.expectOne(
      `${environment.apiBaseUrl}/system/health`
    );

    expect(request.request.method).toBe('GET');

    request.flush(expectedResponse);
  });
});
