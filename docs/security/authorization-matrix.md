# Authorization and CSRF characterization matrix (Sprint 3)

What the legacy SocialGoal app actually enforces, pinned by test. This is a
characterization record -- it documents current behavior, defects included, and
becomes the **enforcement spec for the Phase 2 rebuild** (epic Sprints 8-11).
Nothing here is fixed; the rebuild's job is to make every "defect" row below
fail a negative test.

Spec sources: LMRR R-007 (auth/anti-forgery dark zone) and the secondary risk
report's Gap #1 (broken object-level authorization) and Gap #2 (state-changing
GETs / CSRF). Test level per decision **D11** (`ai-context/decisions.md`): no
in-process HTTP host exists for System.Web MVC 5, so the surface is pinned by
reflection and the behavior by controller-invocation over LocalDB. True
HTTP-level tests arrive naturally in the Phase 2 rebuild (ASP.NET Core
`TestServer`), where this matrix flips from pinning to enforcement.

## How it is pinned (two layers)

1. **Declarative surface** -- `source/SocialGoal.Tests/Authorization/`
   (`ControllerSurface*`, `EnforcementDefectPin*`, `InertFilter*`): reflection
   over the `SocialGoal.Web` controller assembly diffs the actual
   `[Authorize]`/`[AllowAnonymous]`/verb/`[ValidateAntiForgeryToken]` surface,
   action by action, against a pinned table of all 149 actions. Stock attribute
   semantics are trusted, so attribute presence pins the HTTP-facing enforcement
   exactly.
2. **Behavioral matrix** -- `source/SocialGoal.Tests/Authorization/*MatrixTests`:
   real controllers wired to real services/repositories over LocalDB, invoked as
   each actor (owner / unrelated user / group member / group admin / request
   recipient), asserting persisted state. This is where the broken object-level
   authorization is demonstrated, not just inferred.

## Enforcement reality, in one paragraph

Authentication is the only boundary the app enforces, and it enforces it with
stock `[Authorize]` at the class level on six of seven controllers. There is no
global authorization filter, no global antiforgery filter, and the two custom
security filters that would have added them are dead code. Past the login wall,
any authenticated user can act on any object: ownership, group-admin, and
request-recipient checks are absent from every mutating action. `GroupUser.Admin`
is written to the database and read only to decide what the UI renders -- it
gates no server action.

## The surface, by the numbers (all pinned)

| Fact | Count | Pin |
|---|---|---|
| Controllers | 7 | `ControllerSurfaceCharacterizationTests` |
| Public actions | 149 | same (row-for-row table) |
| Mutating actions | 53 | `ExpectedControllerSurface` mutating set |
| POST actions | 32 | surface table |
| POSTs with `[ValidateAntiForgeryToken]` | 7 | `EnforcementDefectPinTests` |
| POSTs without antiforgery | 25 | same |
| **Mutating actions reachable via GET** | **23** | same |
| Controllers without `[Authorize]` | 1 (`SearchController`) | same (D4 evidence) |

The 23 mutating GETs correct the secondary report's "~17" estimate -- an
LMRR-feedback correction (`ai-context/lmrr-feedback.md`).

## Anti-forgery posture (Gap #2)

The only actions that validate an antiforgery token are 7 `AccountController`
POSTs: `Login`, `Register`, `Disassociate`, `Manage`, `ExternalLogin`,
`LinkLogin`, `ExternalLoginConfirmation`. Every other POST -- 25 of them, across
Goal, Group, and the remaining Account actions (`UploadImage`, `EditProfile`) --
accepts a cross-site-forgeable request. The `AntiForgeryTokenFilterProvider` in
`SocialGoal.Web.Core` would have attached the token check to every POST globally;
it is never registered (see Inert filters). `Bootstrapper.cs:45`'s
`RegisterFilterProvider()` is Autofac's own MVC filter provider, unrelated.

## Safe-verb violations (Gap #2): the 23 mutating GETs

State change on GET is both a CSRF multiplier (no token possible on a plain link)
and a correctness hazard (prefetch, crawlers, history). The full set:

- **Goal (5):** `SupportGoal`, `SupportGoalNow`, `UnSupportGoal`, `SupportUpdate`,
  `UnSupportUpdate`.
- **Group (10):** `DeleteMember`, `SaveUpdate` (its `[HttpPost]` is commented out,
  `GroupController.cs:485`), `InviteUser`, `JoinGroup`, `GroupJoinRequest`,
  `AcceptRequest`, `RejectRequest`, `GoalStatus`, `SupportUpdate`,
  `UnSupportUpdate`.
- **Account (6):** `LinkLoginCallback`, `LogOff` (its `[HttpPost]` and
  `[ValidateAntiForgeryToken]` are commented out, `AccountController.cs:333-334`),
  `FollowRequest`, `AcceptRequest`, `RejectRequest`, `Unfollow`.
- **EmailRequest (2):** `AddGroupUser`, `AddSupportToGoal` (token-mediated).

`LogOff`-by-GET is the canonical example: any page that embeds
`<img src="/Account/LogOff">` logs the victim out.

## Broken object-level authorization (Gap #1): the BOLA surface

Each row is an action whose target object is chosen by a caller-supplied id or
model field, with no check that the acting user owns or may administer it. The
behavioral matrix pins each by having an **unrelated authenticated user** perform
the mutation and asserting it persists.

| Action | Target bound from | What an unrelated user can do |
|---|---|---|
| `Goal.Edit` (POST) | `editGoal.GoalId` | Edit any goal |
| `Goal.GoalStatus` | `goalid` param | Set any goal's status |
| `Goal.DeleteConfirmed` | `id` route | Delete any goal |
| `Goal.EditUpdate` / `DeleteConfirmedUpdate` | update/goal id | Edit/delete any update |
| `Group.EditGroup` | `GroupId` in model | Edit any group |
| `Group.DeleteConfirmedGroup` | `id` route | Delete any group + its memberships |
| `Group.DeleteMember` (GET) | `userId`+`groupId` | Remove any member from any group |
| `Group.EditGoal` / `DeleteConfirmed` | model/route id | Edit/delete any group goal |
| `Group.GoalStatus` (GET) | `goalid` | Set any group goal's status |
| `Group.AcceptRequest` / `RejectRequest` (GET) | both party ids | Approve/reject anyone into any group |
| `Group.CreateFocus`/`EditFocus`/`DeleteConfirmedFocus` | focus/group id | Manage any group's focus |
| `Account.EditProfile` (POST) | `editedProfile.UserId` | **Edit any user's profile, name, and email** |
| `Account.AcceptRequest` / `RejectRequest` (GET) | both party ids | Forge/destroy follow relationships between arbitrary users |

Correctly-scoped counter-examples (bind actor and scope to
`User.Identity.GetUserId()`, so an unrelated user's call is a no-op) are pinned
too, as the boundary cases: `Goal.UnSupportGoal`, `Goal.UnSupportUpdate`,
`Account.Unfollow`, `Account.Manage`, `Account.Disassociate`.

## The `GroupUser.Admin` flag gates nothing

`Admin` is set true for a group's creator and false for everyone who joins. It is
read in exactly one place -- `GroupController.Index` (`:93`) -- to decide whether
the UI shows admin controls. No mutating action consults it. So group admin,
group member, and unrelated user are the **same** authorization principal on
every group operation: edit, delete, remove member, manage focus and goals,
approve and reject join requests. The behavioral matrix pins all three actors
succeeding identically.

## Inert filters (R-007): declared, never wired

- `FilterConfig.RegisterGlobalFilters` (`:8-12`) registers exactly one global
  filter, `HandleErrorAttribute`. Pinned by `InertFilterCharacterizationTests`.
- `SocialGoalAuthorizeAttribute` (`SocialGoal.Web.Core`) -- a custom authorize
  filter -- is referenced by no controller, action, or registration. Dead.
- `AntiForgeryTokenFilterProvider` (`SocialGoal.Web.Core`; the file name carries
  a trailing space) -- an `IFilterProvider` that would force antiforgery on every
  POST -- is added to no provider chain. Dead.

Their existence is the trap: a reviewer skimming the solution sees "authorize
filter" and "antiforgery provider" and assumes coverage that the wiring never
delivers.

## From pin to enforcement (Phase 2)

Each defect class maps to a rule already recorded in
`.claude/rules/modernization.md`, to be enforced during the rebuild:

| Pinned defect | Phase 2 enforcement |
|---|---|
| BOLA on 15+ mutations | Policy-based resource authorization (`CanEditGoal`, `IsGroupAdmin`, `IsRequestRecipient`); identity from the authenticated principal, never a posted/routed id |
| 23 mutating GETs | All mutations move to POST/PUT/PATCH/DELETE; GET is side-effect free |
| 25 unprotected POSTs | Global `AutoValidateAntiforgeryToken` |
| `SearchController` anonymous | D4: `[Authorize]` or a documented public decision |
| `Admin` flag ignored | Real group-admin policy check on every group mutation |
| Domain entities model-bound | Command DTOs only; DTOs never expose server-derivable owner ids |

When the rebuilt controllers exist, these same actor cases become **negative
tests**: the unrelated user, the non-admin member, and the non-recipient must be
rejected, and every mutation must reject a missing antiforgery token.
