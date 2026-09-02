export interface ReleaseReadinessResponse {
  referenceNumber: string;
  title: string;
  priority: string;
  status: string;
  score: number;
  isBlocked: boolean;
  canMoveToReleaseApproval: boolean;
  blockers: string[];
}
