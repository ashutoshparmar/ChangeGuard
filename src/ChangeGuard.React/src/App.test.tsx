import { render, screen } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import App from './App';
import { getSystemHealth } from './core/services/system-health-service';

vi.mock('./core/services/system-health-service', () => ({
  getSystemHealth: vi.fn(),
}));

vi.mock('./features/change-requests/components/ChangeRequestWorkspace', () => ({
  default: () => <section aria-label="Change workspace">Workspace</section>,
}));

describe('App', () => {
  beforeEach(() => {
    vi.mocked(getSystemHealth).mockResolvedValue({
      status: 'Healthy',
      service: 'ChangeGuard.Api',
      version: '1.0.0',
      timestampUtc: '2026-09-02T10:00:00Z',
    });
  });

  it('shows the API health and the React workspace', async () => {
    render(<App />);

    expect(screen.getByRole('heading', { name: /ship change with evidence/i })).toBeInTheDocument();
    expect(await screen.findByTestId('health-status')).toHaveTextContent('Healthy');
    expect(screen.getByLabelText('Change workspace')).toBeInTheDocument();
    expect(screen.getByText('ChangeGuard.Api v1.0.0')).toBeInTheDocument();
  });

  it('shows a clear offline state when health loading fails', async () => {
    vi.mocked(getSystemHealth).mockRejectedValueOnce(new Error('offline'));

    render(<App />);

    expect(await screen.findByTestId('health-status')).toHaveTextContent('API offline');
  });
});
