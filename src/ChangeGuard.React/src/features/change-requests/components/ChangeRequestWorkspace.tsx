import { useEffect, useState, type FormEvent } from 'react';
import { ApiError } from '../../../shared/http/api-client';
import type {
  ChangePriority,
  ChangeRequestAuditEntry,
  ChangeRequestDetails,
  ChangeRequestStatus,
  ChangeRequestSummary,
  CreateChangeRequestRequest,
  DashboardResponse,
  SearchChangeRequests,
  WorkflowAction,
} from '../models/change-request-models';
import { changeRequestService } from '../services/change-request-service';
import './ChangeRequestWorkspace.css';

interface WorkflowOption {
  action: WorkflowAction;
  label: string;
}

interface FilterForm {
  search: string;
  priority: '' | ChangePriority;
  status: '' | ChangeRequestStatus;
}

interface ArtifactForm {
  qaEvidenceNotes: string;
  rollbackPlan: string;
  actor: string;
}

type CreateErrors = Partial<Record<keyof CreateChangeRequestRequest, string>>;

const priorities: ChangePriority[] = ['Low', 'Medium', 'High', 'Critical'];
const statuses: ChangeRequestStatus[] = [
  'Draft',
  'RequirementReview',
  'InDevelopment',
  'QaTesting',
  'ReleaseApproval',
  'Released',
  'Rejected',
  'Closed',
];

const emptyFilters: FilterForm = { search: '', priority: '', status: '' };
const emptyCreateForm: CreateChangeRequestRequest = {
  referenceNumber: '',
  title: '',
  description: '',
  priority: 'Medium',
  actor: 'product-owner@changeguard.local',
};
const emptyArtifactForm: ArtifactForm = {
  qaEvidenceNotes: '',
  rollbackPlan: '',
  actor: 'qa@changeguard.local',
};

function statusLabel(value: string) {
  return value.replace(/([a-z])([A-Z])/g, '$1 $2');
}

function priorityClass(priority: ChangePriority) {
  return `priority priority--${priority.toLowerCase()}`;
}

function formatDate(value: string) {
  return new Intl.DateTimeFormat(undefined, {
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  }).format(new Date(value));
}

function formatAuditDate(value: string) {
  return new Intl.DateTimeFormat(undefined, {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(value));
}

function readError(error: unknown) {
  return error instanceof ApiError
    ? error.message
    : 'The operation could not be completed. Check that the API is running.';
}

function validateCreateForm(form: CreateChangeRequestRequest): CreateErrors {
  const errors: CreateErrors = {};
  if (!/^CG-[0-9]{3,8}$/.test(form.referenceNumber.trim())) {
    errors.referenceNumber = 'Use CG- followed by 3 to 8 digits.';
  }
  if (form.title.trim().length < 5) {
    errors.title = 'Enter a meaningful title of at least 5 characters.';
  }
  if (form.description.length > 4000) {
    errors.description = 'Description cannot exceed 4,000 characters.';
  }
  if (!form.actor.trim()) {
    errors.actor = 'Requested by is required.';
  }
  return errors;
}

function primaryAction(request: ChangeRequestDetails): WorkflowOption | null {
  const actions: Partial<Record<ChangeRequestStatus, WorkflowOption>> = {
    Draft: { action: 'SubmitForReview', label: 'Submit for review' },
    RequirementReview: { action: 'StartDevelopment', label: 'Start development' },
    InDevelopment: { action: 'StartQaTesting', label: 'Start QA testing' },
    QaTesting: { action: 'CompleteQaTesting', label: 'Complete QA testing' },
    ReleaseApproval: { action: 'ApproveRelease', label: 'Approve release' },
    Released: { action: 'Close', label: 'Close request' },
    Rejected: { action: 'Close', label: 'Close request' },
  };
  return actions[request.status] ?? null;
}

export default function ChangeRequestWorkspace() {
  const [dashboard, setDashboard] = useState<DashboardResponse | null>(null);
  const [requests, setRequests] = useState<ChangeRequestSummary[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [selected, setSelected] = useState<ChangeRequestDetails | null>(null);
  const [audit, setAudit] = useState<ChangeRequestAuditEntry[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [filterForm, setFilterForm] = useState<FilterForm>(emptyFilters);
  const [appliedFilters, setAppliedFilters] = useState<FilterForm>(emptyFilters);
  const [createForm, setCreateForm] = useState<CreateChangeRequestRequest>(emptyCreateForm);
  const [createErrors, setCreateErrors] = useState<CreateErrors>({});
  const [artifactForm, setArtifactForm] = useState<ArtifactForm>(emptyArtifactForm);
  const [refreshKey, setRefreshKey] = useState(0);

  useEffect(() => {
    const controller = new AbortController();
    const filters: SearchChangeRequests = {
      search: appliedFilters.search.trim() || undefined,
      priority: appliedFilters.priority || undefined,
      status: appliedFilters.status || undefined,
      page: 1,
      pageSize: 50,
    };

    setIsLoading(true);
    setErrorMessage(null);
    Promise.all([
      changeRequestService.getDashboard(controller.signal),
      changeRequestService.search(filters, controller.signal),
    ])
      .then(([nextDashboard, page]) => {
        setDashboard(nextDashboard);
        setRequests(page.items);
        setTotalCount(page.totalCount);
        setIsLoading(false);
      })
      .catch((error: unknown) => {
        if (error instanceof DOMException && error.name === 'AbortError') return;
        setIsLoading(false);
        setErrorMessage(readError(error));
      });

    return () => controller.abort();
  }, [appliedFilters, refreshKey]);

  function clearMessages() {
    setErrorMessage(null);
    setSuccessMessage(null);
  }

  function applyFilters(event: FormEvent) {
    event.preventDefault();
    clearMessages();
    setAppliedFilters({ ...filterForm });
  }

  function clearFilters() {
    clearMessages();
    setFilterForm(emptyFilters);
    setAppliedFilters({ ...emptyFilters });
  }

  function toggleCreateForm() {
    setShowCreateForm((visible) => !visible);
    setCreateErrors({});
    clearMessages();
  }

  async function createRequest(event: FormEvent) {
    event.preventDefault();
    const errors = validateCreateForm(createForm);
    setCreateErrors(errors);
    if (Object.keys(errors).length > 0) return;

    setIsSaving(true);
    clearMessages();
    try {
      const request = {
        ...createForm,
        referenceNumber: createForm.referenceNumber.trim(),
        title: createForm.title.trim(),
        description: createForm.description.trim(),
        actor: createForm.actor.trim(),
      };
      const created = await changeRequestService.create(request);
      setShowCreateForm(false);
      setCreateForm(emptyCreateForm);
      setCreateErrors({});
      setSuccessMessage(`${created.referenceNumber} was created as a draft.`);
      setRefreshKey((value) => value + 1);
      await selectRequest(created.referenceNumber, false);
    } catch (error) {
      setErrorMessage(readError(error));
    } finally {
      setIsSaving(false);
    }
  }

  async function selectRequest(referenceNumber: string, clear = true) {
    if (clear) clearMessages();
    try {
      const [detail, nextAudit] = await Promise.all([
        changeRequestService.getByReferenceNumber(referenceNumber),
        changeRequestService.getAudit(referenceNumber),
      ]);
      setSelected(detail);
      setAudit(nextAudit);
      setArtifactForm({
        qaEvidenceNotes: detail.qaEvidenceNotes ?? '',
        rollbackPlan: detail.rollbackPlan ?? '',
        actor: 'qa@changeguard.local',
      });
    } catch (error) {
      setErrorMessage(readError(error));
    }
  }

  function closeDetails() {
    setSelected(null);
    setAudit([]);
  }

  async function afterMutation(updated: ChangeRequestDetails, message: string) {
    setSelected(updated);
    setSuccessMessage(message);
    setRefreshKey((value) => value + 1);
    try {
      setAudit(await changeRequestService.getAudit(updated.referenceNumber));
    } catch {
      setAudit([]);
    }
  }

  async function applyAction(action: WorkflowAction) {
    if (!selected) return;
    setIsSaving(true);
    clearMessages();
    try {
      const updated = await changeRequestService.applyWorkflowAction(selected.referenceNumber, {
        action,
        actor: 'workflow-user@changeguard.local',
        comment: `Action ${action} completed from the workspace.`,
      });
      await afterMutation(updated, `${statusLabel(action)} succeeded.`);
    } catch (error) {
      setErrorMessage(readError(error));
    } finally {
      setIsSaving(false);
    }
  }

  async function saveArtifacts(event: FormEvent) {
    event.preventDefault();
    if (!selected) return;

    const qaEvidenceNotes =
      selected.status === 'QaTesting' && artifactForm.qaEvidenceNotes.trim()
        ? artifactForm.qaEvidenceNotes.trim()
        : null;
    const rollbackPlan = artifactForm.rollbackPlan.trim() || null;

    if (!artifactForm.actor.trim()) {
      setErrorMessage('Recorded by is required.');
      return;
    }
    if (!qaEvidenceNotes && !rollbackPlan) {
      setErrorMessage('Enter QA evidence or a rollback plan first.');
      return;
    }

    setIsSaving(true);
    clearMessages();
    try {
      const updated = await changeRequestService.recordReleaseArtifacts(
        selected.referenceNumber,
        { qaEvidenceNotes, rollbackPlan, actor: artifactForm.actor.trim() },
      );
      await afterMutation(updated, 'Release evidence was saved.');
    } catch (error) {
      setErrorMessage(readError(error));
    } finally {
      setIsSaving(false);
    }
  }

  const selectedAction = selected ? primaryAction(selected) : null;
  const canReject = selected && !['Released', 'Rejected', 'Closed'].includes(selected.status);
  const canRecordArtifacts =
    selected && (selected.status === 'InDevelopment' || selected.status === 'QaTesting');

  return (
    <section className="workspace" aria-labelledby="workspace-title">
      <header className="workspace__header">
        <div>
          <p className="kicker">Mission control</p>
          <h2 id="workspace-title">Change operations</h2>
          <p>Move every request from a clear requirement to a safe release.</p>
        </div>
        <button className="button button--primary" type="button" onClick={toggleCreateForm}>
          {showCreateForm ? 'Cancel' : '+ New change request'}
        </button>
      </header>

      {errorMessage && (
        <div className="notice notice--error" role="alert">
          <strong>Action needed</strong>
          <span>{errorMessage}</span>
        </div>
      )}
      {successMessage && (
        <div className="notice notice--success" role="status">
          <strong>Mission updated</strong>
          <span>{successMessage}</span>
        </div>
      )}

      {showCreateForm && (
        <form className="create-panel" onSubmit={createRequest} noValidate>
          <div className="section-heading">
            <span>01</span>
            <div>
              <h3>Open a traceable request</h3>
              <p>The reference number is the permanent business identifier.</p>
            </div>
          </div>

          <div className="form-grid">
            <label>
              <span>Reference number</span>
              <input
                value={createForm.referenceNumber}
                onChange={(event) =>
                  setCreateForm({ ...createForm, referenceNumber: event.target.value.toUpperCase() })
                }
                placeholder="CG-501"
                aria-invalid={Boolean(createErrors.referenceNumber)}
              />
              {createErrors.referenceNumber && <small>{createErrors.referenceNumber}</small>}
            </label>
            <label>
              <span>Priority</span>
              <select
                value={createForm.priority}
                onChange={(event) =>
                  setCreateForm({ ...createForm, priority: event.target.value as ChangePriority })
                }
              >
                {priorities.map((priority) => (
                  <option key={priority} value={priority}>{priority}</option>
                ))}
              </select>
            </label>
            <label className="form-grid__wide">
              <span>Title</span>
              <input
                value={createForm.title}
                onChange={(event) => setCreateForm({ ...createForm, title: event.target.value })}
                placeholder="What business change is required?"
                aria-invalid={Boolean(createErrors.title)}
              />
              {createErrors.title && <small>{createErrors.title}</small>}
            </label>
            <label className="form-grid__wide">
              <span>Description</span>
              <textarea
                value={createForm.description}
                onChange={(event) =>
                  setCreateForm({ ...createForm, description: event.target.value })
                }
                rows={4}
                placeholder="Explain the problem, expected outcome and business impact."
                aria-invalid={Boolean(createErrors.description)}
              />
              {createErrors.description && <small>{createErrors.description}</small>}
            </label>
            <label className="form-grid__wide">
              <span>Requested by</span>
              <input
                value={createForm.actor}
                onChange={(event) => setCreateForm({ ...createForm, actor: event.target.value })}
                type="email"
                aria-invalid={Boolean(createErrors.actor)}
              />
              {createErrors.actor && <small>{createErrors.actor}</small>}
            </label>
          </div>

          <div className="form-actions">
            <button className="button button--primary" type="submit" disabled={isSaving}>
              {isSaving ? 'Creating…' : 'Create draft'}
            </button>
          </div>
        </form>
      )}

      {dashboard && (
        <div className="metrics" aria-label="Change request metrics">
          <article><span className="metric-icon metric-icon--blue">Σ</span><div><strong>{dashboard.totalRequests}</strong><span>Total requests</span></div></article>
          <article><span className="metric-icon metric-icon--green">↗</span><div><strong>{dashboard.activeRequests}</strong><span>Active missions</span></div></article>
          <article><span className="metric-icon metric-icon--red">!</span><div><strong>{dashboard.blockedRequests}</strong><span>Release blocked</span></div></article>
          <article><span className="metric-icon metric-icon--amber">◷</span><div><strong>{dashboard.slaBreachedRequests}</strong><span>SLA breached</span></div></article>
        </div>
      )}

      <section className="request-panel">
        <div className="section-heading section-heading--compact">
          <span>02</span>
          <div>
            <h3>Request radar</h3>
            <p>{totalCount} matching request{totalCount === 1 ? '' : 's'}</p>
          </div>
        </div>

        <form className="filters" onSubmit={applyFilters}>
          <label className="search-field">
            <span className="sr-only">Search requests</span>
            <input
              value={filterForm.search}
              onChange={(event) => setFilterForm({ ...filterForm, search: event.target.value })}
              placeholder="Search reference or title"
            />
          </label>
          <label>
            <span className="sr-only">Filter by priority</span>
            <select
              value={filterForm.priority}
              onChange={(event) =>
                setFilterForm({ ...filterForm, priority: event.target.value as FilterForm['priority'] })
              }
            >
              <option value="">All priorities</option>
              {priorities.map((priority) => <option key={priority} value={priority}>{priority}</option>)}
            </select>
          </label>
          <label>
            <span className="sr-only">Filter by status</span>
            <select
              value={filterForm.status}
              onChange={(event) =>
                setFilterForm({ ...filterForm, status: event.target.value as FilterForm['status'] })
              }
            >
              <option value="">All statuses</option>
              {statuses.map((status) => <option key={status} value={status}>{statusLabel(status)}</option>)}
            </select>
          </label>
          <button className="button button--dark" type="submit">Apply</button>
          <button className="button button--ghost" type="button" onClick={clearFilters}>Clear</button>
        </form>

        {isLoading ? (
          <div className="empty-state" role="status">Scanning change requests…</div>
        ) : requests.length === 0 ? (
          <div className="empty-state"><strong>No requests found.</strong><span>Create the first request or clear the filters.</span></div>
        ) : (
          <div className="table-wrap">
            <table>
              <thead><tr><th>Request</th><th>Priority</th><th>Status</th><th>Readiness</th><th>SLA</th><th><span className="sr-only">Open</span></th></tr></thead>
              <tbody>
                {requests.map((request) => (
                  <tr key={request.id} className={selected?.id === request.id ? 'row--selected' : ''}>
                    <td>
                      <button className="request-link" type="button" onClick={() => void selectRequest(request.referenceNumber)}>
                        <strong>{request.referenceNumber}</strong><span>{request.title}</span>
                      </button>
                    </td>
                    <td><span className={priorityClass(request.priority)}>{request.priority}</span></td>
                    <td><span className="status-pill">{statusLabel(request.status)}</span></td>
                    <td><div className="readiness"><span>{request.readinessScore}%</span><i><b style={{ width: `${request.readinessScore}%` }} /></i></div></td>
                    <td><span className={request.isSlaBreached ? 'sla--breached' : ''}>{request.isSlaBreached ? 'Breached' : formatDate(request.slaDueUtc)}</span></td>
                    <td><button className="icon-button" type="button" onClick={() => void selectRequest(request.referenceNumber)} aria-label={`Open ${request.referenceNumber}`}>→</button></td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>

      {selected && (
        <>
          <button className="detail-backdrop" type="button" onClick={closeDetails} aria-label="Close request details" />
          <aside className="detail-panel" role="dialog" aria-modal="true" aria-labelledby="detail-title">
            <header className="detail-panel__header">
              <div><span>{selected.referenceNumber}</span><h3 id="detail-title">{selected.title}</h3></div>
              <button className="icon-button" type="button" onClick={closeDetails} aria-label="Close details">×</button>
            </header>

            <div className="detail-summary">
              <div className={`score-ring${selected.isBlocked ? ' score-ring--blocked' : ''}`}><strong>{selected.readinessScore}%</strong><span>ready</span></div>
              <div>
                <span className={priorityClass(selected.priority)}>{selected.priority}</span>
                <h4>{statusLabel(selected.status)}</h4>
                <p className={selected.isSlaBreached ? 'sla--breached' : ''}>
                  {selected.isSlaBreached
                    ? `SLA breached by ${Math.abs(selected.remainingSlaHours).toFixed(1)} hours`
                    : `SLA: ${selected.remainingSlaHours.toFixed(1)} hours remaining`}
                </p>
              </div>
            </div>

            <div className="detail-block"><h4>Business context</h4><p>{selected.description || 'No description was provided.'}</p></div>

            <div className="gate-grid">
              <article className={selected.hasQaEvidence ? 'gate--complete' : ''}><span>{selected.hasQaEvidence ? '✓' : '!'}</span><div><strong>QA evidence</strong><small>{selected.hasQaEvidence ? 'Recorded' : 'Missing'}</small></div></article>
              <article className={selected.hasRollbackPlan ? 'gate--complete' : ''}><span>{selected.hasRollbackPlan ? '✓' : '!'}</span><div><strong>Rollback plan</strong><small>{selected.hasRollbackPlan ? 'Recorded' : 'Missing'}</small></div></article>
            </div>

            {selected.blockers.length > 0 && (
              <div className="blockers"><strong>Gate findings</strong><ul>{selected.blockers.map((blocker) => <li key={blocker}>{blocker}</li>)}</ul></div>
            )}

            {canRecordArtifacts && (
              <form className="artifact-form" onSubmit={saveArtifacts}>
                <h4>Release evidence</h4>
                {selected.status === 'QaTesting' && (
                  <label><span>QA evidence notes</span><textarea value={artifactForm.qaEvidenceNotes} onChange={(event) => setArtifactForm({ ...artifactForm, qaEvidenceNotes: event.target.value })} rows={3} placeholder="Test run, result and evidence link" /></label>
                )}
                <label><span>Rollback plan</span><textarea value={artifactForm.rollbackPlan} onChange={(event) => setArtifactForm({ ...artifactForm, rollbackPlan: event.target.value })} rows={3} placeholder="Exact recovery steps" /></label>
                <label><span>Recorded by</span><input value={artifactForm.actor} onChange={(event) => setArtifactForm({ ...artifactForm, actor: event.target.value })} type="email" /></label>
                <button className="button button--dark" type="submit" disabled={isSaving}>Save evidence</button>
              </form>
            )}

            <div className="workflow-actions">
              {selectedAction && <button className="button button--primary" type="button" disabled={isSaving} onClick={() => void applyAction(selectedAction.action)}>{selectedAction.label}</button>}
              {canReject && <button className="button button--danger" type="button" disabled={isSaving} onClick={() => void applyAction('Reject')}>Reject</button>}
            </div>

            <div className="audit">
              <h4>Audit trail</h4>
              {audit.length === 0 ? <p>No audit entries exist for this request yet.</p> : (
                <ol>{audit.map((entry) => <li key={entry.id}><span /><div><strong>{statusLabel(entry.action)}</strong><small>{entry.actor} · {formatAuditDate(entry.occurredUtc)}</small>{entry.comment && <p>{entry.comment}</p>}</div></li>)}</ol>
              )}
            </div>
          </aside>
        </>
      )}
    </section>
  );
}
