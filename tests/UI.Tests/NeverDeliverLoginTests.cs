using Allure.Net.Commons;
using Allure.NUnit;
using Allure.NUnit.Attributes;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using UI.Tests.PageObjects;

namespace UI.Tests;

[TestFixture]
[AllureNUnit]
[AllureFeature("Never Deliver - Authentication")]
public class NeverDeliverLoginTests : PageTest
{
    private NeverDeliverLoginPage _loginPage = null!;

    [SetUp]
    public void Setup()
    {
        _loginPage = new NeverDeliverLoginPage(Page);
    }

    [Test]
    [AllureSeverity(SeverityLevel.critical)]
    public async Task User_CanNavigateToLoginPage()
    {
        await _loginPage.GotoAsync();

        Assert.That(await _loginPage.IsLoginPageDisplayedAsync(), Is.True, "Login page should be displayed");
    }

    [Test]
    [AllureSeverity(SeverityLevel.critical)]
    public async Task ValidUser_CanLogin()
    {
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync("test@example.com", "password");

        // After login, user should be redirected away from login page
        Assert.That(Page.Url, Does.Not.Contain("login"), "User should be redirected from login page");
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task InvalidCredentials_ShowsErrorMessage()
    {
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync("wrong@example.com", "wrongpassword");

        var errorMessage = await _loginPage.GetErrorMessageAsync();
        Assert.That(errorMessage.Length, Is.GreaterThan(0), "Error message should be displayed");
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task EmptyEmail_ShowsValidationError()
    {
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync("", "password");

        var errorMessage = await _loginPage.GetErrorMessageAsync();
        Assert.That(errorMessage.Length, Is.GreaterThan(0), "Validation error should be displayed");
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task EmptyPassword_ShowsValidationError()
    {
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync("test@example.com", "");

        var errorMessage = await _loginPage.GetErrorMessageAsync();
        Assert.That(errorMessage.Length, Is.GreaterThan(0), "Validation error should be displayed");
    }
}
