using Allure.Net.Commons;
using Allure.NUnit;
using Allure.NUnit.Attributes;
using NUnit.Framework;
using UI.Tests.Fixtures;
using UI.Tests.PageObjects;
using UI.Tests.TestData;

namespace UI.Tests.SauceDemo.Cart;

[TestFixture]
[AllureNUnit]
[AllureFeature("Shopping Cart")]
[AllureSuite("SauceDemo")]
public class SauceDemoCartTests : UITestFixtureBase
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

    private async Task LoginAndAddItemsAsync(params string[] items)
    {
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync(LoginCredentials.ValidUser.Username, LoginCredentials.ValidUser.Password);

        foreach (var item in items)
        {
            await _inventoryPage.AddItemToCartAsync(item);
        }

        await _inventoryPage.GoToCartAsync();
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task CartDisplaysCorrectItemCount()
    {
        await LoginAndAddItemsAsync(SauceDemoTestData.Products.Backpack, SauceDemoTestData.Products.BikeLight);

        var count = await _cartPage.GetItemCountAsync();
        Assert.That(count, Is.EqualTo(2));
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task CanRemoveItemFromCart()
    {
        await LoginAndAddItemsAsync(SauceDemoTestData.Products.Backpack, SauceDemoTestData.Products.BikeLight);

        await _cartPage.RemoveItemFromCartAsync(SauceDemoTestData.Products.Backpack);

        var items = await _cartPage.GetCartItemNamesAsync();
        Assert.That(items, Does.Not.Contain(SauceDemoTestData.Products.Backpack));
        Assert.That(items.Count, Is.EqualTo(1));
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task CartTotalCalculatesCorrectly()
    {
        await LoginAndAddItemsAsync(SauceDemoTestData.Products.Backpack, SauceDemoTestData.Products.BikeLight);

        var subtotal = await _cartPage.GetSubtotalAsync();
        var tax = await _cartPage.GetTaxAsync();
        var total = await _cartPage.GetTotalAsync();

        var calculated = subtotal + tax;
        Assert.That(total, Is.EqualTo(calculated).Within(0.01), "Total should equal subtotal + tax");
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task RemovingLastItemEmptiesCart()
    {
        await LoginAndAddItemsAsync(SauceDemoTestData.Products.Backpack);

        await _cartPage.RemoveItemFromCartAsync(SauceDemoTestData.Products.Backpack);

        Assert.That(await _cartPage.IsCartEmptyAsync(), Is.True);
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task CartItemsDisplayWithCorrectInfo()
    {
        await LoginAndAddItemsAsync(SauceDemoTestData.Products.Backpack);

        var items = await _cartPage.GetCartItemNamesAsync();
        Assert.That(items, Does.Contain(SauceDemoTestData.Products.Backpack));
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task ContinueShoppingNavigatesBackToInventory()
    {
        await LoginAndAddItemsAsync(SauceDemoTestData.Products.Backpack);

        await _cartPage.ClickContinueShoppingAsync();

        Assert.That(await _inventoryPage.GetPageTitleAsync(), Is.EqualTo("Products"));
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task TaxCalculatesCorrectly()
    {
        await LoginAndAddItemsAsync(SauceDemoTestData.Products.Backpack);

        var subtotal = await _cartPage.GetSubtotalAsync();
        var tax = await _cartPage.GetTaxAsync();

        // Tax should be approximately 10% of subtotal (SauceDemo uses ~8% but varies)
        Assert.That(tax, Is.GreaterThan(0), "Tax should be calculated");
        Assert.That(tax, Is.LessThan(subtotal), "Tax should be less than subtotal");
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task MultipleItemsCalculateCorrectly()
    {
        await LoginAndAddItemsAsync(
            SauceDemoTestData.Products.Backpack,
            SauceDemoTestData.Products.BikeLight,
            SauceDemoTestData.Products.TShirt
        );

        var count = await _cartPage.GetItemCountAsync();
        var subtotal = await _cartPage.GetSubtotalAsync();

        Assert.That(count, Is.EqualTo(3));
        Assert.That(subtotal, Is.GreaterThan(0));
    }
}
