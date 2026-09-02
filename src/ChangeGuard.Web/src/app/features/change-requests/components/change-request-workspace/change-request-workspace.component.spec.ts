import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { vi } from 'vitest';

import { ChangeRequestService } from '../../services/change-request.service';
import { ChangeRequestWorkspaceComponent } from './change-request-workspace.component';

describe('ChangeRequestWorkspaceComponent', () => {
  let fixture: ComponentFixture<ChangeRequestWorkspaceComponent>;

  const summary = {
    id: '0f3a895d-2056-4a87-8623-f142768d3d53',
    referenceNumber: 'CG-501',
    title: 'Improve payment validation',
    priority: 'Critical' as const,
    status: 'QaTesting' as const,
    readinessScore: 45,
    isBlocked: true,
    slaDueUtc: '2026-08-31T14:00:00Z',
    isSlaBreached: false,
    updatedUtc: '2026-08-31T10:00:00Z',
  };

  beforeEach(async () => {
    const service = {
      getDashboard: vi.fn(() =>
        of({
          totalRequests: 1,
          activeRequests: 1,
          blockedRequests: 1,
          slaBreachedRequests: 0,
          byStatus: [{ name: 'QaTesting', count: 1 }],
          byPriority: [{ name: 'Critical', count: 1 }],
          recentRequests: [summary],
        }),
      ),
      search: vi.fn(() =>
        of({
          items: [summary],
          page: 1,
          pageSize: 50,
          totalCount: 1,
          totalPages: 1,
        }),
      ),
      getByReferenceNumber: vi.fn(),
      getAudit: vi.fn(),
      create: vi.fn(),
      applyWorkflowAction: vi.fn(),
      recordReleaseArtifacts: vi.fn(),
    };

    await TestBed.configureTestingModule({
      imports: [ChangeRequestWorkspaceComponent],
      providers: [{ provide: ChangeRequestService, useValue: service }],
    }).compileComponents();

    fixture = TestBed.createComponent(ChangeRequestWorkspaceComponent);
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();
  });

  it('displays dashboard metrics and the filtered request list', () => {
    const element = fixture.nativeElement as HTMLElement;

    expect(element.textContent).toContain('Total requests');
    expect(element.textContent).toContain('CG-501');
    expect(element.textContent).toContain('Improve payment validation');
    expect(element.textContent).toContain('45%');
  });
});
