# ChangeGuard AI - Product Requirements Baseline

## Document Information

| Field | Value |
|---|---|
| Status | Initial Day 1 baseline |
| Date | 2026-08-17 |
| Product | ChangeGuard AI |
| Purpose | Requirement-to-release risk intelligence |

This document is the correct location for the Business Problem, Business Impact, Stakeholders, functional requirements, non-functional requirements, business rules, acceptance criteria, assumptions, dependencies, risks, constraints, and scope.

It should be reviewed with a real client or product owner before it becomes a contractual specification.

## 1. Business Problem

The current change-delivery process is fragmented across meetings, email, tickets, spreadsheets, and verbal communication. Work can begin before requirement gaps are clarified. Development, QA, and deployment may be completed without consistent evidence, database-change records, approval history, or rollback information.

The organisation needs one traceable workflow that connects requirement submission, clarification, development, testing, approval, release, and closure.

## 2. Business Impact

The current process may cause:

- Production incidents caused by incomplete requirements
- Release delays and repeated rework
- Increased delivery and support cost
- Missing evidence during audits
- SLA breaches and missed business deadlines
- Unclear ownership and accountability
- Incomplete release and rollback preparation
- Limited management visibility
- Repeated incidents because historical patterns are not analysed

These impacts explain why the project is valuable. They should later be supported with client data such as incident count, delay hours, rework cost, SLA breach rate, and audit findings.

## 3. Current Process

1. A change is communicated through a meeting, email, or ticket.
2. A work item may be created without complete clarification.
3. Development starts.
4. QA performs testing.
5. Deployment is completed.
6. Evidence, database changes, approvals, and rollback details may not be consistently recorded.

## 4. Proposed Process

1. A user submits a structured change request.
2. Required information is validated.
3. A Product Owner or Business Analyst clarifies requirement gaps.
4. The request is assigned and developed.
5. QA records test cases, results, and evidence.
6. Release readiness is reviewed.
7. An authorised approver approves or rejects the release.
8. Deployment and rollback information are recorded.
9. The request is closed with a complete audit history.
10. Management can review SLA performance, risk, and delivery reports.

## 5. Stakeholders

### Product Owner

- Owns priorities and product decisions
- Clarifies expected outcomes
- Reviews requirements and accepts delivered functionality

### Business Analyst

- Analyses business needs
- Documents requirements and workflows
- Identifies ambiguity and missing information

### Developer

- Designs and implements the technical change
- Records implementation and database-change details
- Supports defect resolution

### QA Engineer

- Writes and executes test cases
- Records results
- Uploads test evidence
- Confirms whether acceptance criteria are satisfied

### Release Approver

- Reviews testing evidence, risks, dependencies, and rollback readiness
- Approves or rejects a release

### Administrator

- Manages users, roles, workflow configuration, and platform administration

### Senior Management Viewer

- Reviews status, risk, SLA, and management reports
- Has read-only access unless another role is assigned

## 6. Functional Requirements

### FR-001 - Submit Change Request

An authorised user must be able to submit a change request using a structured form.

### FR-002 - Manage Requirement Clarifications

The Product Owner or Business Analyst must be able to record questions, answers, and unresolved requirement gaps.

### FR-003 - Manage Users and Roles

An administrator must be able to manage users and assign authorised roles.

### FR-004 - Track Workflow Status

The system must track the request through defined workflow states.

### FR-005 - Record Development Information

Developers must be able to record implementation notes, affected components, and database changes.

### FR-006 - Record Testing

QA must be able to record test cases, results, defects, and evidence.

### FR-007 - Manage Release Approval

An authorised approver must be able to approve or reject a release with comments.

### FR-008 - Record Rollback Plan

The system must allow the delivery team to document a rollback plan before release approval.

### FR-009 - Maintain Audit History

The system must record important actions, workflow transitions, actors, and timestamps.

### FR-010 - Provide Reports

Authorised users must be able to review request status, SLA performance, risk, and delivery history.

## 7. Non-Functional Requirements

The following are initial targets and require client validation before production commitment.

### NFR-001 - Security

The system must authenticate users and enforce role-based access to protected operations.

### NFR-002 - Auditability

Important workflow actions must be traceable to an actor and UTC timestamp.

### NFR-003 - Performance

A measurable API-response target must be agreed after representative load and usage expectations are known. The vague statement "the system should be fast" is not sufficient.

### NFR-004 - Reliability

The application must handle expected transient dependency failures without silently losing confirmed business operations.

### NFR-005 - Observability

The production solution must provide structured logs, correlation identifiers, metrics, traces, and health information appropriate to its hosting environment.

### NFR-006 - Maintainability

The codebase must use clear module boundaries, automated tests, documented decisions, and repeatable builds.

### NFR-007 - Data Protection

Secrets and sensitive data must not be stored in source code, client-side configuration, or application logs.

## 8. Business Rules

### BR-001 - Mandatory Information

All fields marked as required must be completed before a request can be submitted.

### BR-002 - Test Evidence

QA cannot complete testing until the required test evidence has been uploaded.

### BR-003 - Rollback Readiness

A release cannot be approved until the required rollback information is recorded or an authorised exception is documented.

### BR-004 - Role-Based Transitions

Only users with the required role can perform protected workflow transitions.

### BR-005 - SLA Policy

The request SLA must be calculated according to the agreed priority and SLA policy.

### BR-006 - Audit Creation

Successful workflow transitions must create an audit-history entry.

## 9. Acceptance Criteria

### AC-001 - Complete QA Testing Successfully

```gherkin
Given the request is in QA Testing
And QA has uploaded the required test evidence
When QA selects Complete Testing
Then the request status changes to Release Approval
And the system creates an audit-history entry
```

### AC-002 - Prevent Completion Without Evidence

```gherkin
Given the request is in QA Testing
And the required test evidence has not been uploaded
When QA selects Complete Testing
Then the request remains in QA Testing
And the system displays a test-evidence-required message
And no successful transition audit is created
```

### AC-003 - Enforce Release Permission

```gherkin
Given a user is authenticated
And the user does not have the Release Approver role
When the user attempts to approve a release
Then the operation is forbidden
And the request status is unchanged
```

## 10. Assumptions

- The client will provide representative historical incident and service-request data.
- The client will identify a Product Owner who can clarify workflow decisions.
- Users will access the application through supported modern browsers.
- Initial usage can be supported by a modular monolith.

Assumptions must be validated. If an assumption is proven false, review its effect on scope, cost, risk, and architecture.

## 11. Dependencies

- Historical-data analysis depends on receiving usable client data.
- Role and permission design depends on stakeholder confirmation.
- SLA implementation depends on approved priority and calendar rules.
- Azure deployment depends on an Azure subscription, budget, and access.
- External integrations depend on credentials and technical documentation supplied through approved channels.

## 12. Risks

- Historical data may be incomplete or inconsistent.
- Stakeholders may disagree on workflow ownership.
- Unclear SLA rules may cause incorrect deadlines.
- Scope may expand before the core workflow is validated.
- Adding distributed services too early may create unnecessary complexity and cost.
- Secrets may be mishandled unless secure configuration rules are followed.

Each material risk should later have an owner, probability, impact, mitigation, and current status.

## 13. Constraints

- The initial client/team model is small.
- The solution should not introduce unnecessary service complexity.
- Initial development is local and should minimise cloud cost.
- The React client must communicate through the API and must never connect directly to SQL Server.
- Secrets must not be committed to Git.

## 14. Initial Scope

### In Scope

- Change-request submission
- Requirement clarification
- Workflow status tracking
- Role-based actions
- SLA tracking
- QA evidence
- Release approval
- Rollback information
- Audit history
- Basic reports and dashboards
- Risk indicators

### Out of Scope Until Confirmed

- Automatic production deployment to client environments
- Replacing the client's existing ticketing system
- Fully autonomous AI decisions
- Microservice extraction without a validated need
- Every possible external integration

## 15. Open Questions

1. How are priority and SLA deadlines calculated?
2. Which workflow states and transitions are mandatory?
3. Who can approve emergency changes?
4. Which test evidence types are required?
5. When may a rollback-plan exception be accepted?
6. Which reports are required for management and audit teams?
7. What historical data can the client provide?
8. What data is confidential or regulated?
9. Which external systems must be integrated?
10. What production availability and performance targets are required?

## 16. How to Maintain This Document

- Update it when a requirement is clarified or changed.
- Give requirements stable identifiers such as `FR-001` and `BR-001`.
- Record measurable criteria instead of vague adjectives.
- Keep completed functionality separate from planned functionality.
- Convert important requirements into test cases.
- Obtain Product Owner approval before treating the baseline as final.
