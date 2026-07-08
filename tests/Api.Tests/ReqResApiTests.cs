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

    [OneTimeSetUp]
    public async Task GlobalSetup()
    {
        _playwright = await Playwright.CreateAsync();
        _request = await _playwright.APIRequest.NewContextAsync(new APIRequestNewContextOptions
        {
            BaseURL = "https://reqres.in/api",
            ExtraHTTPHeaders = new Dictionary<string, string>
            {
                // reqres.in requires this demo key on its free tier.
                { "x-api-key", "reqres-free-v1" }
            }
        });
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
        var response = await _request.PutAsync("/users/2", new APIRequestContextOptions
        {
            DataObject = new { name = "Josh", job = "Senior QA Automation Engineer" }
        });

        Assert.That(response.Ok, Is.True);
    }

    [Test]
    public async Task DeleteUser_Returns204()
    {
        var response = await _request.DeleteAsync("/users/2");
        Assert.That(response.Status, Is.EqualTo(204));
    }
}
