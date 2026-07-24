---
paths:
  - "**/*.cs"
  - "**/*.csproj"
  - "**/*.cshtml"
---

# .NET modernization conventions

## Touching legacy code (`source/`, pre-rebuild)

- Legacy code is a behavioral reference, not a trusted design. Pin behavior with
  characterization tests before changing it (Phase 0 discipline).
- Never propagate these patterns anywhere, including into tests that aren't
  explicitly pinning them: authorization by caller-supplied entity ID, mutating
  GET actions, the `GetImageFromUrl` URL fetch, `DropCreateDatabaseIfModelChanges`,
  the unregistered `SocialGoalAuthorizeAttribute`/`AntiForgeryTokenFilterProvider`.
- Keep controllers service-mediated (R-003 guardrail); don't add new
  controller -> repository shortcuts even though DI would allow it.

## New code (.NET 10 host, Sprint 5 onward)

- Authorization: policy-based with resource handlers (`CanEditGoal`,
  `IsGroupAdmin`, `IsRequestRecipient`). Owner/admin/recipient identity comes from
  the authenticated principal -- never from a posted or routed ID. Every mutation
  gets negative tests (unrelated user, non-admin member, anonymous).
- HTTP semantics: GET is side-effect free. All mutations are POST (or
  PUT/PATCH/DELETE) under global `AutoValidateAntiforgeryToken`.
- Binding: command DTOs only; domain entities are never model-bound. DTOs do not
  expose owner/user IDs the server can derive.
- Data: preserve string user IDs and existing table/column names; schema changes
  only via reviewed EF Core migrations tied to a decision in
  `ai-context/decisions.md`. Async end to end with `CancellationToken`;
  projections and `AsNoTracking` for reads; paginate before materializing;
  no generic repository over EF Core.
- Bulk operations: `ExecuteDelete`/`ExecuteUpdate` run immediately and bypass
  the change tracker -- never mix them with tracked entities in the same unit
  of work (a tracked entity can go stale and a later `SaveChanges` throw).
  Updates to a loaded entity mutate properties on the tracked instance; never
  `context.Update(detachedCopy)` (marks every column modified -- unset fields
  overwrite the row; pinned by `EfCoreDataBehaviorTests`).
- Every `datetime` column must be set explicitly via the injected clock before
  insert: entities carry no constructor date defaults, and `default(DateTime)`
  fails the insert with SqlDateTime overflow (below the 1753 floor). Loud, but
  only if a test exercises the path -- set dates at creation, always.
- Time and types: UTC via an injectable clock (no `DateTime.Now`); `decimal` for
  business measures; `string` for phone/postal values.
- Images: ImageSharp/SkiaSharp, never `System.Drawing`. Uploads validated by
  size, content signature, and decoded dimensions; generated safe filenames.
- Secrets/config from environment or secret store; nothing in committed config.
