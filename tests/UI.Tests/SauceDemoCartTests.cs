using Allure.Net.Commons;
using Allure.NUnit;
using Allure.NUnit.Attributes;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using UI.Tests.PageObjects;

namespace UI.Tests;

[TestFixture]
[AllureNUnit]
[AllureFeature("Cart/Checkout")]
public class SauceDemoCartTests : PageTest
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
    [AllureSeverity(SeverityLevel.normal)]
    public async Task StandardUser_CanRemoveItemFromCart()
    {
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync("standard_user", "secret_sauce");

        await _inventoryPage.AddItemToCartAsync("Sauce Labs Backpack");
        await _inventoryPage.AddItemToCartAsync("Sauce Labs Bike Light");

        await _cartPage.GotoAsync();
        Assert.That(await _cartPage.GetItemCountAsync(), Is.EqualTo(2));

        await _cartPage.RemoveItemFromCartAsync("Sauce Labs Backpack");
        Assert.That(await _cartPage.GetItemCountAsync(), Is.EqualTo(1));
    }

    [Test]
    [AllureSeverity(SeverityLevel.critical)]
    public async Task StandardUser_CanCompleteCheckout()
    {
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync("standard_user", "secret_sauce");

        await _inventoryPage.AddItemToCartAsync("Sauce Labs Backpack");

        await _cartPage.GotoAsync();
        await _cartPage.ClickCheckoutAsync();

        await _checkoutPage.FillInformationAsync("First", "Last", "12345");
        await _checkoutPage.ClickContinueAsync();
        await _checkoutPage.ClickFinishAsync();

        // The site returns a title-cased message with punctuation
        Assert.That(await _checkoutPage.GetCompleteHeaderAsync(), Is.EqualTo("Thank you for your order!"));
    }
}
