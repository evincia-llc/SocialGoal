# Building the solution (SDK-style, Sprint 4 onward)

Proven 2026-07-24 on Windows 11 with VS2022 Build Tools (MSBuild 17.14) and the
.NET 10 SDK (10.0.302). These are the only verified steps; if you change them,
prove them first and update this file (repo rule: no guessed build commands).
CI mirrors these exact steps: `.github/workflows/legacy-ci.yml`.

Result to expect: 7/7 projects build clean (warnings only, all pre-existing
test-code warnings plus the known NU1903 audit set); **187 NUnit tests, 187
passing**; `SocialGoal.Core` additionally compiles for `net10.0`.

## Shape of the build (Sprint 4, D13)

All seven projects are SDK-style. Six use `Microsoft.NET.Sdk`; the Web project
uses `MSBuild.SDK.SystemWeb/4.0.107` (pinned in the csproj `Sdk` attribute)
because plain SDK projects cannot host System.Web web applications. Package
versions live centrally in `source/Directory.Packages.props`; every project
has a committed `packages.lock.json` and CI restores in locked mode. There is
no `packages.config` and no `packages/` folder anymore.

Two toolchain facts shape the steps (journal 2026-07-24):

1. **Desktop MSBuild only for the solution.** The SystemWeb SDK does not work
   with `dotnet build`. The web-application targets it needs are fed from the
   pinned `MSBuild.Microsoft.VisualStudio.Web.targets` NuGet package (a
   dependency of the Web project), so no VS web workload or manual shim
   install is required.
2. **The dotnet CLI only for net10.0.** The .NET 10 SDK requires MSBuild 18,
   which desktop MSBuild 17 is not. `SocialGoal.Core`'s `net10.0` flavor is
   therefore opt-in (`-p:IncludeNet10=true`) and built as a separate step; the
   solution build stays net48 everywhere.

## Prerequisites

- Windows with VS2022 Build Tools (or full VS) -- MSBuild 17.x. Locate it via
  `vswhere -latest -products * -requires Microsoft.Component.MSBuild`.
- .NET 10 SDK (for the `SocialGoal.Core` net10.0 proof step).
- `nuget.exe` only to install the pinned NUnit console runner (test step).
- No SQL Server needed to build or run the unit tests other than LocalDB
  (`MSSQLLocalDB`) for the characterization suites.

## Steps (PowerShell, repo root)

```powershell
$msbuild = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" `
  -latest -products * -requires Microsoft.Component.MSBuild `
  -find MSBuild\Current\Bin\MSBuild.exe

# 1. Restore (PackageReference + central versions + committed lock files)
& $msbuild source\SocialGoal.sln /t:Restore /p:Configuration=Release /p:RestoreLockedMode=true /v:minimal

# 2. Build
& $msbuild source\SocialGoal.sln /p:Configuration=Release /m /v:minimal

# 3. Prove the first .NET 10 assembly (opt-in flavor; scratch lock path keeps
#    the committed net48 lock file untouched)
dotnet build source\SocialGoal.Core\SocialGoal.Core.csproj -c Release -f net10.0 `
  -p:IncludeNet10=true "-p:NuGetLockFilePath=$env:TEMP\core-net10.lock.json"

# 4. Test (NUnit 3 console runner; note the net48 TFM subfolder in the path)
sqllocaldb start MSSQLLocalDB
nuget install NUnit.ConsoleRunner -Version 3.22.0 -OutputDirectory .buildtools -NonInteractive
& .buildtools\NUnit.ConsoleRunner.3.22.0\tools\nunit3-console.exe `
  source\SocialGoal.Tests\bin\Release\net48\SocialGoal.Tests.dll --result=TestResult.xml
```

## Modern solution (`src/`, Sprint 5 onward)

The .NET 10 solution (`src/SocialGoal.Modern.slnx`, D15) is dotnet-CLI-only --
desktop MSBuild cannot build it, and the dotnet CLI cannot build the legacy
solution; the toolchain boundary is the directory boundary. CI:
`.github/workflows/modern-ci.yml`.

```powershell
# Restore (CPM + committed lock files, same locked posture as the legacy lane)
dotnet restore src\SocialGoal.Modern.slnx --locked-mode

# Build -- TreatWarningsAsErrors + latest-recommended analyzers are on, so a
# clean build is also the analyzer gate
dotnet build src\SocialGoal.Modern.slnx -c Release --no-restore

# Formatting gate
dotnet format src\SocialGoal.Modern.slnx --verify-no-changes --no-restore

# Tests (spike + slice suites create throwaway LocalDB catalogs from
# docs/schema/schema-baseline.sql and drop them afterwards)
sqllocaldb start MSSQLLocalDB
dotnet test src\SocialGoal.Modern.slnx -c Release --no-build
```

Run the host locally: `dotnet run --project src\SocialGoal.Web` (Development
uses the `SocialGoal_ModernDev` LocalDB catalog from
`appsettings.Development.json`; `/health` answers without a database).

## Notes

- Web project output lands in `source\SocialGoal\bin\` (classic web layout,
  assembly `SocialGoal.dll`); class libraries use `bin\Release\net48\`.
- Restore emits the accepted NU1903 audit warnings (AutoMapper,
  Microsoft.AspNet.Identity.Owin) -- the Sprint 4 residue documented in
  `docs/security/sca-baseline.md` and gated by
  `docs/security/nuget-audit-baseline.txt`.
- Running the *app* needs local SQL Server (`Data Source=.\`). The committed
  Debug config creates and seeds the `SocialGoal` database only if missing
  (never drops or alters); the Release transform disables initialization.
  A dev database created before Sprint 4 fails at startup with "the model
  backing SocialGoalEntities has changed" -- EF 6.5.2 changed the generated
  model (D14). Drop the old dev database and let the app recreate it.
- Smoke-run: `& "C:\Program Files\IIS Express\iisexpress.exe"
  /path:$PWD\source\SocialGoal /port:5002`, then browse
  `http://localhost:5002/`.
- Release publish (proven 2026-07-24, Sprint 5): XDT transforms DO execute
  under the SystemWeb SDK -- the published Web.config carries
  `DatabaseInitializer=None` and no `debug` attribute. Two caveats: a
  file-system publish requires **both** `/p:DeployOnBuild=true
  /p:DeployTarget=WebPublish /p:WebPublishMethod=FileSystem "/p:publishUrl=..."`
  (without `DeployTarget=WebPublish` the publish silently produces a WebDeploy
  package instead), and the SDK's default Content globs exclude `.cshtml` and
  `Scripts`/`Content`/`fonts`/`Images`, so the output is code-only and NOT a
  runnable MVC app (journal 2026-07-24; deliberately unfixed -- no legacy
  deploy path exists (D1) and the legacy Web project retires in Sprint 11).
