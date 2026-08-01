using Allure.Net.Commons;
using Allure.NUnit;
using Allure.NUnit.Attributes;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using UI.Tests.PageObjects;

namespace UI.Tests;

[TestFixture]
[AllureNUnit]
[AllureFeature("Never Deliver - Shopping")]
public class NeverDeliverShoppingTests : PageTest
{
    private NeverDeliverLoginPage _loginPage = null!;
    private NeverDeliverShopPage _shopPage = null!;
    private NeverDeliverBasketPage _basketPage = null!;

    [SetUp]
    public void Setup()
    {
        _loginPage = new NeverDeliverLoginPage(Page);
        _shopPage = new NeverDeliverShopPage(Page);
        _basketPage = new NeverDeliverBasketPage(Page);
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task User_CanViewProductCatalog()
    {
        await _shopPage.GotoAsync();

        var productCount = await _shopPage.GetProductCountAsync();
        Assert.That(productCount, Is.GreaterThan(0), "Product catalog should display at least one product");
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task User_CanViewProductNames()
    {
        await _shopPage.GotoAsync();

        var productNames = await _shopPage.GetProductNamesAsync();
        Assert.That(productNames.Count, Is.GreaterThan(0), "Should retrieve product names");
        Assert.That(productNames, Is.All.Not.Empty, "All product names should be non-empty");
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task User_CanViewProductPrices()
    {
        await _shopPage.GotoAsync();

        var prices = await _shopPage.GetProductPricesAsync();
        Assert.That(prices.Count, Is.GreaterThan(0), "Should retrieve product prices");
        Assert.That(prices, Is.All.GreaterThan(0), "All prices should be positive values");
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task User_CanAddProductToBasket()
    {
        await _shopPage.GotoAsync();

        var products = await _shopPage.GetProductNamesAsync();
        Assert.That(products.Count, Is.GreaterThan(0), "At least one product should be available");

        // Add the first product to basket
        await _shopPage.AddProductToBasketAsync(products[0]);

        var basketCount = await _shopPage.GetBasketCountAsync();
        Assert.That(basketCount, Is.EqualTo(1), "Basket should contain 1 item");
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task User_CanAddMultipleProductsToBasket()
    {
        await _shopPage.GotoAsync();

        var products = await _shopPage.GetProductNamesAsync();
        Assert.That(products.Count, Is.GreaterThanOrEqualTo(3), "At least 3 products should be available");

        // Add first 3 products
        await _shopPage.AddProductToBasketAsync(products[0]);
        await _shopPage.AddProductToBasketAsync(products[1]);
        await _shopPage.AddProductToBasketAsync(products[2]);

        var basketCount = await _shopPage.GetBasketCountAsync();
        Assert.That(basketCount, Is.EqualTo(3), "Basket should contain 3 items");
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task User_CanAddSameProductMultipleTimes()
    {
        await _shopPage.GotoAsync();

        var products = await _shopPage.GetProductNamesAsync();
        Assert.That(products.Count, Is.GreaterThan(0), "At least one product should be available");

        // Add same product twice
        await _shopPage.AddProductToBasketAsync(products[0]);
        await _shopPage.AddProductToBasketAsync(products[0]);

        var basketCount = await _shopPage.GetBasketCountAsync();
        Assert.That(basketCount, Is.EqualTo(2), "Basket should contain 2 instances of same product");
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task User_CanViewBasketFromShop()
    {
        await _shopPage.GotoAsync();

        var products = await _shopPage.GetProductNamesAsync();
        await _shopPage.AddProductToBasketAsync(products[0]);

        // Navigate to basket
        await _shopPage.GoToBasketAsync();

        var basketItemCount = await _basketPage.GetBasketItemCountAsync();
        Assert.That(basketItemCount, Is.EqualTo(1), "Basket should display the added product");
    }
}
