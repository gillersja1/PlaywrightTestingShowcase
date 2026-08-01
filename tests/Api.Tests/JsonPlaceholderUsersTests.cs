using Allure.Net.Commons;
using Allure.NUnit;
using Allure.NUnit.Attributes;
using Microsoft.Playwright;
using NUnit.Framework;

namespace Api.Tests;

/// <summary>
/// Tests for JSONPlaceholder /users endpoint.
/// Demonstrates retrieving user information.
/// </summary>
[TestFixture]
[AllureNUnit]
[AllureFeature("JSONPlaceholder - Users")]
public class JsonPlaceholderUsersTests
{
    private IPlaywright _playwright = null!;
    private IAPIRequestContext _request = null!;

    [OneTimeSetUp]
    public async Task GlobalSetup()
    {
        _playwright = await Playwright.CreateAsync();
        var contextOptions = new APIRequestNewContextOptions
        {
            BaseURL = "https://jsonplaceholder.typicode.com",
        };

        _request = await _playwright.APIRequest.NewContextAsync(contextOptions);
    }

    [OneTimeTearDown]
    public async Task GlobalTeardown()
    {
        await _request.DisposeAsync();
        _playwright.Dispose();
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task GetUsers_ReturnsMultipleUsers()
    {
        var response = await _request.GetAsync("/users");
        Assert.That(response.Ok, Is.True);

        var body = await response.JsonAsync();
        var userCount = body!.Value.GetArrayLength();

        Assert.That(userCount, Is.GreaterThan(0), "Should return multiple users");
    }

    [Test]
    [AllureSeverity(SeverityLevel.critical)]
    public async Task GetUser_ReturnsUserDetails()
    {
        var response = await _request.GetAsync("/users/1");
        Assert.That(response.Ok, Is.True);

        var body = await response.JsonAsync();
        var userId = body!.Value.GetProperty("id").GetInt32();
        var userName = body.Value.GetProperty("name").GetString();

        Assert.That(userId, Is.EqualTo(1));
        Assert.That(userName, Is.Not.Empty);
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task GetUserProfile_ContainsExpectedFields()
    {
        var response = await _request.GetAsync("/users/1");
        Assert.That(response.Ok, Is.True);

        var body = await response.JsonAsync();
        var user = body!.Value;

        Assert.That(user.TryGetProperty("id", out _), Is.True, "Should have id");
        Assert.That(user.TryGetProperty("name", out _), Is.True, "Should have name");
        Assert.That(user.TryGetProperty("email", out _), Is.True, "Should have email");
        Assert.That(user.TryGetProperty("username", out _), Is.True, "Should have username");
    }
}
