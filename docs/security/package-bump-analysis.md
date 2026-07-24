# Package bump analysis -- Sprint 4 (2026-07-24)

The epic's Sprint 4 remit: remediate the critical subset of package
vulnerabilities that can move without breaking the legacy app, and record
breaking-bump analysis for the rest. This is that record. Versions are pinned
centrally in `source/Directory.Packages.props` (D13); audit gate state is in
`sca-baseline.md` and `nuget-audit-baseline.txt`.

## Moved in Sprint 4

| Package | From | To | Notes |
|---|---|---|---|
| EntityFramework | 6.0.2-beta1 / 6.0.1 / 6.0.0 | 6.5.2 | R-004/R-012 unification at latest stable EF6. One generated-DDL change surfaced and was accepted (D14: `AspNetUsers.UserName` NOT NULL). Assembly version stays 6.0.0.0; no redirect churn. |
| Newtonsoft.Json | 5.0.6 | 13.0.4 | Clears GHSA-5crp-9r3c-p9vr. Binding redirect to 13.0.0.0 added in Web.config for older binders (MvcMailer et al.). |
| Microsoft.Owin (+ Host.SystemWeb, Security, Security.Cookies/OAuth/Google/Facebook/Twitter/MicrosoftAccount) | 2.0.0 | 4.2.3 | Clears GHSA-hxrm-9w7p-39cc and GHSA-3rq8-h3gj-r5c6. Binding redirects added for the four assemblies Identity.Owin 1.0.0 binds at 2.0.0.0. One forced source edit: the dead parameterless `UseGoogleAuthentication()` (OpenID 2.0) no longer exists (see D5 note). Proven by build + 187/187 + register/login smoke. |
| bootstrap, jQuery, jQuery.Validation, Microsoft.jQuery.Unobtrusive.Validation, Modernizr, Respond, T4Scaffolding.Core | (various) | REMOVED | Content-only packages: under PackageReference they deliver no assets, and the app has always served the vendored copies in `Scripts/`/`Content/` (tracked by the retire.js baseline until Sprint 12). Removing them clears their audit findings honestly -- the shipped bytes are unchanged and stay on the retire.js register. |

## Not moved -- analysis

| Package | Pinned | Fixed-in | Why the bump breaks | Disposition |
|---|---|---|---|---|
| AutoMapper | 3.1.1-ci1000 | 13.0.1+ (GHSA-rvv3-g6hj-g44x) | Every fixed version postdates the 4.2/5.0 API break: the static `Mapper.CreateMap`/`Mapper.Map` surface the app and Web.Core mapping layer are built on was removed in favor of instance `MapperConfiguration`. A bump is a rewrite of the mapping layer, not a version edit. | Phase 2: the mapping layer is rebuilt with the controllers (Sprints 9-11); AutoMapper is replaced or re-adopted at current version there. Note the pin is a CI-feed prerelease (`-ci1000`) that still resolves from nuget.org. |
| Microsoft.AspNet.Identity.Owin (+ Identity.Core, Identity.EntityFramework) | 1.0.0 | 2.x (GHSA-25c8-p796-jg6r) | Identity 2.x changes `IdentityUser` and the `AspNet*` table shapes (new columns: Email, EmailConfirmed, LockoutEndDateUtc, TwoFactorEnabled, ...). That mutates the schema baseline of record and the EF model hash -- exactly the drift the Phase 0 net pins against. A "security bump" here is a data-model migration in disguise. | Sprint 8 replaces the whole stack with ASP.NET Core Identity under the Sprint 3 auth characterization suite; doing it twice (1.0 -> 2.x -> Core) buys risk, not safety. |
| Microsoft.AspNet.Mvc (System.Web.Mvc 5.0.0) + Razor/WebPages/Helpers | 5.0.0 / 3.0.0 | n/a (no open advisory at baseline audit) | A 5.2.9/5.3.0 bump is routinely safe but is not vulnerability-driven, touches the Web.config binding redirects and view engine assemblies, and the entire System.Web MVC surface retires in Sprints 9-11. | Left at baseline: zero-advisory churn on a component scheduled for deletion. Revisit only if an advisory lands before Sprint 11. |
| Autofac / Autofac.Mvc5 | 3.1.5 / 3.0.0 | n/a (no advisory) | Autofac 4+ reworks the container API (builder/registration changes ripple through Bootstrapper.cs). | Composition root is absorbed into the .NET 10 host's built-in DI at Sprint 5 (epic); no interim bump. |
| elmah.corelibrary / Elmah.MVC | 1.2.2 / 2.1.1 | n/a (no advisory) | ELMAH is effectively frozen upstream; it was locked behind Admin auth in Sprint 1. | Replaced by the modern host's error handling + observability stack (Sprint 5 / Sprint 13). |
| MvcMailer | 4.5 | n/a (no advisory) | Unmaintained; tied to System.Web Razor rendering. | Email path is decision D7 (Sprint 13); rebuilt or no-op'd there. |
| PagedList / PagedList.Mvc | 1.17.0.0 / 4.5.0.0 | n/a (no advisory) | Unmaintained but inert. | Superseded by pagination-before-materialization in the EF Core rework (Sprints 6-7). |
| WebGrease, Antlr, Microsoft.Web.Infrastructure, Owin (interface) | pinned | n/a | Transitive/support packages of the System.Web stack. | Retire with their consumers in Phase 2/3. |

## Net effect on the audit gate

`nuget-audit-baseline.txt` shrinks from 8 entries to 2 (AutoMapper,
Microsoft.AspNet.Identity.Owin), both with a scheduled structural resolution
rather than a version bump. Baseline shrink requires no decision (sca-baseline
gate semantics); the two survivors are re-justified above.
