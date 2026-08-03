using Allure.Net.Commons;
using Allure.NUnit;
using Allure.NUnit.Attributes;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using UI.Tests.PageObjects;

namespace UI.Tests;

[TestFixture]
[AllureNUnit]
[AllureFeature("Products")]
public class SauceDemoProductTests : PageTest
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
    [AllureSeverity(SeverityLevel.normal)]
    public async Task StandardUser_CanViewProductList()
    {
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync("standard_user", "secret_sauce");

        var itemCount = await _inventoryPage.GetProductCountAsync();
        Assert.That(itemCount, Is.GreaterThan(0), "Product list should contain items");
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task StandardUser_CanAddMultipleItemsToCart()
    {
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync("standard_user", "secret_sauce");

        await _inventoryPage.AddItemToCartAsync("Sauce Labs Backpack");
        await _inventoryPage.AddItemToCartAsync("Sauce Labs Bike Light");
        await _inventoryPage.AddItemToCartAsync("Sauce Labs Bolt T-Shirt");

        var cartCount = await _inventoryPage.GetCartCountAsync();
        Assert.That(cartCount, Is.EqualTo(3), "Cart should contain 3 items");
    }

    [Test]
    [AllureSeverity(SeverityLevel.minor)]
    public async Task StandardUser_CanViewProductByPrice()
    {
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync("standard_user", "secret_sauce");

        var productPrices = await _inventoryPage.GetAllProductPricesAsync();
        Assert.That(productPrices.Count, Is.GreaterThan(0), "Should retrieve product prices");
        Assert.That(productPrices, Is.All.GreaterThan(0), "All prices should be positive");
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task StandardUser_CanAddSameItemTwice()
    {
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync("standard_user", "secret_sauce");

        await _inventoryPage.AddItemToCartAsync("Sauce Labs Backpack");
        await _inventoryPage.AddItemToCartAsync("Sauce Labs Bike Light");

        var cartCount = await _inventoryPage.GetCartCountAsync();
        Assert.That(cartCount, Is.EqualTo(2), "Same item should be addable multiple times");
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task ProblemUser_CanStillAddItemsToCart()
    {
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync("problem_user", "secret_sauce");

        await _inventoryPage.AddItemToCartAsync("Sauce Labs Backpack");

        var cartCount = await _inventoryPage.GetCartCountAsync();
        Assert.That(cartCount, Is.EqualTo(1), "Problem user should still be able to add items");
    }
}
