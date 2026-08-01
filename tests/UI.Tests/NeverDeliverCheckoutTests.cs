using Allure.Net.Commons;
using Allure.NUnit;
using Allure.NUnit.Attributes;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using UI.Tests.PageObjects;

namespace UI.Tests;

[TestFixture]
[AllureNUnit]
[AllureFeature("Never Deliver - Checkout")]
public class NeverDeliverCheckoutTests : PageTest
{
    private NeverDeliverShopPage _shopPage = null!;
    private NeverDeliverBasketPage _basketPage = null!;
    private NeverDeliverCheckoutPage _checkoutPage = null!;

    [SetUp]
    public void Setup()
    {
        _shopPage = new NeverDeliverShopPage(Page);
        _basketPage = new NeverDeliverBasketPage(Page);
        _checkoutPage = new NeverDeliverCheckoutPage(Page);
    }

    [Test]
    [AllureSeverity(SeverityLevel.critical)]
    public async Task User_CanCompleteCheckoutWithSingleItem()
    {
        await _shopPage.GotoAsync();

        var products = await _shopPage.GetProductNamesAsync();
        await _shopPage.AddProductToBasketAsync(products[0]);

        await _shopPage.GoToBasketAsync();
        await _basketPage.ProceedToCheckoutAsync();

        await _checkoutPage.FillDeliveryDetailsAsync("John", "Doe", "john@example.com", "123 Main Street", "SW1A 1AA");
        await _checkoutPage.CompleteOrderAsync();

        var isSuccessful = await _checkoutPage.IsOrderSuccessfulAsync();
        Assert.That(isSuccessful, Is.True, "Order should be completed successfully");
    }

    [Test]
    [AllureSeverity(SeverityLevel.critical)]
    public async Task User_CanCompleteCheckoutWithMultipleItems()
    {
        await _shopPage.GotoAsync();

        var products = await _shopPage.GetProductNamesAsync();
        Assert.That(products.Count, Is.GreaterThanOrEqualTo(3), "At least 3 products should be available");

        await _shopPage.AddProductToBasketAsync(products[0]);
        await _shopPage.AddProductToBasketAsync(products[1]);
        await _shopPage.AddProductToBasketAsync(products[2]);

        await _shopPage.GoToBasketAsync();
        await _basketPage.ProceedToCheckoutAsync();

        await _checkoutPage.FillDeliveryDetailsAsync("Jane", "Smith", "jane@example.com", "456 Oak Avenue", "M1 1AD");
        await _checkoutPage.CompleteOrderAsync();

        var isSuccessful = await _checkoutPage.IsOrderSuccessfulAsync();
        Assert.That(isSuccessful, Is.True, "Order with multiple items should complete successfully");
    }

    [Test]
    [AllureSeverity(SeverityLevel.critical)]
    public async Task User_CanCompleteCheckoutWithDifferentPostcodes()
    {
        await _shopPage.GotoAsync();

        var products = await _shopPage.GetProductNamesAsync();
        await _shopPage.AddProductToBasketAsync(products[0]);

        await _shopPage.GoToBasketAsync();
        await _basketPage.ProceedToCheckoutAsync();

        await _checkoutPage.FillDeliveryDetailsAsync("Bob", "Johnson", "bob@example.com", "789 Elm Road", "B33 8TH");
        await _checkoutPage.CompleteOrderAsync();

        var isSuccessful = await _checkoutPage.IsOrderSuccessfulAsync();
        Assert.That(isSuccessful, Is.True, "Order should complete with valid UK postcode");
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task User_CanCompleteCheckoutWithSpecialCharactersInName()
    {
        await _shopPage.GotoAsync();

        var products = await _shopPage.GetProductNamesAsync();
        await _shopPage.AddProductToBasketAsync(products[0]);

        await _shopPage.GoToBasketAsync();
        await _basketPage.ProceedToCheckoutAsync();

        await _checkoutPage.FillDeliveryDetailsAsync("José", "O'Brien-García", "jose@example.com", "321 Héctor Plaza", "EC1A 1BB");
        await _checkoutPage.CompleteOrderAsync();

        var isSuccessful = await _checkoutPage.IsOrderSuccessfulAsync();
        Assert.That(isSuccessful, Is.True, "Order should accept special characters in names");
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task User_CanViewSuccessMessage()
    {
        await _shopPage.GotoAsync();

        var products = await _shopPage.GetProductNamesAsync();
        await _shopPage.AddProductToBasketAsync(products[0]);

        await _shopPage.GoToBasketAsync();
        await _basketPage.ProceedToCheckoutAsync();

        await _checkoutPage.FillDeliveryDetailsAsync("Test", "User", "test@example.com", "999 Test Drive", "N1 1AN");
        await _checkoutPage.CompleteOrderAsync();

        var successMessage = await _checkoutPage.GetSuccessMessageAsync();
        Assert.That(successMessage.Length, Is.GreaterThan(0), "Success message should be displayed");
        Assert.That(successMessage.ToLower(), Does.Contain("thank").Or.Contain("confirm").Or.Contain("success"), 
            "Success message should contain confirmation text");
    }
}
