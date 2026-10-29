# Test Strategy

How this project decides what to test, at which level, and what to do when a
test fails. The code shows *how* the tests work; this document explains *why*
they look the way they do.

## Systems under test and their risks

None of the targets are ours, so the strategy is shaped as much by what we
**can't** control as by what we want to cover.

| Target | What it is | Main risks to the product | Main risks to the tests |
|---|---|---|---|
| [saucedemo.com](https://www.saucedemo.com/) | Stable demo store with demo accounts that have deliberately planted bugs | Wrong totals/tax at checkout, broken auth, cart state lost between pages, inaccessible UI | Background analytics keep the network busy, so "network idle" is a poor readiness signal |
| [never-deliver-shop.vercel.app](https://never-deliver-shop.vercel.app/) | Live store whose catalog is fetched client-side from third-party APIs | Pricing and currency errors, checkout validation gaps, accessibility of forms | Catalog order and stock levels change; upstream catalog APIs occasionally stall |
| [JSONPlaceholder](https://jsonplaceholder.typicode.com/) | Public fake REST API | Contract changes: status codes, response shape | Writes are faked and not persisted; occasional 5xx from the public service |
| `CalculatorLib` | In-repo class library | Arithmetic edge cases (division by zero) | None: fully deterministic |

Highest product risk is **money and checkout**: totals, tax, currency and
required-field validation get the most cases, including data-driven boundary
cases. Lowest is static content, which gets a smoke check at most.

## What runs at which level

| Level | Covers | Why at this level |
|---|---|---|
| Unit | Calculator logic and its edge cases | Fast, deterministic; logic belongs here, not in a browser |
| API | Status codes, response shape, filtering, CRUD semantics | Checks the contract without UI cost or flakiness |
| UI / E2E | User journeys: login, browse, cart, checkout, order history | Only the browser can prove the pieces work together |
| Accessibility | axe-core WCAG 2.1 A/AA scan of each screen, plus keyboard and screen-reader behavior | A rules engine catches markup issues; scripted keyboard tests catch what it can't |
| Performance (indicative) | Page and action response times under a single user | Catches gross regressions early; real load testing lives in the separate [k6](https://github.com/gillersja1/K6LoadTestExample) and [JMeter](https://github.com/gillersja1/JMeterLoadTestExample) projects |

UI tests are the most expensive, so they cover journeys, not every input
permutation. Input permutations are data-driven (`[TestCase]`, `[Values]`)
so one journey covers many cases.

### Deliberately out of scope

- **Payment processing and real orders**: neither store takes real payments.
- **Load and stress**: one browser can't generate meaningful load; see the
  k6 and JMeter projects.
- **Persistence through the API**: JSONPlaceholder fakes writes, so API tests
  assert the response, not that data was stored.
- **Visual regression**: the live catalog changes too often for stable
  screenshots. It would be worth adding for SauceDemo's static pages.
- **Security testing**: no authorization to scan third-party sites.

## Assertions that survive live data

Tests against live sites must not depend on data they don't own:

- **Assert relationships, not fixed values.** For example, cart subtotal
  equals the sum of line totals, and the result count drops after filtering.
- **Pick test data at run time.** Quantity tests choose products without a
  low-stock warning instead of "the first product", because the catalog order
  and stock change.
- **Wait for the state, not the network.** Readiness means a specific element
  is visible (`Expect(...).ToBeVisibleAsync()`), never a fixed sleep or
  network idle.

## Flaky tests

A flaky test is a bug, either in the test or the product, and gets
root-caused like one. **There are no blanket test retries**: an automatic
retry hides the problem and slows down every real failure.

The process:

1. **Reproduce.** Rerun the single test repeatedly (`--filter` plus a loop),
   and on the CI browser if it only fails there. Traces and videos are
   recorded for every UI test.
2. **Classify.** Is it the test (timing, locator, data assumption), the
   product (a real intermittent bug), or the environment (upstream outage)?
3. **Fix the cause in the page object** so every test benefits.
4. **Prove the fix.** Run it at least five times in a row. Where possible,
   force the failure condition, e.g. block the upstream API to prove a
   recovery path works.

Examples fixed in this repo:

| Symptom | Root cause | Fix |
|---|---|---|
| Quantity "+" button disabled in CI only | Live stock changed, so the first products were "Only 1 left" | Choose well-stocked products at run time |
| NeverDeliver setup timeouts under parallel CI load | Client-side catalog fetch from a third-party API occasionally stalls | Reload once, then fail with a clear message. Verified by blocking the APIs in a probe test |
| Product-link check failed randomly with "null" | URL changes before the old list unmounts, and both views share a `data-test` value | Locate by a class only the details page uses |
| Login looked slow for every user | Timer waited for network idle, which includes background analytics | Stop the timer when the product list is visible |

An **environment** failure (e.g. JSONPlaceholder returning 502) is not fixed
in the test: rerun the failed job, and if it persists, report it upstream.

## Defects and the KnownDefect pattern

When a test finds a real product bug, the test stays and keeps asserting the
**correct** behavior. It's tagged `[Category("KnownDefect")]` and CI runs it
in a separate, non-blocking step:

- The build stays green, so the bug doesn't block unrelated work.
- The failure stays visible in TRX and Allure reports.
- When the product is fixed, the test starts passing, which is the signal to
  remove the tag. A KnownDefect test that unexpectedly passes is
  investigated, not ignored.

`KnownDefect` is **only** for confirmed product bugs. It is never used to
quarantine a flaky test; that would hide a test problem behind a product
label.

### Triage

Every new failure gets answered in this order:

1. **Is the test right?** Check the trace: did the test do what a user would
   do, and is the expectation correct per the site's own behavior or spec?
2. **Does it reproduce manually?** If yes, it's a product defect.
3. **Severity** based on user impact:

| Severity | Meaning | Example |
|---|---|---|
| Critical | Blocks a core journey or loses money/data | Checkout can't complete |
| Major | Core journey works but gives a wrong result | Wrong total or tax |
| Normal | Feature degraded, workaround exists; includes WCAG A/AA failures | Form labels not announced to screen readers |
| Minor | Cosmetic or wording | Wrong currency symbol on a surcharge label |

4. **Record it.** Defect ID in the README table, test name, severity, and a
   one-line description of the user impact.

The SauceDemo **user-contract suite** applies the same idea at scale: every
demo account runs the same checks as `standard_user`, and each account's
planted bugs are listed in one map. The result is a defect matrix showing
exactly which account breaks which behavior. A new bug in any account fails
the build; a planted bug that disappears shows up as a KnownDefect test that
unexpectedly passes.

## Quality gates

| Gate | Blocks merge? |
|---|---|
| Unit tests | Yes |
| API tests | Yes |
| UI tests on Chromium, Firefox and WebKit (excluding KnownDefect) | Yes |
| KnownDefect tests | No: reported only |
| Allure report | No: published on pushes to `main` |

`main` is protected: every change goes through a pull request, and
force-pushes and branch deletion are blocked.

## When to revisit this strategy

- A target site changes structure or starts requiring authentication.
- A test needs a retry to pass, which means the cause hasn't been found yet.
- CI time grows past about 20 minutes per browser, the point at which to
  consider sharding or moving journeys down to the API level.
