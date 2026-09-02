import {
  ComponentFixture,
  TestBed
} from '@angular/core/testing';
import { of } from 'rxjs';

import { ReleaseReadinessResponse } from '../../models/release-readiness-response.model';
import { ReleaseReadinessService } from '../../services/release-readiness.service';
import { ReleaseReadinessCardComponent } from './release-readiness-card.component';

describe('ReleaseReadinessCardComponent', () => {
  let fixture:
    ComponentFixture<ReleaseReadinessCardComponent>;

  const response: ReleaseReadinessResponse = {
    referenceNumber: 'CG-101',
    title: 'Payment validation update',
    priority: 'Critical',
    status: 'QaTesting',
    score: 45,
    isBlocked: true,
    canMoveToReleaseApproval: false,
    blockers: [
      'QA evidence is missing.',
      'Rollback plan is missing.'
    ]
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ReleaseReadinessCardComponent],
      providers: [
        {
          provide: ReleaseReadinessService,
          useValue: {
            getReleaseReadiness: () =>
              of(response)
          }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(
      ReleaseReadinessCardComponent
    );

    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();
  });

  it('should display the blocked release decision', () => {
    const element =
      fixture.nativeElement as HTMLElement;

    expect(element.textContent).toContain('CG-101');
    expect(element.textContent).toContain('45%');
    expect(element.textContent).toContain(
      'RELEASE BLOCKED'
    );
    expect(element.textContent).toContain(
      'QA evidence is missing.'
    );
    expect(element.textContent).toContain(
      'Rollback plan is missing.'
    );

    expect(
      element.querySelector('.request-card--blocked')
    ).not.toBeNull();

    expect(
      element.querySelectorAll('.gate-findings li')
        .length
    ).toBe(2);
  });
});
