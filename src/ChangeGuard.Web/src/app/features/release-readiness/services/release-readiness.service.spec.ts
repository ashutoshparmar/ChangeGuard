import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { environment } from '../../../../environments/environment';
import { ReleaseReadinessResponse } from '../models/release-readiness-response.model';
import { ReleaseReadinessService } from './release-readiness.service';

describe('ReleaseReadinessService', () => {
  let service: ReleaseReadinessService;
  let httpController: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        ReleaseReadinessService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });

    service = TestBed.inject(ReleaseReadinessService);
    httpController = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpController.verify();
  });

  it('requests release readiness for CG-101', () => {
    const expectedResponse: ReleaseReadinessResponse = {
      referenceNumber: 'CG-101',
      title: 'Payment validation update',
      priority: 'Critical',
      status: 'QaTesting',
      score: 45,
      isBlocked: true,
      canMoveToReleaseApproval: false,
      blockers: [
        'QA evidence is missing.',
        'Rollback plan is missing.'
      ]
    };

    service
      .getReleaseReadiness()
      .subscribe((response) => {
        expect(response).toEqual(expectedResponse);
      });

    const request = httpController.expectOne(
      `${environment.apiBaseUrl}/change-requests/CG-101/release-readiness`
    );

    expect(request.request.method).toBe('GET');

    request.flush(expectedResponse);
  });
});
