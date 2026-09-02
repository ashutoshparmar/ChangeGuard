import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import type {
  ChangeRequestDetails,
  ChangeRequestSummary,
  DashboardResponse,
} from '../models/change-request-models';
import { changeRequestService } from '../services/change-request-service';
import ChangeRequestWorkspace from './ChangeRequestWorkspace';

vi.mock('../services/change-request-service', () => ({
  changeRequestService: {
    search: vi.fn(),
    getDashboard: vi.fn(),
    getByReferenceNumber: vi.fn(),
    getAudit: vi.fn(),
    create: vi.fn(),
    applyWorkflowAction: vi.fn(),
    recordReleaseArtifacts: vi.fn(),
  },
}));

const request: ChangeRequestSummary = {
  id: 'request-101',
  referenceNumber: 'CG-101',
  title: 'Payment validation update',
  priority: 'Critical',
  status: 'QaTesting',
  readinessScore: 45,
  isBlocked: true,
  slaDueUtc: '2026-09-03T10:00:00Z',
  isSlaBreached: false,
  updatedUtc: '2026-09-02T10:00:00Z',
};

const dashboard: DashboardResponse = {
  totalRequests: 3,
  activeRequests: 2,
  blockedRequests: 1,
  slaBreachedRequests: 0,
  byStatus: [],
  byPriority: [],
  recentRequests: [request],
};

const details: ChangeRequestDetails = {
  ...request,
  description: 'Validate card details before release.',
  hasQaEvidence: false,
  qaEvidenceNotes: null,
  hasRollbackPlan: false,
  rollbackPlan: null,
  canMoveToReleaseApproval: false,
  blockers: ['QA evidence is missing.', 'Rollback plan is missing.'],
  createdUtc: '2026-09-01T10:00:00Z',
  remainingSlaHours: 24,
};

describe('ChangeRequestWorkspace', () => {
  beforeEach(() => {
    vi.mocked(changeRequestService.getDashboard).mockResolvedValue(dashboard);
    vi.mocked(changeRequestService.search).mockResolvedValue({
      items: [request],
      page: 1,
      pageSize: 50,
      totalCount: 1,
      totalPages: 1,
    });
    vi.mocked(changeRequestService.getByReferenceNumber).mockResolvedValue(details);
    vi.mocked(changeRequestService.getAudit).mockResolvedValue([]);
  });

  it('loads the dashboard and searchable request list', async () => {
    render(<ChangeRequestWorkspace />);

    expect(await screen.findByText('Payment validation update')).toBeInTheDocument();
    expect(screen.getByText('3')).toBeInTheDocument();
    expect(screen.getByText('Total requests')).toBeInTheDocument();
    expect(screen.getByText('45%')).toBeInTheDocument();
  });

  it('opens persisted request details with release blockers', async () => {
    const user = userEvent.setup();
    render(<ChangeRequestWorkspace />);

    await user.click(await screen.findByRole('button', { name: /payment validation update/i }));

    expect(await screen.findByRole('dialog')).toBeInTheDocument();
    expect(screen.getByText('QA evidence is missing.')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Complete QA testing' })).toBeInTheDocument();
  });

  it('protects the create endpoint with client-side business input validation', async () => {
    const user = userEvent.setup();
    render(<ChangeRequestWorkspace />);

    await user.click(screen.getByRole('button', { name: '+ New change request' }));
    await user.clear(screen.getByLabelText('Reference number'));
    await user.clear(screen.getByLabelText('Title'));
    await user.click(screen.getByRole('button', { name: 'Create draft' }));

    expect(screen.getByText('Use CG- followed by 3 to 8 digits.')).toBeInTheDocument();
    expect(screen.getByText('Enter a meaningful title of at least 5 characters.')).toBeInTheDocument();
    expect(changeRequestService.create).not.toHaveBeenCalled();
  });
});
