import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';
import { ReleaseReadinessResponse } from '../models/release-readiness-response.model';

@Injectable({
  providedIn: 'root',
})
export class ReleaseReadinessService {
  private readonly http = inject(HttpClient);

  getReleaseReadiness(referenceNumber = 'CG-101'): Observable<ReleaseReadinessResponse> {
    return this.http.get<ReleaseReadinessResponse>(
      `${environment.apiBaseUrl}/change-requests/${encodeURIComponent(referenceNumber)}/release-readiness`,
    );
  }
}
