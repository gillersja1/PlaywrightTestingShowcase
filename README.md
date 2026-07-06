# Playwright .NET Testing Showcase

A portfolio project demonstrating test automation across three layers —
**unit**, **API**, and **UI** — using C#, NUnit, and
[Playwright for .NET](https://playwright.dev/dotnet/), wired up to a
GitHub Actions CI pipeline with cross-browser testing and
[Allure](https://allurereport.org/) reporting.

## Why this repo exists

This is a demo/portfolio project meant to show:

- Clean separation between "system under test" code and test code
- The Page Object Model pattern for UI tests
- Using Playwright for **both** UI automation and API testing (no separate
  HTTP client library needed)
- Cross-browser UI testing (Chromium, Firefox, WebKit) via a CI matrix
- Rich, unified test reporting across all three layers with Allure
- A working, parallelized CI pipeline that auto-publishes an HTML report

## Project structure

```
PlaywrightTestingShowcase/
├── src/
│   └── CalculatorLib/                 # Simple library acting as the "system under test"
├── tests/
│   ├── CalculatorLib.UnitTests/       # NUnit unit tests + allureConfig.json
│   ├── UI.Tests/                      # Playwright UI tests (saucedemo.com) + Page Objects
│   │   ├── PageObjects/
│   │   ├── allureConfig.json
│   │   └── playwright.runsettings     # Browser selection, overridable in CI
│   └── Api.Tests/                     # Playwright API tests (reqres.in) + allureConfig.json
├── .github/workflows/ci.yml           # CI pipeline (unit, API, UI x3 browsers, report)
└── PlaywrightTestingShowcase.sln
```

## What each project tests

| Project | Type | Target |
|---|---|---|
| `CalculatorLib.UnitTests` | Unit | `CalculatorLib` (in-repo class library) |
| `UI.Tests` | UI / E2E | [saucedemo.com](https://www.saucedemo.com/) — a public site built for test automation practice |
| `Api.Tests` | API | [reqres.in](https://reqres.in/) — a public test REST API |

## Running locally

**Prerequisites:** [.NET 8 SDK](https://dotnet.microsoft.com/download)

```bash
# Restore dependencies
dotnet restore

# Run just the unit tests
dotnet test tests/CalculatorLib.UnitTests/CalculatorLib.UnitTests.csproj

# Install Playwright browsers (only needed once, and after Playwright updates)
dotnet build tests/UI.Tests/UI.Tests.csproj
pwsh tests/UI.Tests/bin/Debug/net8.0/playwright.ps1 install --with-deps

# Run UI tests (defaults to chromium — see playwright.runsettings)
dotnet test tests/UI.Tests/UI.Tests.csproj --settings tests/UI.Tests/playwright.runsettings

# Run UI tests against a specific browser
dotnet test tests/UI.Tests/UI.Tests.csproj --settings tests/UI.Tests/playwright.runsettings -- Playwright.BrowserName=firefox

# Run API tests
dotnet test tests/Api.Tests/Api.Tests.csproj
```

> If `pwsh` isn't installed, grab it from the
> [PowerShell installation docs](https://learn.microsoft.com/powershell/scripting/install/installing-powershell),
> or use the equivalent `playwright install` command shown in the
> [Playwright .NET docs](https://playwright.dev/dotnet/docs/browsers).

## Cross-browser testing

`tests/UI.Tests/playwright.runsettings` controls which browser Playwright
launches. Locally it defaults to Chromium; in CI, a matrix strategy runs the
full UI suite against **Chromium, Firefox, and WebKit** in parallel jobs, each
overriding the setting via:

```bash
dotnet test -- Playwright.BrowserName=<chromium|firefox|webkit>
```

## Test reporting with Allure

Every test project (`Allure.NUnit`) writes raw results to an `allure-results`
folder alongside its build output. In CI, a dedicated `allure-report` job:

1. Downloads the `allure-results-*` artifacts from every unit/API/UI job
2. Merges them and generates a single combined HTML report with
   `allure-commandline`
3. Uploads the report as a build artifact
4. Publishes it to GitHub Pages on pushes to `main`

To view a report locally:

```bash
dotnet test tests/UI.Tests/UI.Tests.csproj
npx allure-commandline@2 generate tests/UI.Tests/bin/Debug/net8.0/allure-results --clean -o allure-report
npx allure-commandline@2 open allure-report
```

> Allure's commandline tool requires a Java runtime (JRE 8+) on your machine.

## CI/CD

`.github/workflows/ci.yml` runs on every push/PR to `main`:

- **unit-tests** — runs `CalculatorLib.UnitTests`
- **api-tests** — runs `Api.Tests` against reqres.in
- **ui-tests** — matrix job, runs `UI.Tests` against Chromium, Firefox, and WebKit in parallel
- **allure-report** — waits on all of the above, merges results, and publishes the combined report

All jobs upload `.trx` and Allure result artifacts even on failure, so you can
inspect what went wrong without re-running anything.

> **Note:** publishing to GitHub Pages requires enabling Pages for the repo
> (Settings → Pages → deploy from the `gh-pages` branch) and — on a public
> repo — the default `GITHUB_TOKEN` permissions are usually sufficient.

## Possible extensions

- Add a `docker-compose.yml` to run tests in containers
- Add a nightly scheduled run in addition to push/PR triggers
- Add visual regression testing with Playwright's screenshot comparisons
- Add retry logic and flaky-test quarantine tagging via Allure
