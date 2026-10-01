# HomeServe IT Full Test Plan

**Status:** Planned; no tests were run as part of drafting this plan.
**Scope:** Current web application across public, Customer, Technician, and Administrator experiences.
**Purpose:** Give the team a repeatable release-readiness pass covering user experience, browser behavior, authorization, application logic, persistence, concurrency, security, and performance.

## 1. Product areas in scope

The plan follows the current MVC areas and supporting endpoints:

- **Public and Identity:** home/privacy pages, registration, login, lockout, password change, and invited-account setup.
- **Customer:** dashboard, service requests, devices, quotations and checkout, bills/payments, support, notifications, and profile/settings.
- **Technician:** dashboard, assigned jobs, schedule, job progress/handover, quotations, deliverables, chat, notifications, and profile/settings.
- **Administrator:** dashboard, service-request operations, technician assignment, customer CRM/history, quotations, billing, inventory/stock movements, reports, support, notifications, user management/archive, and system settings.
- **Shared/backend paths:** HomeServe API, notification actions, private-file delivery, SignalR chat hub, Identity, EF Core migrations/query filters, and services for accounts, settings, assignments, inventory, notifications, uploads, and PDF reports.

The current app has no external payment-provider integration. Verify its local quotation, invoice, and payment-state rules; do not treat a local `Paid` state as proof that money was captured. External services such as weather should be replaced with controlled test responses where practical.

## 2. Risk order

Run the checks in this order so that high-impact failures are found early.

1. **P0 — protect people and records:** role separation, ownership/assignment checks, valid workflow transitions, antiforgery, trusted server-side amounts, inventory and financial atomicity, safe database targeting, and private-file access.
2. **P1 — complete the service journey:** customer request → administrator assignment/quotation → technician work → customer review → invoice/payment state; cancellation, rescheduling, notifications, reports, account settings, and recovery paths.
3. **P2 — product quality under variation:** accessibility, responsive layouts, browser differences, empty/error states, larger datasets, slow/failing dependencies, and performance baselines.

Every state-changing scenario should assert both the user-visible result and the persisted state. For denied actions, assert that no related record, stock quantity, payment state, notification, or audit entry changed unexpectedly.

## 3. Test environment and data

Use a dedicated local test environment with fictional data only. The Playwright and Development app paths can apply EF migrations at startup, so verify the configured database target before launching the app. Never point browser, concurrency, load, or security tests at a shared, staging, or production database.

Build an isolated fixture containing:

- one Administrator, two Customers, and two Technicians, each with distinct Identity accounts;
- requests in each meaningful workflow state, including unassigned, assigned, active, awaiting review, completed, cancelled, and archived records where the current rules permit them;
- overlapping and non-overlapping technician appointments;
- approved, pending, rejected, paid, unpaid, and voided quotation/invoice combinations as valid in the application;
- inventory items with ample stock, low stock, and scarce stock, plus existing stock-movement history;
- read and unread notifications, support records, devices, report history, and private deliverables;
- files and names containing Unicode, whitespace, punctuation, long values, and HTML-like text.

Use reserved `.invalid` email domains and generated passwords supplied through a secret store/environment variables. Reset the fixture between destructive browser suites. Retain only redacted logs and fictional screenshots/traces. The existing `npm run test:phases` runner is intended to own a randomly named disposable MySQL database and remove it afterward; use that isolated path for destructive browser coverage. MySQL-specific race tests require `HOMESERVE_TEST_CONNECTION` to target a local disposable MySQL server that can create and drop test databases.

## 4. Test layers

### Build and fast regression

- Restore/build the solution and run all xUnit tests on every change.
- Keep service and controller behavior tests fast with the existing in-memory SQLite fixture.
- Add/extend tests around rule boundaries: allowed and denied transitions, invalid input, empty datasets, duplicate submission, ownership mismatch, archived/cancelled records, query-filter behavior, totals, and rollback.
- Inspect generated Razor/Tailwind output through the application; do not edit generated `app.css` directly.

### MySQL and persistence integration

- Replay all migrations against an empty disposable MySQL database and upgrade a disposable prior-schema database to the latest migration.
- Compare important MySQL behavior with SQLite test behavior: decimal/date handling, unique constraints, foreign keys, query filters, ordering/pagination, and transaction rollback.
- Test multi-record operations for all-or-nothing results: assignment/rescheduling, quotation/invoice updates, cancellation and cleanup, inventory usage and movement, and account/role changes.
- Force concurrent writes for scarce inventory and overlapping technician schedules. Confirm no negative stock, duplicate reservation/deduction, double booking, duplicate payment transition, or missing audit movement.
- Exercise report generation and PDF history with realistic date ranges, empty results, and boundary dates in the configured display timezone.

### Browser and end-to-end

- Use Playwright for normal role journeys and direct-request negative tests. The configured full browser matrix is Chromium, Firefox, desktop WebKit, and mobile WebKit. Keep the full matrix for release qualification; use a faster Chromium/WebKit subset for focused iteration.
- Check page errors, console errors, failed network requests, unexpected 5xx responses, redirects, and persisted outcomes.
- Capture traces/screenshots only on test failure, and keep fixture data fictional.
- Cover API/controller/hub calls made from the browser as well as page navigation. UI hiding is never evidence of authorization.

### Manual UI/UX and accessibility

- Walk every primary page for each role at 320/360/390, 768, 1024, and 1440 CSS pixels; include narrow and wide content, zoom/text scaling, and touch use.
- Check navigation, hierarchy, terminology, current status, next action, field labels/help, inline and summary errors, loading/empty/error/success states, confirmation for destructive actions, and recovery after an error.
- Complete core journeys using only keyboard. Check skip/navigation order, visible focus, Escape behavior, focus return, dialogs and focus containment, tab controls, form errors, and no keyboard traps.
- Run axe on every primary route and representative dialog/state, then manually assess contrast, semantic headings/landmarks, accessible names, status announcements, reduced motion, and screen-reader flow with VoiceOver and a Windows screen reader when available.
- Check target sizes, text contrast, non-color status cues, table/chart alternatives, currency/date readability, and horizontal overflow. Automated axe results alone do not establish accessibility conformance.
- Run short task-based sessions with representative users or colleagues acting as each role. Give them realistic goals without showing the route in advance; record completion, hesitation, errors, and unclear language for request creation, assignment, job progress, quotation review/payment state, and inventory updates. Establish usability targets after observing a baseline rather than inventing them.

### Security and privacy

Test safely against the isolated fixture:

- anonymous access to protected pages and all three cross-role access directions;
- Customer A attempting to read or mutate Customer B's requests, devices, invoices, quotations, messages, support items, notifications, and files;
- Technician A attempting to access unassigned, differently assigned, cancelled, archived, or reassigned jobs and their chat/files;
- forged resource IDs, role fields, user IDs, prices, totals, payment states, assignment IDs, stock quantities, and hidden form values;
- missing/invalid antiforgery tokens on cookie-authenticated writes; confirm GET requests do not mutate state;
- registration controls, invitation expiry/single-use, password rules, lockout, logout/session behavior, profile email collisions, and role changes;
- output encoding and safe rendering for HTML-like text, Unicode, newlines, quotes, backslashes, and long values;
- uploads with mismatched extension/content type, malformed/oversized bodies, traversal-like names, executable content, and unauthorized private-file reads; verify storage names are randomized, physical paths stay hidden, and files are not served as executable content;
- SignalR authentication, request membership, group joins, sender identity, inputs, reconnect, and rejection of forged request IDs;
- sensitive-data handling in application logs, browser storage, rendered HTML, error pages, and generated reports.

Use dependency/security scanners only against a safe checkout and isolated environment. Any active probing or load generation must stay on localhost or a purpose-built disposable test deployment.

### Performance and resilience

First record an uncontended baseline, then agree service-level thresholds before using them as a release gate. Use seeded datasets at representative and larger sizes to measure dashboard, list/search/pagination, customer history, quotation/billing, report/PDF, and notification routes. Track response time percentiles, query count/latency, memory, and failures.

Exercise slow or unavailable optional dependencies, database timeout/connection interruption, repeated form submission, browser refresh/back, reconnect, and partial-operation failures. Confirm useful error feedback, no partial financial/inventory writes, no sensitive exception details, and successful retry where the operation is safe to retry. Stress tests should be bounded and isolated; do not invent a production-capacity claim from a local run.

## 5. Role and cross-role scenario matrix

### Public and account journeys

- Register with valid and invalid data; verify field-level and summary validation, duplicate email handling, password confirmation, and server-side enforcement.
- Sign in/out, wrong password, lockout, expired/invalid invitation setup link, reused invitation link, password change, and return-to-role-dashboard behavior.
- Verify anonymous redirects and that post-login return paths cannot redirect to an unsafe external destination.

### Customer journeys

- Update profile/settings and notification preferences; reject email collisions and overposted role/availability/user identifiers.
- Add/edit/remove devices within allowed rules; verify another customer's device IDs are inaccessible.
- Create a service request, select/supply the intended details, review validation, and confirm it appears in the dashboard/history with the expected initial state.
- Follow assignment, schedule, progress, chat, quotation, deliverable, completion-review, invoice, and payment-state changes; check every step reflects the latest server state after reload.
- Approve/reject quotation and handle payment-state retries; verify the server-calculated total and duplicate submission behavior.
- Cancel or request changes at each permitted lifecycle point; ensure cancelled work no longer leaks into active billing, notifications, reports, or dashboards and inventory reservations are released where required.
- Open support, respond/read notifications, and access private files; confirm all data remains scoped to the signed-in Customer.

### Technician journeys

- View schedule and assigned-job list; verify only currently assigned active requests and correct empty/overlap states.
- Open an assigned job, record allowed progress/checklist/handover data, request/submit quotation details, communicate through chat, and upload a deliverable.
- Verify work cannot start or complete before financial/workflow gates, checklist requirements, or customer-review rules are satisfied.
- Attempt stale, repeated, forged, cancelled, archived, and reassigned job operations; expect a safe rejection and no persisted change.
- Confirm profile/settings, notifications, and files are scoped to the technician's own account and authorized assignments.

### Administrator journeys

- Check dashboard totals, period filters, revenue/status chart, empty data, date boundaries, and navigation to the source records.
- Create/manage users and invitations, enforce role assignment, archive/restore/permanently-delete only where explicitly permitted, and validate audit trail/related-data handling.
- Review customer profile/history, manage requests, assign/reassign/reschedule technicians, and verify conflict detection under normal and concurrent writes.
- Review/approve/reject quotations and manage billing states; verify calculations, allowed transitions, cancellation cleanup, and audit attribution.
- Create/edit/archive inventory, adjust/use stock, inspect movement history, and verify every quantity change has item, signed delta, reason/source, actor, time, and related request where relevant.
- Generate reports and PDFs for each available period/type; validate totals against fixture rows, pagination/filter behavior, export readability, empty states, and stored report history.
- Update application/company/security/notification settings; verify persistence, public branding/support details, registration gate, defaults/ranges, and audit records for sensitive setting changes.
- Process support and notifications; confirm no cross-role or private-data leakage.

### Cross-role lifecycle and edge cases

- Complete at least one clean request-to-review-to-billing journey with all three roles, checking the same status, schedule, quotation total, invoice state, and notifications from each perspective.
- Repeat with rejected quotation, customer cancellation before and after assignment, technician reassignment, schedule conflict, technician unavailable, failed/duplicate update, and a request with no deliverable.
- Confirm every state transition has exactly the expected notification/audit side effects and no duplicate side effects on retry.
- Exercise same-time updates by two tabs/users to confirm stale forms fail safely and current persisted state remains authoritative.

## 6. Proposed execution order and commands

1. **Preflight:** inspect the effective database target without printing secrets; confirm the DB is disposable, fixture reset is available, external integrations are stubbed or disabled, and the expected test accounts exist.
2. **Fast checks:** from the repository root run `dotnet build HomeServeIT.slnx --no-restore` and `dotnet test HomeServeIT.slnx --no-restore`.
3. **Provider checks:** with `HOMESERVE_TEST_CONNECTION` set from a secret store to a local disposable MySQL server, run the `MySqlConcurrencyTests` filter documented in `AGENTS.md`; never use an unknown/shared connection.
4. **Browser suite:** from `HomeServeIT.Web`, run `npm test` only after confirming its Development app will use a safe test database. It starts the configured Playwright browser matrix unless `HOMESERVE_BASE_URL` points at an already-running safe test target. Supply authenticated credentials through environment variables, never command arguments or source files.
5. **Isolated destructive workflows:** run `npm run test:phases` with its dedicated disposable fixture and verify cleanup occurred, including after a forced test failure.
6. **Manual release pass:** execute the role journeys and UI/accessibility checks above, then bounded performance/resilience checks against the isolated test environment.
7. **Triage and retest:** record each issue with role, URL/action, fixture state, expected/actual result, severity, browser/device, sanitized evidence, and whether the failure affected persisted state. Fixes receive a focused regression test and rerun their relevant layer plus the P0 regression suite.

## 7. Pass criteria

Release readiness requires all of the following:

- all P0 scenarios pass, including negative role/ownership cases and transaction/rollback checks;
- the three-role end-to-end lifecycle passes from a clean disposable fixture;
- no open critical/high-severity security or data-integrity defects;
- no serious/critical automated accessibility findings on covered pages, plus no unresolved keyboard blocker on a core journey;
- no unexplained 5xx responses, unhandled browser errors, duplicate financial/stock side effects, negative stock, unauthorized data exposure, or test-data leakage;
- migration replay/upgrade and the supported browser matrix pass in the isolated environment;
- performance results are recorded against explicit, agreed thresholds, with failures and environment limits disclosed;
- the fixture/database and uploaded test artifacts are cleaned up, and the test report names the exact commit, browser/OS, database provider, commands, skipped checks, and known limitations.

Any waived check needs an owner, reason, compensating check, and expiry/revisit point. A green automated suite is not a substitute for the manual role and accessibility pass.

## 8. Existing automation and current gaps to close

The repository currently contains xUnit tests for account/profile, settings, authorization boundaries, technician assignment/handover/quotation, inventory, notifications, reports, dashboard, archive pagination, uploads, completion timing, and optional MySQL concurrency. Playwright currently covers public smoke, account lifecycle, selected administrator/customer/technician flows, uploads, responsive shells, keyboard dialogs, and axe checks. `playwright.config.js` defines Chromium, Firefox, desktop WebKit, and mobile WebKit; `npm run test:phases` is the isolated disposable-MySQL browser path.

Expand coverage by mapping each scenario above to an existing test or a new test before release. Based on the current browser spec inventory, the largest end-to-end gaps to verify/close are full cross-role service lifecycle coverage, customer device/support/billing journeys, technician schedule and job-state edge cases, report/PDF correctness in-browser, complete route-by-route responsive/accessibility review, migration upgrade coverage, and broader security/performance/resilience checks. Some may already have lower-level unit coverage; record the layer and assertion rather than counting file names as proof.

## 9. Test report template

For each run record:

```text
Commit/build:
Environment (local/disposable only):
Database provider and fixture ID:
Browsers/devices:
Commands and manual scenarios:
Passed / failed / skipped:
P0/P1 defects and data-integrity impact:
Accessibility and performance results:
Cleanup verified:
Known limitations / waived checks:
```
