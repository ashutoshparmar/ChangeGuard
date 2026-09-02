import { config } from '../../config';
import { apiRequest } from '../../shared/http/api-client';
import type { SystemHealthResponse } from '../models/system-health';

export function getSystemHealth(signal?: AbortSignal): Promise<SystemHealthResponse> {
  return apiRequest<SystemHealthResponse>(`${config.apiBaseUrl}/system/health`, { signal });
}
