using Allure.Net.Commons;
using Allure.NUnit;
using Allure.NUnit.Attributes;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using UI.Tests.PageObjects;

namespace UI.Tests;

[TestFixture]
[AllureNUnit]
[AllureFeature("Login")]
public class SauceDemoLoginTests : PageTest
{
    private LoginPage _loginPage = null!;
    private InventoryPage _inventoryPage = null!;

    [SetUp]
    public void Setup()
    {
        _loginPage = new LoginPage(Page);
        _inventoryPage = new InventoryPage(Page);
    }

    [Test]
    [AllureSeverity(SeverityLevel.critical)]
    public async Task StandardUser_CanLogin_AndSeeInventory()
    {
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync("standard_user", "secret_sauce");

        await Expect(Page).ToHaveURLAsync("https://www.saucedemo.com/inventory.html");
        Assert.That(await _inventoryPage.GetPageTitleAsync(), Is.EqualTo("Products"));
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task LockedOutUser_SeesErrorMessage()
    {
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync("locked_out_user", "secret_sauce");

        var error = await _loginPage.GetErrorMessageAsync();
        Assert.That(error, Does.Contain("locked out"));
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task StandardUser_CanAddItemToCart()
    {
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync("standard_user", "secret_sauce");

        await _inventoryPage.AddItemToCartAsync("Sauce Labs Backpack");

        Assert.That(await _inventoryPage.GetCartCountAsync(), Is.EqualTo(1));
    }
}
