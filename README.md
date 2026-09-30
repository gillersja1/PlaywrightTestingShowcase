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
│   ├── UI.Tests/                      # Playwright UI tests (saucedemo.com & neverdeliver.co.uk) + Page Objects
│   │   ├── PageObjects/               # Enhanced page object models (Login, Inventory, Cart, Checkout)
│   │   ├── Fixtures/                  # UITestFixtureBase with retry logic, screenshot capture
│   │   ├── TestData/                  # Test data builder and constants
│   │   ├── SauceDemo/                 # Organized test suites by domain
│   │   │   ├── Authentication/        # 9 comprehensive auth tests
│   │   │   ├── Shopping/              # 10 shopping & inventory tests
│   │   │   ├── Cart/                  # 10 cart management tests
│   │   │   ├── Checkout/              # 10 checkout validation tests
│   │   │   ├── Accessibility/         # 7 accessibility/WCAG tests
│   │   │   └── Performance/           # 8 performance & load tests
│   │   ├── Never​Deliver*/            # Tests for secondary site (organized similarly)
│   │   ├── allureConfig.json
│   │   └── playwright.runsettings     # Browser selection, trace/video capture config
│   └── Api.Tests/                     # Playwright API tests (jsonplaceholder.typicode.com) + allureConfig.json
├── docs/
│   └── TRACE_VIDEO_OUTPUT_GUIDE.md   # Guide for accessing and using trace/video artifacts
├── .github/
│   ├── copilot-instructions.md       # Team coding guidelines
│   └── workflows/ci.yml              # CI pipeline (unit, API, UI x3 browsers, report)
└── PlaywrightTestingShowcase.sln
```

## What each project tests

| Project | Type | Target | Test Coverage |
|---|---|---|---|
| `CalculatorLib.UnitTests` | Unit | `CalculatorLib` (in-repo class library) | Calculator operations |
| `UI.Tests` | UI / E2E | [saucedemo.com](https://www.saucedemo.com/) | **54 comprehensive tests**: Authentication (9), Shopping (10), Cart (10), Checkout (10), Accessibility (7), Performance (8) |
| `UI.Tests` | UI / E2E | [neverdeliver.co.uk](https://neverdeliver.co.uk/) | Authentication, Shopping, Basket management, Order completion (22 tests) |
| `Api.Tests` | API | [JSONPlaceholder](https://jsonplaceholder.typicode.com/) — free fake API for testing | Posts, Comments, Users, Todos, Albums, Photos (23 tests) |

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

# Run all UI tests (defaults to chromium — see playwright.runsettings with video+trace capture)
dotnet test tests/UI.Tests/UI.Tests.csproj --settings tests/UI.Tests/playwright.runsettings

# Run UI tests against a specific browser
dotnet test tests/UI.Tests/UI.Tests.csproj --settings tests/UI.Tests/playwright.runsettings -- Playwright.BrowserName=firefox

# Run only SauceDemo tests
dotnet test tests/UI.Tests/UI.Tests.csproj --filter "SauceDemo" --settings tests/UI.Tests/playwright.runsettings

# Run only SauceDemo Authentication tests (9 tests)
dotnet test tests/UI.Tests/UI.Tests.csproj --filter "SauceDemoAuthententicationTests" --settings tests/UI.Tests/playwright.runsettings

# Run only SauceDemo Shopping tests (10 tests)
dotnet test tests/UI.Tests/UI.Tests.csproj --filter "SauceDemoShoppingTests" --settings tests/UI.Tests/playwright.runsettings

# Run only SauceDemo Cart tests (10 tests)
dotnet test tests/UI.Tests/UI.Tests.csproj --filter "SauceDemoCartTests" --settings tests/UI.Tests/playwright.runsettings

# Run only SauceDemo Checkout tests (10 tests)
dotnet test tests/UI.Tests/UI.Tests.csproj --filter "SauceDemoCheckoutValidationTests" --settings tests/UI.Tests/playwright.runsettings

# Run only Accessibility tests (7 tests)
dotnet test tests/UI.Tests/UI.Tests.csproj --filter "SauceDemoAccessibilityTests" --settings tests/UI.Tests/playwright.runsettings

# Run only Performance tests (8 tests)
dotnet test tests/UI.Tests/UI.Tests.csproj --filter "SauceDemoPerformanceTests" --settings tests/UI.Tests/playwright.runsettings

# Run only NeverDeliver tests
dotnet test tests/UI.Tests/UI.Tests.csproj --filter "NeverDeliver" --settings tests/UI.Tests/playwright.runsettings

# Run API tests
dotnet test tests/Api.Tests/Api.Tests.csproj

# Run API tests against JSONPlaceholder
dotnet test tests/Api.Tests/Api.Tests.csproj --filter "JsonPlaceholder" --settings tests/Api.Tests/jsonplaceholder.runsettings

# View captured trace files (videos and traces automatically saved to bin/Debug/net8.0/)
npx playwright show-trace tests/UI.Tests/bin/Debug/net8.0/trace/trace.zip
```

> If `pwsh` isn't installed, grab it from the
> [PowerShell installation docs](https://learn.microsoft.com/powershell/scripting/install/installing-powershell),
> or use the equivalent `playwright install` command shown in the
> [Playwright .NET docs](https://playwright.dev/dotnet/docs/browsers).

## UI Test Suites

### SauceDemo Tests (54 comprehensive tests)
Tests the [saucedemo.com](https://www.saucedemo.com/) e-commerce site with extensive coverage:

#### Authentication & Login (9 tests)
- **SauceDemoAuthenticationTests** — User validation, empty fields, special characters, keyboard shortcuts
  - Valid login, locked out users, invalid input handling
  - Boundary testing with special chars and very long usernames
  - Alternative login methods (Enter key support)

#### Shopping & Inventory (10 tests)
- **SauceDemoShoppingTests** — Product discovery and cart operations
  - Product display and pricing validation
  - Add/remove items, multiple item handling
  - Cart badge updates, persistence across navigation
  - Problem user scenario testing

#### Shopping Cart (10 tests)
- **SauceDemoCartTests** — Cart management and calculations
  - Item count verification
  - Total calculation (subtotal + tax precision validation)
  - Tax computation, multiple items handling
  - Item removal and cart state transitions

#### Checkout & Validation (10 tests)
- **SauceDemoCheckoutValidationTests** — Order completion workflows
  - Required field validation (First Name, Last Name, Postal Code)
  - Empty field error handling
  - Special character and long name handling
  - Cart state verification during checkout

#### Accessibility (7 tests)
- **SauceDemoAccessibilityTests** — WCAG compliance and screen reader support
  - Input labels and ARIA attributes
  - Cart badge accessibility
  - Interactive element text content
  - Form input accessibility (placeholders, labels)
  - Error message visibility for screen readers
  - Proper heading hierarchy
  - Image alt text verification

#### Performance (8 tests)
- **SauceDemoPerformanceTests** — Load times and response metrics
  - Page load time assertions (< 5 seconds)
  - Add-to-cart response time (< 2 seconds)
  - Cart/checkout page performance
  - Product visibility timing
  - Response time consistency across operations

### NeverDeliver Tests (22 tests)
Tests the [neverdeliver.co.uk](https://neverdeliver.co.uk/) e-commerce site:
- **NeverDeliverLoginTests** — User login and validation
- **NeverDeliverShoppingTests** — Product browsing and catalog features
- **NeverDeliverBasketTests** — Shopping basket management
- **NeverDeliverCheckoutTests** — Complete order workflows

All suites use the **Page Object Model** pattern with dedicated, enhanced page objects for better maintainability and reusability.

## API Test Suites

### JSONPlaceholder Tests (23 tests)
Tests the [JSONPlaceholder](https://jsonplaceholder.typicode.com/) free fake REST API.
Organized into resource-focused test classes for better maintainability:

- **JsonPlaceholderPostsTests** (7 tests) — GET, POST, PUT, PATCH, DELETE operations on posts
- **JsonPlaceholderCommentsTests** (3 tests) — Retrieve and create comments
- **JsonPlaceholderUsersTests** (3 tests) — User information and profile validation
- **JsonPlaceholderTodosTests** (3 tests) — Todo resources with filtering
- **JsonPlaceholderMediaTests** (7 tests) — Albums and photos with filtering

JSONPlaceholder is ideal for API testing because it requires no authentication, provides realistic data structures, and supports full CRUD operations.

## Test infrastructure & features

### Trace & Video Capture
All UI tests automatically capture:
- **Video recordings** (`.webm` format) — Full browser screen recordings for every test
- **Detailed traces** (`.zip` format) — Playwright trace archives containing DOM snapshots, network logs, console messages, screenshots, and action timeline

Videos and traces are saved to `tests/UI.Tests/bin/Debug/net8.0/` and are invaluable for debugging failed tests:
```bash
# View a trace file (requires Node.js/npm)
npx playwright show-trace <path-to-trace.zip>
```

Configure recording in `tests/UI.Tests/playwright.runsettings`:
- `RecordVideo` and `RecordTrace` set to `on` (all tests)
- Can be set to `retain-on-failure` to save disk space in CI

See [docs/TRACE_VIDEO_OUTPUT_GUIDE.md](docs/TRACE_VIDEO_OUTPUT_GUIDE.md) for complete details.

### Test Fixture Base & Helpers
`UITestFixtureBase` provides:
- Automatic screenshot capture on test failure
- Page load time assertions with configurable timeouts
- Retry logic with exponential backoff for flaky operations
- Test performance tracking and logging
- Derived test classes benefit from all infrastructure automatically

### Test Data & Constants
`TestDataBuilder` centralizes:
- Login credentials (valid user, locked out user, problem user, invalid inputs)
- Checkout information (valid, invalid, edge cases with special characters)
- Product names and constants
- Error message expectations
- All test scenarios reusable across multiple test classes

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
- **api-tests** — runs `Api.Tests` against JSONPlaceholder
- **ui-tests** — matrix job, runs `UI.Tests` against Chromium, Firefox, and WebKit in parallel
- **allure-report** — waits on all of the above, merges results, and publishes the combined report

All jobs upload `.trx` and Allure result artifacts even on failure, so you can
inspect what went wrong without re-running anything.

> **Note:** publishing to GitHub Pages requires enabling Pages for the repo
> (Settings → Pages → deploy from the `gh-pages` branch) and — on a public
> repo — the default `GITHUB_TOKEN` permissions are usually sufficient.

## Possible extensions

- Add comprehensive API integration tests with error scenarios
- Add a `docker-compose.yml` to run tests in containers
- Add a nightly scheduled run in addition to push/PR triggers
- Add visual regression testing with Playwright's screenshot comparisons
- Integrate flaky-test quarantine tagging and retry strategies via Allure
- Expand test coverage for additional edge cases and security scenarios
- Add load/stress testing with Playwright
- Add contract testing between UI and API layers
- Implement test environment management (local, staging, production)
