using Allure.Net.Commons;
using Allure.NUnit;
using Allure.NUnit.Attributes;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using UI.Tests.PageObjects;

namespace UI.Tests;

[TestFixture]
[AllureNUnit]
[AllureFeature("Checkout")]
public class SauceDemoCheckoutTests : PageTest
{
    private LoginPage _loginPage = null!;
    private InventoryPage _inventoryPage = null!;
    private CartPage _cartPage = null!;
    private CheckoutPage _checkoutPage = null!;

    [SetUp]
    public void Setup()
    {
        _loginPage = new LoginPage(Page);
        _inventoryPage = new InventoryPage(Page);
        _cartPage = new CartPage(Page);
        _checkoutPage = new CheckoutPage(Page);
    }

    [Test]
    [AllureSeverity(SeverityLevel.critical)]
    public async Task StandardUser_CanCheckoutWithMultipleItems()
    {
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync("standard_user", "secret_sauce");

        await _inventoryPage.AddItemToCartAsync("Sauce Labs Backpack");
        await _inventoryPage.AddItemToCartAsync("Sauce Labs Bike Light");
        await _inventoryPage.AddItemToCartAsync("Sauce Labs Bolt T-Shirt");

        await _cartPage.GotoAsync();
        var itemsInCart = await _cartPage.GetItemCountAsync();
        Assert.That(itemsInCart, Is.EqualTo(3), "Cart should have 3 items");

        await _cartPage.ClickCheckoutAsync();
        await _checkoutPage.FillInformationAsync("John", "Doe", "54321");
        await _checkoutPage.ClickContinueAsync();
        await _checkoutPage.ClickFinishAsync();

        Assert.That(await _checkoutPage.GetCompleteHeaderAsync(), Is.EqualTo("Thank you for your order!"));
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task StandardUser_CanCheckoutAfterRemovingItem()
    {
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync("standard_user", "secret_sauce");

        await _inventoryPage.AddItemToCartAsync("Sauce Labs Backpack");
        await _inventoryPage.AddItemToCartAsync("Sauce Labs Bike Light");

        await _cartPage.GotoAsync();
        await _cartPage.RemoveItemFromCartAsync("Sauce Labs Bike Light");

        var itemsInCart = await _cartPage.GetItemCountAsync();
        Assert.That(itemsInCart, Is.EqualTo(1), "Cart should have 1 item after removal");

        await _cartPage.ClickCheckoutAsync();
        await _checkoutPage.FillInformationAsync("Jane", "Smith", "98765");
        await _checkoutPage.ClickContinueAsync();
        await _checkoutPage.ClickFinishAsync();

        Assert.That(await _checkoutPage.GetCompleteHeaderAsync(), Is.EqualTo("Thank you for your order!"));
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task StandardUser_CanCheckoutWithSpecialCharactersInName()
    {
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync("standard_user", "secret_sauce");

        await _inventoryPage.AddItemToCartAsync("Sauce Labs Backpack");

        await _cartPage.GotoAsync();
        await _cartPage.ClickCheckoutAsync();

        await _checkoutPage.FillInformationAsync("José", "O'Brien-Smith", "12345");
        await _checkoutPage.ClickContinueAsync();
        await _checkoutPage.ClickFinishAsync();

        Assert.That(await _checkoutPage.GetCompleteHeaderAsync(), Is.EqualTo("Thank you for your order!"));
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task StandardUser_CanContinueWithNumericPostalCode()
    {
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync("standard_user", "secret_sauce");

        await _inventoryPage.AddItemToCartAsync("Sauce Labs Bike Light");

        await _cartPage.GotoAsync();
        await _cartPage.ClickCheckoutAsync();

        await _checkoutPage.FillInformationAsync("Bob", "Johnson", "00000");
        await _checkoutPage.ClickContinueAsync();
        await _checkoutPage.ClickFinishAsync();

        Assert.That(await _checkoutPage.GetCompleteHeaderAsync(), Is.EqualTo("Thank you for your order!"));
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task PerformanceGlitchUser_CanCompleteCheckout()
    {
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync("performance_glitch_user", "secret_sauce");

        await _inventoryPage.AddItemToCartAsync("Sauce Labs Backpack");

        await _cartPage.GotoAsync();
        await _cartPage.ClickCheckoutAsync();

        await _checkoutPage.FillInformationAsync("Test", "User", "12345");
        await _checkoutPage.ClickContinueAsync();
        await _checkoutPage.ClickFinishAsync();

        Assert.That(await _checkoutPage.GetCompleteHeaderAsync(), Is.EqualTo("Thank you for your order!"));
    }
}
