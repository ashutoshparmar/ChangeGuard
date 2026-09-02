import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { environment } from '../../../../environments/environment';
import { ChangeRequestService } from './change-request.service';

describe('ChangeRequestService', () => {
  let service: ChangeRequestService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [ChangeRequestService, provideHttpClient(), provideHttpClientTesting()],
    });

    service = TestBed.inject(ChangeRequestService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpTesting.verify());

  it('searches with typed filters and paging', () => {
    service
      .search({
        search: 'payment',
        priority: 'Critical',
        status: 'QaTesting',
        page: 2,
        pageSize: 10,
      })
      .subscribe();

    const request = httpTesting.expectOne(
      (candidate) =>
        candidate.url === `${environment.apiBaseUrl}/change-requests` &&
        candidate.params.get('search') === 'payment' &&
        candidate.params.get('priority') === 'Critical' &&
        candidate.params.get('status') === 'QaTesting' &&
        candidate.params.get('page') === '2' &&
        candidate.params.get('pageSize') === '10',
    );

    expect(request.request.method).toBe('GET');
    request.flush({
      items: [],
      page: 2,
      pageSize: 10,
      totalCount: 0,
      totalPages: 0,
    });
  });

  it('creates a change request using POST', () => {
    const payload = {
      referenceNumber: 'CG-501',
      title: 'Improve payment validation',
      description: 'Prevent duplicate settlements.',
      priority: 'High' as const,
      actor: 'product-owner@changeguard.local',
    };

    service.create(payload).subscribe();

    const request = httpTesting.expectOne(`${environment.apiBaseUrl}/change-requests`);
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(payload);
    request.flush({
      id: '0f3a895d-2056-4a87-8623-f142768d3d53',
      ...payload,
      status: 'Draft',
      createdUtc: '2026-08-31T10:00:00Z',
      slaDueUtc: '2026-09-01T10:00:00Z',
    });
  });
});
