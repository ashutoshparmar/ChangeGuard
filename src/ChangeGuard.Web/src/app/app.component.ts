import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { catchError, map, Observable, of, startWith } from 'rxjs';

import { SystemHealthResponse } from './core/models/system-health-response.model';
import { SystemHealthService } from './core/services/system-health.service';
import { ChangeRequestWorkspaceComponent } from './features/change-requests/components/change-request-workspace/change-request-workspace.component';

interface HealthViewState {
  loading: boolean;
  data: SystemHealthResponse | null;
  error: string | null;
}

@Component({
  selector: 'cg-root',
  imports: [AsyncPipe, ChangeRequestWorkspaceComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppComponent {
  private readonly systemHealthService = inject(SystemHealthService);

  protected readonly healthState$: Observable<HealthViewState> = this.systemHealthService
    .getHealth()
    .pipe(
      map((data): HealthViewState => ({
        loading: false,
        data,
        error: null,
      })),
      catchError(() =>
        of<HealthViewState>({
          loading: false,
          data: null,
          error: 'Unable to connect to the ChangeGuard API.',
        }),
      ),
      startWith({
        loading: true,
        data: null,
        error: null,
      }),
    );
}
