# Sprint backlog -- status mirror

Scope source of truth: `docs/SocialGoal_Modernization_Epic.md` (do not change scope
here; see `ai-context/README.md`). Status values: `todo` / `active` / `done` /
`blocked(reason)`.

## Phase 0 -- Safety gate

| Sprint | Theme | Status | Exit gate |
|---|---|---|---|
| 1 | Containment + reproducible legacy build (CI, initializer off, ELMAH locked, URL import flagged off, SBOM) | done | Clean CI build from fresh clone; initializer unreachable; ELMAH locked; SBOM committed |
| 2 | Safety net I: data-layer characterization, schema snapshot, trigger question | done | **Gate PASSED 2026-07-24**: Data 72% line (was 0), CI-enforced floor (`docs/coverage-baseline.md`); schema baseline of record committed with checksum + drift tests (`docs/schema/`); trigger unknown CLOSED in writing, empirical + by-construction (`docs/schema/triggers.md`); post-merge CI green @ f618d84; tag `s2-gate` |
| 3 | Safety net II: authz matrix + CSRF characterization; test-infra refresh | done | **Gate PASSED 2026-07-24 -- PHASE 0 COMPLETE**: 187/187 in CI (16 surface + 27 behavioral matrix tests, `docs/security/authorization-matrix.md`); all three R-007 seams lit (data 87.6%, auth surface pinned incl. proven-dead filters, triggers closed S2); structural-work sign-off granted at gate (D11 pending operator ratification); post-merge CI green @ 1359be1; tag `s3-gate` |

## Phase 1 -- Foundation retarget

| Sprint | Theme | Status | Exit gate |
|---|---|---|---|
| 4 | SDK-style conversion; platform-agnostic libs toward net10.0; EF unified stable; critical vuln subset | done | **Gate PASSED 2026-07-24**: all 7 csproj SDK-style (verified); Central Package Management (EF 6.5.2 + D14, Katana 4.2.3, Newtonsoft 13.0.4); `SocialGoal.Core` compiles on net10.0 -- CI step "Prove SocialGoal.Core on .NET 10" = success @ 40fa23a; legacy app builds + 187/187 suite green; nuget-audit baseline 8->2; post-merge both CI lanes green @ 40fa23a; tag `s4-gate` |
| 5 | Modern .NET 10 host + gating spikes (EF Core mapping, Identity hash compat, one read-only slice) | done | **Gate PASSED 2026-07-24 -- PHASE 1 COMPLETE**: ADR-001 recorded + ACCEPTED at gate; all three spikes proven with tests in `src/SocialGoal.Web.Tests/` (schema parity: 35 cols/7 FKs exact vs baseline; Identity 1.0 hash verified + v3 rehash by Core `PasswordHasher`; `/Goal/Details` slice served over WebApplicationFactory -- epic's first true HTTP test); modern-ci lane green; all 3 CI lanes green @ 44866f0; D15 ratified; legacy-publish regression accepted document-only; tag `s5-gate` |

## Phase 2 -- The two big rebuilds

| Sprint | Theme | Status | Exit gate |
|---|---|---|---|
| 6-7 | EF6 -> EF Core (schema-preserving baseline migration, async services, retire repo/UoW ceremony) | active (S6 gate PASSED) | Data characterization tests green on EF Core vs SQL Server; no undocumented schema drift. **S6 share PASSED 2026-07-24** (merged 97d3953, all 3 CI lanes green; modern-ci 30/30): 30-table model at exact parity via `Database.Migrate()` (153 cols/30 PKs/28 FKs, 26-index delta pinned), Baseline migration + HasData seed, in-suite mutation-tested drift check, characterization port with 3 documented EF6->EF Core deltas; D17 ratified; tag `s6-gate`. **S7 (remaining) = async query/service layer, repo/UoW retirement, GetGoalsByPage/GoalRepositoryCharacterization port, unconstrained-column decision** -> block gate closes at S7 |
| 8 | ASP.NET Core Identity re-platform (string IDs, hash compat, single DbContext, cookie hardening) | todo | Sprint 3 auth suite green on Core host; password-compat test passes; logout POST-only |
| 9 | Web rebuild slices 1-3: read-only profiles/goals; goal CRUD/updates/comments/support; dashboard | todo | Per-slice: authz matrix enforced, verb/CSRF green, Playwright journey green |
| 10 | Web rebuild slices 4-5: social graph; groups (admin-gated mutations) | todo | Same per-slice gates |
| 11 | Web rebuild slices 6-7: invitations/tokens, image upload (ImageSharp), search, charts, session flow | todo | Zero System.Web references; legacy Web/Web.Core/Tests retired; full matrix green |

## Phase 3 -- Cleanup and close-out

| Sprint | Theme | Status | Exit gate |
|---|---|---|---|
| 12 | Front-end consolidation (static assets, one jQuery, Bootstrap per D6, delete script museum) | todo | One current copy of each dependency; no vendored vulnerable assets; UI parity vs golden paths |
| 13 | Dependency residue + observability (MailKit, paging, AutoMapper stable, Serilog/health, arch test) | todo | SCA clean; observability live; architecture test in CI |
| 14 | Hardening, cutover, close-out (security headers, restore drill, legacy deletion, re-score) | todo | Legacy gone from build; all suites green; close-out review vs epic doc |
