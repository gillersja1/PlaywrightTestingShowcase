using Allure.Net.Commons;
using Allure.NUnit;
using Allure.NUnit.Attributes;
using NUnit.Framework;
using UI.Tests.Fixtures;
using UI.Tests.PageObjects;
using UI.Tests.TestData;

namespace UI.Tests.SauceDemo.Shopping;

[TestFixture]
[AllureNUnit]
[AllureFeature("Shopping")]
[AllureSuite("SauceDemo")]
public class SauceDemoShoppingTests : UITestFixtureBase
{
    private LoginPage _loginPage = null!;
    private InventoryPage _inventoryPage = null!;
    private CartPage _cartPage = null!;

    [SetUp]
    public void Setup()
    {
        BaseSetUp();
        _loginPage = new LoginPage(Page);
        _inventoryPage = new InventoryPage(Page);
        _cartPage = new CartPage(Page);
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task ProductsDisplayCorrectly()
    {
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync(LoginCredentials.ValidUser.Username, LoginCredentials.ValidUser.Password);

        var count = await _inventoryPage.GetProductCountAsync();
        Assert.That(count, Is.GreaterThan(0), "Should display products");

        var displayed = await _inventoryPage.AreAllProductsDisplayedAsync();
        Assert.That(displayed, Is.True, "All products should be displayed with images");
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task ProductsHaveValidPrices()
    {
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync(LoginCredentials.ValidUser.Username, LoginCredentials.ValidUser.Password);

        var prices = await _inventoryPage.GetAllProductPricesAsync();
        Assert.That(prices.Count, Is.GreaterThan(0));
        Assert.That(prices, Is.All.GreaterThan(0), "All prices should be positive");
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task CanAddItemToCart()
    {
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync(LoginCredentials.ValidUser.Username, LoginCredentials.ValidUser.Password);

        await _inventoryPage.AddItemToCartAsync(SauceDemoTestData.Products.Backpack);
        var count = await _inventoryPage.GetCartCountAsync();

        Assert.That(count, Is.EqualTo(1));
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task CanAddMultipleItemsToCart()
    {
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync(LoginCredentials.ValidUser.Username, LoginCredentials.ValidUser.Password);

        await _inventoryPage.AddItemToCartAsync(SauceDemoTestData.Products.Backpack);
        await _inventoryPage.AddItemToCartAsync(SauceDemoTestData.Products.BikeLight);
        await _inventoryPage.AddItemToCartAsync(SauceDemoTestData.Products.TShirt);

        var count = await _inventoryPage.GetCartCountAsync();
        Assert.That(count, Is.EqualTo(3));
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task CartBadgeUpdatesWhenAddingItems()
    {
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync(LoginCredentials.ValidUser.Username, LoginCredentials.ValidUser.Password);

        var countBefore = await _inventoryPage.GetCartCountAsync();
        Assert.That(countBefore, Is.EqualTo(0));

        await _inventoryPage.AddItemToCartAsync(SauceDemoTestData.Products.Backpack);

        var countAfter = await _inventoryPage.GetCartCountAsync();
        Assert.That(countAfter, Is.EqualTo(1));
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task CartPersistsAcrossPageNavigation()
    {
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync(LoginCredentials.ValidUser.Username, LoginCredentials.ValidUser.Password);

        await _inventoryPage.AddItemToCartAsync(SauceDemoTestData.Products.Backpack);
        var countBefore = await _inventoryPage.GetCartCountAsync();

        // Navigate to cart and back
        await _inventoryPage.GoToCartAsync();
        await _cartPage.ClickContinueShoppingAsync();

        var countAfter = await _inventoryPage.GetCartCountAsync();
        Assert.That(countAfter, Is.EqualTo(countBefore), "Cart should persist");
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task CanRemoveItemFromCart()
    {
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync(LoginCredentials.ValidUser.Username, LoginCredentials.ValidUser.Password);

        await _inventoryPage.AddItemToCartAsync(SauceDemoTestData.Products.Backpack);
        await _inventoryPage.AddItemToCartAsync(SauceDemoTestData.Products.BikeLight);

        await _inventoryPage.RemoveItemFromCartAsync(SauceDemoTestData.Products.Backpack);

        var count = await _inventoryPage.GetCartCountAsync();
        Assert.That(count, Is.EqualTo(1));
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task ProblemUserCanStillShop()
    {
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync(LoginCredentials.ProblemUser.Username, LoginCredentials.ProblemUser.Password);

        await _inventoryPage.AddItemToCartAsync(SauceDemoTestData.Products.Backpack);
        var count = await _inventoryPage.GetCartCountAsync();

        Assert.That(count, Is.EqualTo(1), "Problem user should be able to add items");
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task ProductNamesAreRetrievable()
    {
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync(LoginCredentials.ValidUser.Username, LoginCredentials.ValidUser.Password);

        var names = await _inventoryPage.GetAllProductNamesAsync();
        Assert.That(names.Count, Is.GreaterThan(0));
        Assert.That(names, Is.All.Not.Empty, "All products should have names");
    }
}
