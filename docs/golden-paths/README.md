# Golden paths -- visual reference for the rebuild (Sprint 1)

Captured 2026-07-23 against the legacy app running locally (IIS Express,
`http://localhost:8090`, fresh `SocialGoal` database created by the new
config-switched `CreateDatabaseIfNotExists` initializer -- itself live proof
that the Sprint 1 initializer replacement seeds metrics and goal statuses
correctly). Single user `goldenpath1`.

These screenshots are the UI parity reference for the Phase 2 rebuild slices
(epic Sprints 9-11) and the Sprint 12 front-end consolidation gate.

## Multi-user journeys (updated Sprint 3)

The multi-user *behavior* deferred from Sprint 1 -- follow requests, group
invitations/joins, support, and the actor-vs-object authorization outcomes -- is
now pinned by the Sprint 3 authorization matrix
(`docs/security/authorization-matrix.md`; tests in
`source/SocialGoal.Tests/Authorization/`), which stands up multiple seeded actors
and asserts what each can do to another's objects. That is the load-bearing
Sprint 3 deliverable and a stronger record than screenshots would be.

Multi-user *visual* captures (two browsers, the invite/accept UI) are **not**
captured here. They are deferred to the Phase 2 slice that rebuilds the social
graph and groups UI (epic Sprint 10), where the parity reference is actually
consumed and the second actor's screens exist in the new app. Capturing them now
against the legacy UI would date them before the rebuild uses them. Flagged for
operator confirmation at the Sprint 3 gate.

## Captures

| File | Journey point | Behavior notes |
|---|---|---|
| `01-register.jpg` | `/Account/Register` | Username + password only (no email field). Registration auto-signs-in and redirects to the dashboard. |
| `02-dashboard-empty.jpg` | `/` after first login | Feed placeholder "No feeds available. Connect with people."; Goals and Groups panels with My/Followed tabs. |
| `03-create-goal-modal.jpg` | Create > Create Goal | Modal over the current page. Metric dropdown is the seeded lookup set (%, $, $ M, Rs, Hours, Km, Kg, Years). Dates are MM/DD/YYYY text inputs with a datepicker. Private checkbox. |
| `04-goal-detail-new.jpg` | `/Goal/Index/1` after create | Redirects straight to goal detail. Panels: update composer with status-vs-target input, followers, invite-by-name autocomplete, goal-status changer (In Progress / On Hold / Completed). |
| `05-goal-update-progress-chart.jpg` | after posting an update | Full-page reload. Update appears with status "12 Km achieved out of 100 Km"; jqPlot Goal Progress chart renders (target bar at end date, achieved point at update date). |
| `06-goal-update-comment.jpg` | after commenting on the update | Comment counter increments ("1 Comments"); Support link per update. |
| `07-create-group-modal.jpg` | Create > Create Group | Name + description only. |
| `08-group-detail.jpg` | `/Group/Index/1` after create | Creator auto-added as sole member. Panels: Goals, Focus, Members, Goals Assigned By/To Me; Requests and Actions buttons (admin-only surface). |
| `09-goals-list.jpg` | `/Goal/GoalList` | Sort by Date, Filter All; sidebar mirrors My/Followed Goals and Groups with edit/delete icons. |
| `10-user-profile-activity.jpg` | `/Account/UserProfile/{guid}` | Basic/Personal info blocks (username only until profile edited), Recent Activities feed (join/comment/update/create events with relative timestamps). |
| `11-search-results.jpg` | navbar search "run" | Three-column results: Goals / People / Groups. Served by `SearchController` -- note it is anonymous-capable in the legacy app (D4). |
| `12-dashboard-populated.jpg` | `/` with content | Feed shows own group-join activity; goal and group listed in panels. |
| `13-elmah-locked-login-redirect.jpg` | `/elmah` | Sprint 1 containment proof: previously anonymous, now 401 -> login redirect (`ReturnUrl=/elmah`) even for an authenticated non-Admin user (role gate). Also serves as the login-page visual reference. |

## Observed behaviors worth preserving/knowing

- Every mutation is a full-page reload; no SPA behavior anywhere.
- Goal creation lands on the goal detail page, not the list.
- The update composer couples the update text with a numeric "new status"
  against the goal target; the chart derives from those statuses.
- Session cookie survives IIS Express restarts (OWIN cookie, machine-key
  independent) -- re-login was not required across app restarts.
- First request after a cold start takes ~30s+ (EF database create + Razor
  view compilation); subsequent pages are fast.
