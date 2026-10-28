using Allure.Net.Commons;
using Allure.NUnit;
using Allure.NUnit.Attributes;
using NUnit.Framework;
using UI.Tests.Fixtures;
using UI.Tests.PageObjects;
using UI.Tests.TestData;

namespace UI.Tests.NeverDeliver.Orders;

[TestFixture]
[AllureNUnit]
[AllureFeature("Order History")]
[AllureSuite("NeverDeliver")]
public class NeverDeliverOrderHistoryTests : UITestFixtureBase
{
    private NeverDeliverOrdersPage _ordersPage = null!;

    [SetUp]
    public void Setup()
    {
        BaseSetUp();
        _ordersPage = new NeverDeliverOrdersPage(Page);
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task NewVisitor_SeesEmptyOrderHistory()
    {
        await _ordersPage.GotoAsync();

        Assert.That(await _ordersPage.IsEmptyStateVisibleAsync(), Is.True);
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task StartShopping_ReturnsToCatalog()
    {
        await _ordersPage.GotoAsync();

        await _ordersPage.StartShoppingAsync();

        await Expect(Page).ToHaveURLAsync(NeverDeliverTestData.BaseUrl + "/");
    }

    [Test]
    [AllureSeverity(SeverityLevel.critical)]
    public async Task PlacedOrder_AppearsInOrderHistory()
    {
        var shopPage = new NeverDeliverShopPage(Page);
        await shopPage.GotoAsync();
        var productName = await shopPage.GetProductNameAsync(1);
        await shopPage.AddProductToCartAsync(1);
        var cart = await shopPage.OpenCartAsync();
        var checkout = await cart.ProceedToCheckoutAsync();
        await checkout.FillAddressAsync(DeliveryAddress.Valid);
        await checkout.ContinueToPaymentAsync();
        await checkout.PlaceOrderAsync();
        var orderNumber = await checkout.GetOrderNumberAsync();

        await _ordersPage.GotoAsync();

        var history = await _ordersPage.GetPageTextAsync();
        Assert.Multiple(() =>
        {
            Assert.That(history, Does.Contain(orderNumber), "Order number should appear in history");
            Assert.That(history, Does.Contain(productName), "Ordered product should appear in history");
            Assert.That(history, Does.Contain(DeliveryAddress.Valid.FullName), "Recipient should appear in history");
        });
    }
}
