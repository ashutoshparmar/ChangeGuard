import { config } from '../../../config';
import { apiRequest } from '../../../shared/http/api-client';
import type {
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
} from '../models/change-request-models';

const endpoint = `${config.apiBaseUrl}/change-requests`;

export const changeRequestService = {
  search(filters: SearchChangeRequests = {}, signal?: AbortSignal) {
    const params = new URLSearchParams({
      page: String(filters.page ?? 1),
      pageSize: String(filters.pageSize ?? 20),
    });

    if (filters.search) params.set('search', filters.search);
    if (filters.priority) params.set('priority', filters.priority);
    if (filters.status) params.set('status', filters.status);

    return apiRequest<PagedResponse<ChangeRequestSummary>>(`${endpoint}?${params}`, { signal });
  },

  getDashboard(signal?: AbortSignal) {
    return apiRequest<DashboardResponse>(`${endpoint}/dashboard`, { signal });
  },

  getByReferenceNumber(referenceNumber: string, signal?: AbortSignal) {
    return apiRequest<ChangeRequestDetails>(
      `${endpoint}/${encodeURIComponent(referenceNumber)}`,
      { signal },
    );
  },

  getAudit(referenceNumber: string, signal?: AbortSignal) {
    return apiRequest<ChangeRequestAuditEntry[]>(
      `${endpoint}/${encodeURIComponent(referenceNumber)}/audit`,
      { signal },
    );
  },

  create(request: CreateChangeRequestRequest) {
    return apiRequest<CreateChangeRequestResponse>(endpoint, {
      method: 'POST',
      body: JSON.stringify(request),
    });
  },

  applyWorkflowAction(referenceNumber: string, request: WorkflowActionRequest) {
    return apiRequest<ChangeRequestDetails>(
      `${endpoint}/${encodeURIComponent(referenceNumber)}/workflow`,
      { method: 'POST', body: JSON.stringify(request) },
    );
  },

  recordReleaseArtifacts(referenceNumber: string, request: ReleaseArtifactsRequest) {
    return apiRequest<ChangeRequestDetails>(
      `${endpoint}/${encodeURIComponent(referenceNumber)}/release-artifacts`,
      { method: 'PUT', body: JSON.stringify(request) },
    );
  },
};
