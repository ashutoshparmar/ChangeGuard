import {
  ComponentFixture,
  TestBed,
} from '@angular/core/testing';
import { of } from 'rxjs';

import { AppComponent } from './app.component';
import { SystemHealthResponse } from './core/models/system-health-response.model';
import { SystemHealthService } from './core/services/system-health.service';

describe('AppComponent', () => {
  let fixture: ComponentFixture<AppComponent>;

  const healthResponse: SystemHealthResponse = {
    status: 'Healthy',
    service: 'ChangeGuard.Api',
    version: '1.0.0',
    timestampUtc: '2026-08-17T14:03:42+00:00',
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppComponent],
      providers: [
        {
          provide: SystemHealthService,
          useValue: {
            getHealth: () => of(healthResponse),
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AppComponent);
  });

  it('should create the application', () => {
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should display the API health status', () => {
    fixture.detectChanges();

    const element = fixture.nativeElement as HTMLElement;
    const healthStatus = element.querySelector(
      '[data-testid="health-status"]'
    );

    expect(healthStatus?.textContent).toContain('Healthy');
  });
});
