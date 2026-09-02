import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';
import {
  ChangeRequestAuditEntry,
  ChangeRequestDetails,
  ChangeRequestSummary,
  CreateChangeRequestRequest,
  CreateChangeRequestResponse,
  DashboardResponse,
  PagedResponse,
  ReleaseArtifactsRequest,
  SearchChangeRequests,
  WorkflowActionRequest,
} from '../models/change-request.models';

@Injectable({ providedIn: 'root' })
export class ChangeRequestService {
  private readonly http = inject(HttpClient);
  private readonly endpoint = `${environment.apiBaseUrl}/change-requests`;

  search(filters: SearchChangeRequests = {}): Observable<PagedResponse<ChangeRequestSummary>> {
    let params = new HttpParams()
      .set('page', filters.page ?? 1)
      .set('pageSize', filters.pageSize ?? 20);

    if (filters.search) {
      params = params.set('search', filters.search);
    }
    if (filters.priority) {
      params = params.set('priority', filters.priority);
    }
    if (filters.status) {
      params = params.set('status', filters.status);
    }

    return this.http.get<PagedResponse<ChangeRequestSummary>>(this.endpoint, { params });
  }

  getDashboard(): Observable<DashboardResponse> {
    return this.http.get<DashboardResponse>(`${this.endpoint}/dashboard`);
  }

  getByReferenceNumber(referenceNumber: string): Observable<ChangeRequestDetails> {
    return this.http.get<ChangeRequestDetails>(
      `${this.endpoint}/${encodeURIComponent(referenceNumber)}`,
    );
  }

  getAudit(referenceNumber: string): Observable<ChangeRequestAuditEntry[]> {
    return this.http.get<ChangeRequestAuditEntry[]>(
      `${this.endpoint}/${encodeURIComponent(referenceNumber)}/audit`,
    );
  }

  create(request: CreateChangeRequestRequest): Observable<CreateChangeRequestResponse> {
    return this.http.post<CreateChangeRequestResponse>(this.endpoint, request);
  }

  applyWorkflowAction(
    referenceNumber: string,
    request: WorkflowActionRequest,
  ): Observable<ChangeRequestDetails> {
    return this.http.post<ChangeRequestDetails>(
      `${this.endpoint}/${encodeURIComponent(referenceNumber)}/workflow`,
      request,
    );
  }

  recordReleaseArtifacts(
    referenceNumber: string,
    request: ReleaseArtifactsRequest,
  ): Observable<ChangeRequestDetails> {
    return this.http.put<ChangeRequestDetails>(
      `${this.endpoint}/${encodeURIComponent(referenceNumber)}/release-artifacts`,
      request,
    );
  }
}
