export type ChangePriority = 'Low' | 'Medium' | 'High' | 'Critical';

export type ChangeRequestStatus =
  | 'Draft'
  | 'RequirementReview'
  | 'InDevelopment'
  | 'QaTesting'
  | 'ReleaseApproval'
  | 'Released'
  | 'Rejected'
  | 'Closed';

export type WorkflowAction =
  | 'SubmitForReview'
  | 'StartDevelopment'
  | 'StartQaTesting'
  | 'CompleteQaTesting'
  | 'ApproveRelease'
  | 'Reject'
  | 'Close';

export interface ChangeRequestSummary {
  id: string;
  referenceNumber: string;
  title: string;
  priority: ChangePriority;
  status: ChangeRequestStatus;
  readinessScore: number;
  isBlocked: boolean;
  slaDueUtc: string;
  isSlaBreached: boolean;
  updatedUtc: string;
}

export interface ChangeRequestDetails extends ChangeRequestSummary {
  description: string;
  hasQaEvidence: boolean;
  qaEvidenceNotes: string | null;
  hasRollbackPlan: boolean;
  rollbackPlan: string | null;
  canMoveToReleaseApproval: boolean;
  blockers: string[];
  createdUtc: string;
  remainingSlaHours: number;
}

export interface PagedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface BreakdownItem {
  name: string;
  count: number;
}

export interface DashboardResponse {
  totalRequests: number;
  activeRequests: number;
  blockedRequests: number;
  slaBreachedRequests: number;
  byStatus: BreakdownItem[];
  byPriority: BreakdownItem[];
  recentRequests: ChangeRequestSummary[];
}

export interface ChangeRequestAuditEntry {
  id: string;
  action: string;
  actor: string;
  comment: string;
  fromStatus: ChangeRequestStatus | null;
  toStatus: ChangeRequestStatus;
  occurredUtc: string;
}

export interface CreateChangeRequestRequest {
  referenceNumber: string;
  title: string;
  description: string;
  priority: ChangePriority;
  actor: string;
}

export interface CreateChangeRequestResponse {
  id: string;
  referenceNumber: string;
  title: string;
  description: string;
  priority: ChangePriority;
  status: ChangeRequestStatus;
  createdUtc: string;
  slaDueUtc: string;
}

export interface SearchChangeRequests {
  search?: string;
  priority?: ChangePriority;
  status?: ChangeRequestStatus;
  page?: number;
  pageSize?: number;
}

export interface WorkflowActionRequest {
  action: WorkflowAction;
  actor: string;
  comment: string | null;
}

export interface ReleaseArtifactsRequest {
  qaEvidenceNotes: string | null;
  rollbackPlan: string | null;
  actor: string;
}
