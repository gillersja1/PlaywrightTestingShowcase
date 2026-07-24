CI guidance — configuring REQRES_API_KEY for write tests
===============================================

Overview
--------
The API tests in tests/Api.Tests include both read (GET) and write (POST/PUT/DELETE) tests.
Write tests require a valid reqres API key. If the REQRES_API_KEY environment variable is not present, write tests will be skipped automatically.

Recommended GitHub Actions workflow snippet
------------------------------------------
Save the repository secret REQRES_API_KEY (Repository > Settings > Secrets and variables > Actions > New repository secret).
Below is a minimal GitHub Actions job that sets the secret as an environment variable and runs the tests:

```yaml
name: CI
on: [push, pull_request]

jobs:
  test:
	runs-on: ubuntu-latest
	steps:
	  - uses: actions/checkout@v4
	  - name: Setup .NET
		uses: actions/setup-dotnet@v4
		with:
		  dotnet-version: '8.0.x'
	  - name: Install Playwright browsers
		run: dotnet tool install --global Microsoft.Playwright.CLI || true; playwright install --with-deps
	  - name: Run API tests
		env:
		  REQRES_API_KEY: ${{ secrets.REQRES_API_KEY }}
		run: dotnet test tests/Api.Tests/Api.Tests.csproj --logger "trx"
```

Notes for other CI systems
-------------------------
- Azure Pipelines: add a pipeline variable (secret) named REQRES_API_KEY and reference it as $(REQRES_API_KEY) in the pipeline environment.
- GitLab CI: add a CI/CD variable REQRES_API_KEY (protected if desired) and reference it via $REQRES_API_KEY.

Security
--------
- Store the API key as a secret/variable in your CI provider; do not commit keys to the repository.
- Limit the scope of the key if the provider supports it. Prefer using a key created for CI rather than a personal key.

Local development
-----------------
Set REQRES_API_KEY as an environment variable before running write tests locally. Example (PowerShell):

```powershell
$env:REQRES_API_KEY = 'your_api_key_here'
dotnet test tests/Api.Tests/Api.Tests.csproj
```

If the key is not set, write tests will be skipped and read-only tests will still run.
