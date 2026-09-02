import { DatePipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { forkJoin } from 'rxjs';

import {
  ChangePriority,
  ChangeRequestAuditEntry,
  ChangeRequestDetails,
  ChangeRequestStatus,
  ChangeRequestSummary,
  DashboardResponse,
  WorkflowAction,
} from '../../models/change-request.models';
import { ChangeRequestService } from '../../services/change-request.service';

interface WorkflowOption {
  action: WorkflowAction;
  label: string;
}

@Component({
  selector: 'cg-change-request-workspace',
  imports: [DatePipe, DecimalPipe, ReactiveFormsModule],
  templateUrl: './change-request-workspace.component.html',
  styleUrl: './change-request-workspace.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ChangeRequestWorkspaceComponent implements OnInit {
  private readonly service = inject(ChangeRequestService);
  private readonly formBuilder = inject(FormBuilder);

  protected readonly priorities: ChangePriority[] = ['Low', 'Medium', 'High', 'Critical'];
  protected readonly statuses: ChangeRequestStatus[] = [
    'Draft',
    'RequirementReview',
    'InDevelopment',
    'QaTesting',
    'ReleaseApproval',
    'Released',
    'Rejected',
    'Closed',
  ];

  protected readonly dashboard = signal<DashboardResponse | null>(null);
  protected readonly requests = signal<ChangeRequestSummary[]>([]);
  protected readonly totalCount = signal(0);
  protected readonly selected = signal<ChangeRequestDetails | null>(null);
  protected readonly audit = signal<ChangeRequestAuditEntry[]>([]);
  protected readonly isLoading = signal(true);
  protected readonly isSaving = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly successMessage = signal<string | null>(null);
  protected readonly showCreateForm = signal(false);

  protected readonly filterForm = this.formBuilder.nonNullable.group({
    search: [''],
    priority: [''],
    status: [''],
  });

  protected readonly createForm = this.formBuilder.nonNullable.group({
    referenceNumber: ['', [Validators.required, Validators.pattern(/^CG-[0-9]{3,8}$/)]],
    title: ['', [Validators.required, Validators.minLength(5)]],
    description: ['', [Validators.maxLength(4000)]],
    priority: ['Medium' as ChangePriority, Validators.required],
    actor: ['product-owner@changeguard.local', Validators.required],
  });

  protected readonly artifactForm = this.formBuilder.nonNullable.group({
    qaEvidenceNotes: [''],
    rollbackPlan: [''],
    actor: ['qa@changeguard.local', Validators.required],
  });

  ngOnInit(): void {
    this.loadOverview();
  }

  protected loadOverview(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);
    const raw = this.filterForm.getRawValue();

    forkJoin({
      dashboard: this.service.getDashboard(),
      page: this.service.search({
        search: raw.search.trim() || undefined,
        priority: (raw.priority || undefined) as ChangePriority | undefined,
        status: (raw.status || undefined) as ChangeRequestStatus | undefined,
        page: 1,
        pageSize: 50,
      }),
    }).subscribe({
      next: ({ dashboard, page }) => {
        this.dashboard.set(dashboard);
        this.requests.set(page.items);
        this.totalCount.set(page.totalCount);
        this.isLoading.set(false);
      },
      error: (error: unknown) => {
        this.isLoading.set(false);
        this.errorMessage.set(this.readError(error));
      },
    });
  }

  protected clearFilters(): void {
    this.filterForm.reset({ search: '', priority: '', status: '' });
    this.loadOverview();
  }

  protected toggleCreateForm(): void {
    this.showCreateForm.update((visible) => !visible);
    this.clearMessages();
  }

  protected createRequest(): void {
    if (this.createForm.invalid) {
      this.createForm.markAllAsTouched();
      return;
    }

    this.isSaving.set(true);
    this.clearMessages();
    const request = this.createForm.getRawValue();

    this.service.create(request).subscribe({
      next: (created) => {
        this.isSaving.set(false);
        this.showCreateForm.set(false);
        this.successMessage.set(`${created.referenceNumber} was created as a draft.`);
        this.createForm.reset({
          referenceNumber: '',
          title: '',
          description: '',
          priority: 'Medium',
          actor: 'product-owner@changeguard.local',
        });
        this.loadOverview();
        this.selectRequest(created.referenceNumber);
      },
      error: (error: unknown) => {
        this.isSaving.set(false);
        this.errorMessage.set(this.readError(error));
      },
    });
  }

  protected selectRequest(referenceNumber: string): void {
    this.clearMessages();
    forkJoin({
      detail: this.service.getByReferenceNumber(referenceNumber),
      audit: this.service.getAudit(referenceNumber),
    }).subscribe({
      next: ({ detail, audit }) => {
        this.selected.set(detail);
        this.audit.set(audit);
        this.artifactForm.patchValue({
          qaEvidenceNotes: detail.qaEvidenceNotes ?? '',
          rollbackPlan: detail.rollbackPlan ?? '',
        });
      },
      error: (error: unknown) => {
        this.errorMessage.set(this.readError(error));
      },
    });
  }

  protected closeDetails(): void {
    this.selected.set(null);
    this.audit.set([]);
  }

  protected primaryAction(request: ChangeRequestDetails): WorkflowOption | null {
    const actions: Partial<Record<ChangeRequestStatus, WorkflowOption>> = {
      Draft: { action: 'SubmitForReview', label: 'Submit for review' },
      RequirementReview: {
        action: 'StartDevelopment',
        label: 'Start development',
      },
      InDevelopment: { action: 'StartQaTesting', label: 'Start QA testing' },
      QaTesting: { action: 'CompleteQaTesting', label: 'Complete QA testing' },
      ReleaseApproval: { action: 'ApproveRelease', label: 'Approve release' },
      Released: { action: 'Close', label: 'Close request' },
      Rejected: { action: 'Close', label: 'Close request' },
    };

    return actions[request.status] ?? null;
  }

  protected canReject(status: ChangeRequestStatus): boolean {
    return !['Released', 'Rejected', 'Closed'].includes(status);
  }

  protected canRecordArtifacts(status: ChangeRequestStatus): boolean {
    return status === 'InDevelopment' || status === 'QaTesting';
  }

  protected applyAction(action: WorkflowAction): void {
    const request = this.selected();
    if (!request) {
      return;
    }

    this.isSaving.set(true);
    this.clearMessages();
    this.service
      .applyWorkflowAction(request.referenceNumber, {
        action,
        actor: 'workflow-user@changeguard.local',
        comment: `Action ${action} completed from the workspace.`,
      })
      .subscribe({
        next: (updated) => this.afterMutation(updated, `${action} succeeded.`),
        error: (error: unknown) => this.handleMutationError(error),
      });
  }

  protected saveArtifacts(): void {
    const request = this.selected();
    if (!request || this.artifactForm.invalid) {
      this.artifactForm.markAllAsTouched();
      return;
    }

    const form = this.artifactForm.getRawValue();
    const qaEvidenceNotes =
      request.status === 'QaTesting' && form.qaEvidenceNotes.trim()
        ? form.qaEvidenceNotes.trim()
        : null;
    const rollbackPlan = form.rollbackPlan.trim() || null;

    if (!qaEvidenceNotes && !rollbackPlan) {
      this.errorMessage.set('Enter QA evidence or a rollback plan first.');
      return;
    }

    this.isSaving.set(true);
    this.clearMessages();
    this.service
      .recordReleaseArtifacts(request.referenceNumber, {
        qaEvidenceNotes,
        rollbackPlan,
        actor: form.actor,
      })
      .subscribe({
        next: (updated) => this.afterMutation(updated, 'Release evidence was saved.'),
        error: (error: unknown) => this.handleMutationError(error),
      });
  }

  protected statusLabel(status: string): string {
    return status.replace(/([a-z])([A-Z])/g, '$1 $2');
  }

  protected priorityClass(priority: ChangePriority): string {
    return `priority priority--${priority.toLowerCase()}`;
  }

  private afterMutation(updated: ChangeRequestDetails, message: string): void {
    this.isSaving.set(false);
    this.selected.set(updated);
    this.successMessage.set(message);
    this.loadOverview();
    this.service.getAudit(updated.referenceNumber).subscribe({
      next: (audit) => this.audit.set(audit),
      error: () => this.audit.set([]),
    });
  }

  private handleMutationError(error: unknown): void {
    this.isSaving.set(false);
    this.errorMessage.set(this.readError(error));
  }

  private clearMessages(): void {
    this.errorMessage.set(null);
    this.successMessage.set(null);
  }

  private readError(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      const detail = error.error?.detail as string | undefined;
      const validationErrors = error.error?.errors as Record<string, string[]> | undefined;

      if (detail) {
        return detail;
      }
      if (validationErrors) {
        return Object.values(validationErrors).flat().join(' ');
      }
    }

    return 'The operation could not be completed. Check that the API is running.';
  }
}
