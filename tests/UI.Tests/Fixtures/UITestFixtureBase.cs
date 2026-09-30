using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using System.Diagnostics;

namespace UI.Tests.Fixtures;

/// <summary>
/// Base fixture for all UI tests with common setup/teardown and helper methods.
/// </summary>
public abstract class UITestFixtureBase : PageTest
{
    protected Stopwatch TestStopwatch { get; private set; } = null!;

    [SetUp]
    public virtual void BaseSetUp()
    {
        TestStopwatch = Stopwatch.StartNew();
    }

    [TearDown]
    public virtual void BaseTearDown()
    {
        TestStopwatch.Stop();
        TestContext.Progress.WriteLine($"Test duration: {TestStopwatch.ElapsedMilliseconds}ms");

        if (TestContext.CurrentContext.Result.Outcome.Status.ToString() == "Failed")
        {
            CaptureScreenshot();
        }
    }

    /// <summary>
    /// Capture screenshot for debugging failed tests.
    /// </summary>
    protected async Task CaptureScreenshot()
    {
        try
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var filename = Path.Combine("bin", "Debug", "net8.0", $"screenshot_{timestamp}_{TestContext.CurrentContext.Test.Name}.png");
            Directory.CreateDirectory(Path.GetDirectoryName(filename)!);
            await Page.ScreenshotAsync(new PageScreenshotOptions { Path = filename });
            TestContext.Progress.WriteLine($"Screenshot saved: {filename}");
        }
        catch (Exception ex)
        {
            TestContext.Progress.WriteLine($"Failed to capture screenshot: {ex.Message}");
        }
    }

    /// <summary>
    /// Assert that page loaded within acceptable time.
    /// </summary>
    protected async Task AssertPageLoadTimeAsync(int maxMilliseconds = 5000)
    {
        var stopwatch = Stopwatch.StartNew();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        stopwatch.Stop();

        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(maxMilliseconds), 
            $"Page should load within {maxMilliseconds}ms, but took {stopwatch.ElapsedMilliseconds}ms");
    }

    /// <summary>
    /// Retry operation with exponential backoff.
    /// </summary>
    protected async Task<T> RetryAsync<T>(Func<Task<T>> operation, int maxRetries = 3, int initialDelayMs = 500)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                return await operation();
            }
            catch (PlaywrightException ex) when (i < maxRetries - 1)
            {
                var delay = initialDelayMs * (int)Math.Pow(2, i);
                TestContext.Progress.WriteLine($"Retry {i + 1}/{maxRetries} after {delay}ms: {ex.Message}");
                await Task.Delay(delay);
            }
        }
        throw new InvalidOperationException($"Operation failed after {maxRetries} retries");
    }

    /// <summary>
    /// Wait for element to be stable (visible and no animations).
    /// </summary>
    protected async Task WaitForElementStableAsync(ILocator element, int timeoutMs = 5000)
    {
        await element.WaitForAsync(new LocatorWaitForOptions { Timeout = timeoutMs });
        await Task.Delay(100); // Wait for animations to complete
    }
}
