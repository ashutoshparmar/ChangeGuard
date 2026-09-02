import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { SystemHealthResponse } from '../models/system-health-response.model';

@Injectable({
  providedIn: 'root',
})
export class SystemHealthService {
  private readonly http = inject(HttpClient);

  private readonly endpoint =
    `${environment.apiBaseUrl}/system/health`;

  getHealth(): Observable<SystemHealthResponse> {
    return this.http.get<SystemHealthResponse>(
      this.endpoint
    );
  }
}
