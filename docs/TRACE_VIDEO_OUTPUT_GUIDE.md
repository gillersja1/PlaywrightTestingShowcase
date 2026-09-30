# Trace & Video Output Location Reference

## Overview
Playwright tests are configured to automatically capture trace and video files for debugging and analysis.

## Configuration
Located in: `tests/UI.Tests/playwright.runsettings`

```xml
<RecordVideo>on</RecordVideo>      <!-- Records video for ALL tests -->
<RecordTrace>on</RecordTrace>      <!-- Records detailed traces for ALL tests -->
```

## Output Locations

### Default Output Directory
```
tests/UI.Tests/bin/Debug/net8.0/
```

### Video Files
- **Format**: `.webm` (WebM codec)
- **Location**: Automatically created in test output directory
- **Naming**: Named by test class and method
- **Size**: Typically 1-10 MB per test
- **What it captures**: Full browser screen recording of test execution

### Trace Files
- **Format**: `.zip` (Playwright trace archive)
- **Location**: Automatically created in test output directory  
- **Naming**: `trace` directories organized by test
- **Contents**:
  - DOM snapshots at each action
  - Network request/response logs
  - Console messages and errors
  - Screenshots at key points
  - Timeline of all actions
  - Hover/click coordinates

## Accessing Artifacts

### View Trace Files
```powershell
# Using Playwright Inspector (requires Node.js/npm installed)
npx playwright show-trace trace.zip
```

### Locate Files in Explorer
```
File Explorer > tests\UI.Tests\bin\Debug\net8.0\
```

Or via PowerShell:
```powershell
# List all video files
Get-ChildItem -Path "tests\UI.Tests\bin\Debug\net8.0" -Filter "*.webm" -Recurse

# List all trace files  
Get-ChildItem -Path "tests\UI.Tests\bin\Debug\net8.0" -Filter "*.zip" -Recurse | Where-Object { $_.Name -like "trace*" }
```

## Test Run Scenarios

### Local Development
- Videos and traces are saved automatically after each test
- Files persist until project is cleaned (dotnet clean)
- Use for debugging failed tests in real-time

### CI/CD Pipeline
- Configure to archive artifacts after test runs
- Upload to artifact storage (Azure, GitHub, etc.)
- Include in test reports for failure analysis

### Test Analysis
- **Failed Tests**: Watch video to see exact failure point
- **Flaky Tests**: Compare traces across multiple runs
- **Performance**: Use timeline data to identify bottlenecks
- **UI Issues**: Review DOM snapshots to verify state

## Cleanup

### Remove Artifacts
```powershell
dotnet clean
```

### Selective Removal
```powershell
Remove-Item -Path "tests\UI.Tests\bin\Debug\net8.0\*.webm" -Force
Remove-Item -Path "tests\UI.Tests\bin\Debug\net8.0\*trace*" -Recurse -Force
```

## Configuration Options

### Change Recording Strategy
Current: `on` (all tests)  
Options:
- `on`: Record all tests
- `off`: Record no tests
- `retain-on-failure`: Record only failed tests (saves space)

To modify: Edit `playwright.runsettings` in the `<RecordVideo>` or `<RecordTrace>` section.

## Storage Considerations

- **Space Usage**: ~10-20 MB per test run (depends on test duration)
- **Retention**: Configure cleanup in CI/CD to manage disk space
- **Performance**: Video recording adds ~5-10% to test execution time
- **Network**: May be slow to transfer large video files in CI/CD

## Tips for Debugging

1. **Find failing test video**:
   - Run the specific failing test
   - Look for the video in bin/Debug/net8.0/
   - Open with media player or Playwright Inspector for trace

2. **Compare two runs**:
   - Save traces from both runs
   - Use `npx playwright show-trace` side-by-side
   - Identify divergence point

3. **Performance analysis**:
   - Open trace in Playwright Inspector
   - Check network tab for slow requests
   - Review DOM updates timeline

4. **CI/CD Integration**:
   - Add step to archive artifacts after test run
   - Upload to job artifacts
   - Download for local analysis if needed
