import { AsyncPipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  inject
} from '@angular/core';
import {
  catchError,
  map,
  Observable,
  of,
  startWith
} from 'rxjs';

import { ReleaseReadinessResponse } from '../../models/release-readiness-response.model';
import { ReleaseReadinessService } from '../../services/release-readiness.service';

interface ReleaseReadinessViewState {
  isLoading: boolean;
  request: ReleaseReadinessResponse | null;
  errorMessage: string | null;
}

@Component({
  selector: 'app-release-readiness-card',
  imports: [AsyncPipe],
  templateUrl: './release-readiness-card.component.html',
  styleUrl: './release-readiness-card.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ReleaseReadinessCardComponent {
  private readonly releaseReadinessService =
    inject(ReleaseReadinessService);

  protected readonly viewState$:
    Observable<ReleaseReadinessViewState> =
    this.releaseReadinessService
      .getReleaseReadiness()
      .pipe(
        map(
          (request): ReleaseReadinessViewState => ({
            isLoading: false,
            request,
            errorMessage: null
          })
        ),
        startWith({
          isLoading: true,
          request: null,
          errorMessage: null
        } as ReleaseReadinessViewState),
        catchError(() =>
          of<ReleaseReadinessViewState>({
            isLoading: false,
            request: null,
            errorMessage:
              'Release-readiness data could not be loaded.'
          })
        )
      );
}
