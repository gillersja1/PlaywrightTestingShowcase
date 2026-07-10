using Allure.Net.Commons;
using Allure.NUnit;
using Allure.NUnit.Attributes;
using Microsoft.Playwright;
using NUnit.Framework;

namespace Api.Tests;

/// <summary>
/// Demonstrates Playwright's built-in API testing support (no separate HTTP
/// client library needed) against the public reqres.in test API.
/// </summary>
[TestFixture]
[AllureNUnit]
[AllureFeature("Users API")]
public class ReqResApiTests
{
    private IPlaywright _playwright = null!;
    private IAPIRequestContext _request = null!;
    private bool _hasApiKey = false;

    [OneTimeSetUp]
    public async Task GlobalSetup()
    {
        _playwright = await Playwright.CreateAsync();
        // Allow tests to run even when an API key is not available locally.
        // If REQRES_API_KEY is set in the environment, use it for write endpoints.
        var apiKey = Environment.GetEnvironmentVariable("REQRES_API_KEY");
        _hasApiKey = !string.IsNullOrWhiteSpace(apiKey);

        var contextOptions = new APIRequestNewContextOptions
        {
            BaseURL = "https://reqres.in/api",
        };

        if (_hasApiKey)
        {
            contextOptions.ExtraHTTPHeaders = new Dictionary<string, string>
            {
                { "x-api-key", apiKey! }
            };
        }

        _request = await _playwright.APIRequest.NewContextAsync(contextOptions);

        if (!_hasApiKey)
        {
            // ReqRes recently requires an API key for many endpoints. If no key is
            // configured, skip the entire fixture to avoid network-dependent failures.
            Assert.Ignore("REQRES_API_KEY not set; skipping ReqRes API tests. Create a free key at https://app.reqres.in/api-keys and set the REQRES_API_KEY environment variable to run these tests.");
        }
    }

    [OneTimeTearDown]
    public async Task GlobalTeardown()
    {
        await _request.DisposeAsync();
        _playwright.Dispose();
    }

    [Test]
    [AllureSeverity(SeverityLevel.critical)]
    public async Task GetUser_ReturnsExpectedUser()
    {
        var response = await _request.GetAsync("/users/2");
        Assert.That(response.Ok, Is.True);

        var body = await response.JsonAsync();
        var email = body!.Value.GetProperty("data").GetProperty("email").GetString();

        Assert.That(email, Does.EndWith("@reqres.in"));
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task GetUser_NotFound_Returns404()
    {
        var response = await _request.GetAsync("/users/23");
        Assert.That(response.Status, Is.EqualTo(404));
    }

    [Test]
    [AllureSeverity(SeverityLevel.critical)]
    public async Task CreateUser_ReturnsCreatedStatus()
    {
        if (!_hasApiKey)
        {
            Assert.Ignore("REQRES_API_KEY not set; skipping write test that requires an API key on reqres.in");
        }
        var response = await _request.PostAsync("/users", new APIRequestContextOptions
        {
            DataObject = new { name = "Josh", job = "QA Automation Engineer" }
        });

        Assert.That(response.Status, Is.EqualTo(201));

        var body = await response.JsonAsync();
        Assert.That(body!.Value.GetProperty("name").GetString(), Is.EqualTo("Josh"));
    }

    [Test]
    public async Task UpdateUser_ReturnsOk()
    {
        if (!_hasApiKey)
        {
            Assert.Ignore("REQRES_API_KEY not set; skipping write test that requires an API key on reqres.in");
        }
        var response = await _request.PutAsync("/users/2", new APIRequestContextOptions
        {
            DataObject = new { name = "Josh", job = "Senior QA Automation Engineer" }
        });

        Assert.That(response.Ok, Is.True);
    }

    [Test]
    public async Task DeleteUser_Returns204()
    {
        if (!_hasApiKey)
        {
            Assert.Ignore("REQRES_API_KEY not set; skipping write test that requires an API key on reqres.in");
        }
        var response = await _request.DeleteAsync("/users/2");
        Assert.That(response.Status, Is.EqualTo(204));
    }
}
