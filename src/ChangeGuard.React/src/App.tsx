import { useEffect, useState } from 'react';
import type { SystemHealthResponse } from './core/models/system-health';
import { getSystemHealth } from './core/services/system-health-service';
import ChangeRequestWorkspace from './features/change-requests/components/ChangeRequestWorkspace';
import './App.css';

interface HealthViewState {
  loading: boolean;
  data: SystemHealthResponse | null;
  error: string | null;
}

const initialHealthState: HealthViewState = {
  loading: true,
  data: null,
  error: null,
};

export default function App() {
  const [health, setHealth] = useState<HealthViewState>(initialHealthState);

  useEffect(() => {
    const controller = new AbortController();

    getSystemHealth(controller.signal)
      .then((data) => setHealth({ loading: false, data, error: null }))
      .catch((error: unknown) => {
        if (error instanceof DOMException && error.name === 'AbortError') return;
        setHealth({
          loading: false,
          data: null,
          error: 'Unable to connect to the ChangeGuard API.',
        });
      });

    return () => controller.abort();
  }, []);

  const healthText = health.loading ? 'Connecting' : health.error ? 'API offline' : health.data?.status;
  const healthClass = health.error
    ? 'api-status api-status--error'
    : health.data?.status === 'Healthy'
      ? 'api-status api-status--healthy'
      : 'api-status';

  return (
    <div className="app-root">
      <header className="topbar">
        <a className="brand" href="/" aria-label="ChangeGuard home">
          <span>CG</span>
          <strong>ChangeGuard</strong>
        </a>
        <div className="environment">Local workspace · React</div>
        <div className={healthClass} title="Backend API connection">
          <i aria-hidden="true" />
          <span data-testid="health-status">{healthText}</span>
        </div>
      </header>

      <main className="app-shell">
        <section className="hero">
          <div>
            <p className="eyebrow">Requirement-to-release intelligence</p>
            <h1>
              Ship change with evidence,
              <br />
              <em>not assumptions.</em>
            </h1>
            <p className="subtitle">
              One operational workspace for requirements, SLA, QA proof, rollback safety,
              approvals and a permanent audit trail.
            </p>
          </div>
          <div className="hero__signal" aria-hidden="true">
            <span>Release signal</span>
            <strong>Guarded</strong>
            <i />
          </div>
        </section>

        <ChangeRequestWorkspace />

        <footer>
          <span>ChangeGuard · Job-ready modular monolith</span>
          {health.data && (
            <span>
              {health.data.service} v{health.data.version}
            </span>
          )}
        </footer>
      </main>
    </div>
  );
}
