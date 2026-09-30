using Allure.Net.Commons;
using Allure.NUnit;
using Allure.NUnit.Attributes;
using NUnit.Framework;
using UI.Tests.Fixtures;
using UI.Tests.PageObjects;
using UI.Tests.TestData;

namespace UI.Tests.SauceDemo.Authentication;

[TestFixture]
[AllureNUnit]
[AllureFeature("Authentication")]
[AllureSuite("SauceDemo")]
public class SauceDemoAuthenticationTests : UITestFixtureBase
{
    private LoginPage _loginPage = null!;
    private InventoryPage _inventoryPage = null!;

    [SetUp]
    public void Setup()
    {
        BaseSetUp();
        _loginPage = new LoginPage(Page);
        _inventoryPage = new InventoryPage(Page);
    }

    [Test]
    [AllureSeverity(SeverityLevel.critical)]
    public async Task ValidUser_CanLogin()
    {
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync(LoginCredentials.ValidUser.Username, LoginCredentials.ValidUser.Password);

        Assert.That(await _inventoryPage.GetPageTitleAsync(), Is.EqualTo("Products"));
        Assert.That(Page.Url, Does.Contain("inventory"));
    }

    [Test]
    [AllureSeverity(SeverityLevel.critical)]
    public async Task LockedOutUser_CannotLogin()
    {
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync(LoginCredentials.LockedOutUser.Username, LoginCredentials.LockedOutUser.Password);

        Assert.That(await _loginPage.IsErrorMessageVisibleAsync(), Is.True);
        var error = await _loginPage.GetErrorMessageAsync();
        Assert.That(error, Does.Contain("locked out").IgnoreCase);
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    [TestCaseSource(nameof(GetInvalidLoginCredentials))]
    public async Task LoginWithInvalidFields_ShowsError(LoginCredentials creds)
    {
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync(creds.Username, creds.Password);

        Assert.That(await _loginPage.IsErrorMessageVisibleAsync(), Is.True);
        var error = await _loginPage.GetErrorMessageAsync();
        Assert.That(error, Does.Contain(creds.ExpectedError!).IgnoreCase);
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task EmptyUsername_ShowsError()
    {
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync("", "secret_sauce");

        Assert.That(await _loginPage.IsErrorMessageVisibleAsync(), Is.True);
        Assert.That(await _loginPage.GetErrorMessageAsync(), Does.Contain("username").IgnoreCase);
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task EmptyPassword_ShowsError()
    {
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync("standard_user", "");

        Assert.That(await _loginPage.IsErrorMessageVisibleAsync(), Is.True);
        Assert.That(await _loginPage.GetErrorMessageAsync(), Does.Contain("password").IgnoreCase);
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task EmptyBothFields_ShowsError()
    {
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync("", "");

        Assert.That(await _loginPage.IsErrorMessageVisibleAsync(), Is.True);
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task Login_ViaEnterKey()
    {
        await _loginPage.GotoAsync();
        await _loginPage.LoginViaEnterKeyAsync(LoginCredentials.ValidUser.Username, LoginCredentials.ValidUser.Password);

        Assert.That(await _inventoryPage.GetPageTitleAsync(), Is.EqualTo("Products"));
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task LoginPageDisplaysCorrectly()
    {
        await _loginPage.GotoAsync();

        Assert.That(await _loginPage.IsLoginPageDisplayedAsync(), Is.True);
        Assert.That(await _loginPage.IsLoginButtonEnabledAsync(), Is.True);
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task SpecialCharactersInUsername_HandledCorrectly()
    {
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync("user@#$%", "password");

        // Should either show error or handle gracefully
        Assert.That(await _loginPage.IsLoginPageDisplayedAsync() || await _loginPage.IsErrorMessageVisibleAsync());
    }

    private static IEnumerable<LoginCredentials> GetInvalidLoginCredentials()
    {
        return LoginCredentials.InvalidInputs;
    }
}
