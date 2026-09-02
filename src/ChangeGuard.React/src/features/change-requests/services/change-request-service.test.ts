import { beforeEach, describe, expect, it, vi } from 'vitest';
import type { PagedResponse, ChangeRequestSummary } from '../models/change-request-models';
import { changeRequestService } from './change-request-service';

function jsonResponse(body: unknown, status = 200) {
  return new Response(JSON.stringify(body), {
    status,
    headers: { 'Content-Type': 'application/json' },
  });
}

describe('changeRequestService', () => {
  const fetchMock = vi.fn<typeof fetch>();

  beforeEach(() => {
    vi.stubGlobal('fetch', fetchMock);
  });

  it('builds a typed search request with all filters', async () => {
    const page: PagedResponse<ChangeRequestSummary> = {
      items: [],
      page: 2,
      pageSize: 10,
      totalCount: 0,
      totalPages: 0,
    };
    fetchMock.mockResolvedValueOnce(jsonResponse(page));

    const result = await changeRequestService.search({
      search: 'payment',
      priority: 'Critical',
      status: 'QaTesting',
      page: 2,
      pageSize: 10,
    });

    expect(result).toEqual(page);
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/change-requests?page=2&pageSize=10&search=payment&priority=Critical&status=QaTesting',
      expect.objectContaining({ headers: expect.any(Headers) }),
    );
  });

  it('posts a JSON create command to the existing API contract', async () => {
    const request = {
      referenceNumber: 'CG-501',
      title: 'Protect payment validation',
      description: 'Require evidence before release.',
      priority: 'High' as const,
      actor: 'owner@changeguard.local',
    };
    fetchMock.mockResolvedValueOnce(
      jsonResponse({
        ...request,
        id: 'request-501',
        status: 'Draft',
        createdUtc: '2026-09-02T10:00:00Z',
        slaDueUtc: '2026-09-04T10:00:00Z',
      }, 201),
    );

    await changeRequestService.create(request);

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/change-requests',
      expect.objectContaining({ method: 'POST', body: JSON.stringify(request) }),
    );
  });
});
