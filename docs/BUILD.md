# Building the legacy solution (pre-Sprint 4)

Proven 2026-07-23 on Windows 11 with VS2022 Build Tools (MSBuild 17.14).
These are the only verified steps; if you change them, prove them first and
update this file (repo rule: no guessed build commands).

Result to expect: 7/7 projects build clean (warnings only, all pre-existing
test-code warnings); **113 NUnit tests, 113 passing**.

## Why stock tooling fails

Two vendor retirements break a stock 2026 build of this 2013-era solution:

1. **`Microsoft.WebApplication.targets` is absent** from VS2022 Build Tools
   (and from Visual Studio unless the legacy web workload component is
   installed). `SocialGoal.Web.csproj` imports it → `MSB4226`.
2. **The .NET Framework 4.5 targeting pack is retired.** A
   `Reference Assemblies\...\v4.5` folder may exist but contain only XML doc
   stubs, so the build fails `MSB3644` even though the folder looks installed.

Both are solved with pinned NuGet shim packages -- no admin installs, works
identically on `windows-latest` CI.

## Prerequisites

- Windows with VS2022 Build Tools (or full VS) -- MSBuild 17.x. Locate it via
  `vswhere -latest -products * -requires Microsoft.Component.MSBuild`.
- `nuget.exe` (https://dist.nuget.org/win-x86-commandline/latest/nuget.exe).
- No SQL Server needed to build or run the unit tests (controller fixtures,
  fully mocked).

## Steps (PowerShell, repo root)

```powershell
$msbuild = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" `
  -latest -products * -requires Microsoft.Component.MSBuild `
  -find MSBuild\Current\Bin\MSBuild.exe

# 1. Restore app packages (the MSB4226 complaint during restore is benign --
#    restore falls back to packages.config mode, which is correct here)
nuget restore source\SocialGoal.sln -NonInteractive

# 2. Restore the three build/test shims (pinned) into .buildtools\ (gitignored)
nuget install MSBuild.Microsoft.VisualStudio.Web.targets -Version 14.0.0.3 -OutputDirectory .buildtools -NonInteractive
nuget install Microsoft.NETFramework.ReferenceAssemblies.net45 -Version 1.0.3 -OutputDirectory .buildtools -NonInteractive
nuget install NUnit.Runners -Version 2.6.4 -OutputDirectory .buildtools -NonInteractive

# 3. Build
& $msbuild source\SocialGoal.sln /p:Configuration=Release /m /v:minimal `
  /p:VSToolsPath="$PWD\.buildtools\MSBuild.Microsoft.VisualStudio.Web.targets.14.0.0.3\tools\VSToolsPath" `
  /p:TargetFrameworkRootPath="$PWD\.buildtools\Microsoft.NETFramework.ReferenceAssemblies.net45.1.0.3\build"

# 4. Test (NUnit 2.6.3 suites need the retired 2.x console runner --
#    modern runners and `dotnet test` cannot execute them)
& .buildtools\NUnit.Runners.2.6.4\tools\nunit-console.exe `
  source\SocialGoal.Tests\bin\Release\SocialGoal.Tests.dll /framework:net-4.5 /noshadow
```

## Notes

- Web project output lands in `source\SocialGoal\bin\` (web-app layout), not
  `bin\Release\`; class libraries use `bin\Release\`.
- NuGet restore emits an audit report of known-vulnerable packages (NU1902/
  NU1903). Expected at baseline -- that list is the Sprint 1 SBOM/SCA input,
  remediated per the epic (Sprint 4 critical subset, Phase 2 wholesale).
- Running the *app* (not the build) needs local SQL Server and carries the
  destructive-initializer warning -- see the repo `CLAUDE.md`. Never point the
  app at a database you care about.
- CI mirrors these exact steps: `.github/workflows/legacy-ci.yml`.
