# ADR-001 -- Modern host architecture (Sprint 5)

Status: DRAFT (spike evidence pending) · 2026-07-24 · Sprint 5
Decisions referenced: D0 (single epic to .NET 10), D1 (no live data),
D2 (Azure App Service Linux, private), D15 (modern solution layout).

## Context

Phase 1 exit / LMRR Phase 2 entry requires a proven auth and data approach and
one production-shaped vertical slice on the modern stack before the rebuild
budget (Sprints 6-11) is committed. The legacy app (ASP.NET MVC 5, EF6,
Identity 1.0, .NET Framework 4.8 after Sprint 4) keeps running from `source/`;
the modern host grows in `src/` (D15) until the Sprint 11 cutover.

## Host architecture

| Concern | Choice | Rationale |
|---|---|---|
| Framework | ASP.NET Core MVC on net10.0 | D0; LMRR Phase 1 target. MVC (controllers + Razor views) mirrors the legacy app's shape, keeping slice-by-slice parity reviewable. |
| Composition root | `Program.cs` minimal hosting | Absorbs `Bootstrapper.cs` wiring. Built-in DI container; Autofac only if a demonstrated need survives (none expected -- legacy registrations are plain per-request mappings). |
| Configuration | `appsettings.json` + environment variables; user-secrets in dev | No secrets in source (hard rule). Azure App Service app settings at deploy (D2). |
| Data Protection | Persisted key ring, path from configuration (`DataProtection:KeyPath`) | Survives restarts so auth cookies stay valid. Azure Blob key store at deploy time per D2; file system locally. |
| Error handling | `UseExceptionHandler` + status-code pages in non-dev; developer exception page in dev | Replaces ELMAH + `customErrors`. Observability backend lands Sprint 13 (D8). |
| Health | `MapHealthChecks("/health")` | D2 deploy prerequisite (App Service health check). |
| Logging | Serilog (`Serilog.AspNetCore`), console sink, request logging | Skeleton only this sprint; App Insights sink is a Sprint 13 (D8) concern. |
| Tests | NUnit 4.x (D15) | Mechanical port path for the 187-test legacy suite. |

## Data approach (EF Core spike)

Target: EF Core against the Sprint 2/D14 schema baseline
(`docs/schema/schema-baseline.sql`), table/column names and string user IDs
preserved, no key-type changes (epic Sprints 6-7 constraints).

**Spike evidence (PASSED 2026-07-24):**
`src/SocialGoal.Web.Tests/Spikes/SchemaParitySpikeTests.cs` creates one LocalDB
database from `schema-baseline.sql` and one from the EF Core model
(`src/SocialGoal.Web/Data/SocialGoalDbContext.cs`: Goal, GoalStatus, Metric,
ApplicationUser, FollowUser), then compares live catalogs. Columns (35),
PK column sets, and all 7 FKs match exactly -- including `datetime` (not
`datetime2`), exact `nvarchar` facets, the Identity-1.0 TPH artifacts on
AspNetUsers (nullable ApplicationUser columns + `Discriminator nvarchar(128)
NOT NULL`), EF6's shadow FK columns `ApplicationUser_Id`/`_Id1` on FollowUsers
with their EF6 constraint names, and cascade parity. Only divergence: EF Core
adds 7 FK indexes (additive, desirable, pinned by test so the delta cannot
grow silently) -- carried to Sprints 6-7 as a documented addition. Mapping
requirements the spike hardened into config: `HasColumnType("datetime")`
everywhere, explicit `HasMaxLength`, explicit `HasConstraintName` per FK,
explicit `ToTable` (EF6 pluralization quirks), and deliberate reproduction of
the TPH-nullable columns a naive mapping would emit NOT NULL (schema drift).

## Auth approach (Identity spike)

Target: ASP.NET Core Identity over the existing `AspNetUsers` shape (string
IDs). Identity 1.0 password hashes are ASP.NET Identity "v2" format
(PBKDF2-HMAC-SHA1, 128-bit salt, 1000 iterations, `0x00` marker), which Core
Identity's `PasswordHasher` verifies natively and reports
`SuccessRehashNeeded` -- rehash-on-login upgrades users to v3 transparently.

**Spike evidence (PASSED 2026-07-24):**
`src/SocialGoal.Web.Tests/Spikes/IdentityPasswordCompatSpikeTests.cs`. The
test hash was produced by the real legacy stack (Microsoft.AspNet.Identity.Core
1.0.0 net45 assembly on the .NET Framework 4.8 CLR -- provenance in the test),
not a reimplementation. Proven: (1) Core Identity's `PasswordHasher` verifies
it and reports `SuccessRehashNeeded`; (2) wrong passwords fail; (3) the full
`UserManager.CheckPasswordAsync` path over the legacy-shaped AspNetUsers table
(baseline schema, string IDs, LocalDB) authenticates and transparently
rewrites the stored hash to v3 format (marker 0x01), which then verifies as
plain `Success`. Sprint 8 needs no custom compatibility hasher -- the built-in
compat path suffices; the spike's minimal user store is throwaway scaffolding
the real Sprint 8 store replaces.

## Vertical slice (proof shape)

Goal detail page rendered by the Core host from a database carrying the real
schema (created from `schema-baseline.sql`, seeded). Production-shaped:
controller -> EF Core query -> Razor view, `[Authorize]` by default, no
System.Web.

**Spike evidence (pending).**

## Consequences

- Two solutions, two CI lanes until Sprint 11 (D15).
- If any spike fails, the epic's rebuild budget is not committed until the
  approach is revised (epic Sprint 5 exit gate).
