using Allure.Net.Commons;
using Allure.NUnit;
using Allure.NUnit.Attributes;
using NUnit.Framework;
using UI.Tests.Fixtures;
using UI.Tests.PageObjects;
using UI.Tests.TestData;

namespace UI.Tests.SauceDemo.Checkout;

[TestFixture]
[AllureNUnit]
[AllureFeature("Checkout")]
[AllureSuite("SauceDemo")]
public class SauceDemoCheckoutValidationTests : UITestFixtureBase
{
    private LoginPage _loginPage = null!;
    private InventoryPage _inventoryPage = null!;
    private CartPage _cartPage = null!;
    private CheckoutPage _checkoutPage = null!;

    [SetUp]
    public void Setup()
    {
        BaseSetUp();
        _loginPage = new LoginPage(Page);
        _inventoryPage = new InventoryPage(Page);
        _cartPage = new CartPage(Page);
        _checkoutPage = new CheckoutPage(Page);
    }

    private async Task GoToCheckoutAsync()
    {
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync(LoginCredentials.ValidUser.Username, LoginCredentials.ValidUser.Password);
        await _inventoryPage.AddItemToCartAsync(SauceDemoTestData.Products.Backpack);
        await _inventoryPage.GoToCartAsync();
        await _cartPage.ClickCheckoutAsync();
    }

    [Test]
    [AllureSeverity(SeverityLevel.critical)]
    public async Task ValidCheckoutCompletesSuccessfully()
    {
        await GoToCheckoutAsync();
        await _checkoutPage.FillInformationAsync(CheckoutInfo.Valid.FirstName, CheckoutInfo.Valid.LastName, CheckoutInfo.Valid.PostalCode);
        await _checkoutPage.ClickContinueAsync();
        await _checkoutPage.ClickFinishAsync();

        Assert.That(await _checkoutPage.GetCompleteHeaderAsync(), Is.EqualTo("Thank you for your order!"));
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    [TestCaseSource(nameof(GetInvalidCheckoutInfo))]
    public async Task CheckoutWithMissingFields_ShowsError(CheckoutInfo info)
    {
        await GoToCheckoutAsync();
        await _checkoutPage.FillInformationAsync(info.FirstName, info.LastName, info.PostalCode);
        await _checkoutPage.ClickContinueAsync();

        Assert.That(await _checkoutPage.IsErrorMessageVisibleAsync(), Is.True);
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task CheckoutWithEmptyFirstName_ShowsError()
    {
        await GoToCheckoutAsync();
        await _checkoutPage.FillInformationAsync("", "Doe", "12345");
        await _checkoutPage.ClickContinueAsync();

        Assert.That(await _checkoutPage.IsErrorMessageVisibleAsync(), Is.True);
        Assert.That(await _checkoutPage.GetErrorMessageAsync(), Does.Contain("First Name").IgnoreCase);
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task CheckoutWithEmptyLastName_ShowsError()
    {
        await GoToCheckoutAsync();
        await _checkoutPage.FillInformationAsync("John", "", "12345");
        await _checkoutPage.ClickContinueAsync();

        Assert.That(await _checkoutPage.IsErrorMessageVisibleAsync(), Is.True);
        Assert.That(await _checkoutPage.GetErrorMessageAsync(), Does.Contain("Last Name").IgnoreCase);
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task CheckoutWithEmptyPostalCode_ShowsError()
    {
        await GoToCheckoutAsync();
        await _checkoutPage.FillInformationAsync("John", "Doe", "");
        await _checkoutPage.ClickContinueAsync();

        Assert.That(await _checkoutPage.IsErrorMessageVisibleAsync(), Is.True);
        Assert.That(await _checkoutPage.GetErrorMessageAsync(), Does.Contain("Postal Code").IgnoreCase);
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task CheckoutWithSpecialCharactersInName()
    {
        await GoToCheckoutAsync();
        await _checkoutPage.FillInformationAsync(
            CheckoutInfo.WithSpecialChars.FirstName,
            CheckoutInfo.WithSpecialChars.LastName,
            CheckoutInfo.WithSpecialChars.PostalCode
        );
        await _checkoutPage.ClickContinueAsync();

        // Should either complete or show validation error
        var isError = await _checkoutPage.IsErrorMessageVisibleAsync();
        Assert.That(isError || Page.Url.Contains("checkout-step-two"));
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task CheckoutWithVeryLongNames()
    {
        await GoToCheckoutAsync();
        var longName = new string('a', 100);
        await _checkoutPage.FillInformationAsync(longName, longName, "12345");
        await _checkoutPage.ClickContinueAsync();

        // Should handle gracefully
        Assert.That(await _checkoutPage.IsErrorMessageVisibleAsync() || Page.Url.Contains("checkout-step-two"));
    }

    [Test]
    [AllureSeverity(SeverityLevel.critical)]
    public async Task CheckoutDisplaysCorrectCartItems()
    {
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync(LoginCredentials.ValidUser.Username, LoginCredentials.ValidUser.Password);

        await _inventoryPage.AddItemToCartAsync(SauceDemoTestData.Products.Backpack);
        await _inventoryPage.AddItemToCartAsync(SauceDemoTestData.Products.BikeLight);

        await _inventoryPage.GoToCartAsync();
        await _cartPage.ClickCheckoutAsync();

        var cartItemCount = await _checkoutPage.GetCartItemCountAsync();
        Assert.That(cartItemCount, Is.EqualTo(2));
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task CheckoutAfterRemovingItemFromCart()
    {
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync(LoginCredentials.ValidUser.Username, LoginCredentials.ValidUser.Password);

        await _inventoryPage.AddItemToCartAsync(SauceDemoTestData.Products.Backpack);
        await _inventoryPage.AddItemToCartAsync(SauceDemoTestData.Products.BikeLight);

        await _inventoryPage.GoToCartAsync();
        await _cartPage.RemoveItemFromCartAsync(SauceDemoTestData.Products.Backpack);
        await _cartPage.ClickCheckoutAsync();

        var cartItemCount = await _checkoutPage.GetCartItemCountAsync();
        Assert.That(cartItemCount, Is.EqualTo(1));
    }

    private static IEnumerable<CheckoutInfo> GetInvalidCheckoutInfo()
    {
        return CheckoutInfo.InvalidInputs;
    }
}
