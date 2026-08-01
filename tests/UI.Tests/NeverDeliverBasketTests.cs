using Allure.Net.Commons;
using Allure.NUnit;
using Allure.NUnit.Attributes;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using UI.Tests.PageObjects;

namespace UI.Tests;

[TestFixture]
[AllureNUnit]
[AllureFeature("Never Deliver - Basket Management")]
public class NeverDeliverBasketTests : PageTest
{
    private NeverDeliverShopPage _shopPage = null!;
    private NeverDeliverBasketPage _basketPage = null!;

    [SetUp]
    public void Setup()
    {
        _shopPage = new NeverDeliverShopPage(Page);
        _basketPage = new NeverDeliverBasketPage(Page);
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task User_CanRemoveItemFromBasket()
    {
        await _shopPage.GotoAsync();

        var products = await _shopPage.GetProductNamesAsync();
        Assert.That(products.Count, Is.GreaterThanOrEqualTo(2), "At least 2 products should be available");

        // Add 2 products
        await _shopPage.AddProductToBasketAsync(products[0]);
        await _shopPage.AddProductToBasketAsync(products[1]);

        // Navigate to basket
        await _shopPage.GoToBasketAsync();

        var initialCount = await _basketPage.GetBasketItemCountAsync();
        Assert.That(initialCount, Is.EqualTo(2), "Basket should have 2 items");

        // Remove first product
        await _basketPage.RemoveItemAsync(products[0]);

        var finalCount = await _basketPage.GetBasketItemCountAsync();
        Assert.That(finalCount, Is.EqualTo(1), "Basket should have 1 item after removal");
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task User_CanRemoveAllItemsFromBasket()
    {
        await _shopPage.GotoAsync();

        var products = await _shopPage.GetProductNamesAsync();
        await _shopPage.AddProductToBasketAsync(products[0]);

        await _shopPage.GoToBasketAsync();
        await _basketPage.RemoveItemAsync(products[0]);

        var isEmpty = await _basketPage.IsBasketEmptyAsync();
        Assert.That(isEmpty, Is.True, "Basket should be empty after removing all items");
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task User_CanViewBasketItemNames()
    {
        await _shopPage.GotoAsync();

        var products = await _shopPage.GetProductNamesAsync();
        Assert.That(products.Count, Is.GreaterThanOrEqualTo(2), "At least 2 products should be available");

        await _shopPage.AddProductToBasketAsync(products[0]);
        await _shopPage.AddProductToBasketAsync(products[1]);

        await _shopPage.GoToBasketAsync();

        var basketItems = await _basketPage.GetBasketItemNamesAsync();
        Assert.That(basketItems.Count, Is.EqualTo(2), "Basket should display 2 items");
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task User_CanViewBasketTotal()
    {
        await _shopPage.GotoAsync();

        var products = await _shopPage.GetProductNamesAsync();
        await _shopPage.AddProductToBasketAsync(products[0]);

        await _shopPage.GoToBasketAsync();

        var total = await _basketPage.GetTotalPriceAsync();
        Assert.That(total.Length, Is.GreaterThan(0), "Total price should be displayed");
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task User_CanProceedToCheckout()
    {
        await _shopPage.GotoAsync();

        var products = await _shopPage.GetProductNamesAsync();
        await _shopPage.AddProductToBasketAsync(products[0]);

        await _shopPage.GoToBasketAsync();

        var initialUrl = Page.Url;
        await _basketPage.ProceedToCheckoutAsync();

        var finalUrl = Page.Url;
        Assert.That(finalUrl, Is.Not.EqualTo(initialUrl), "Should navigate to checkout page");
    }
}
