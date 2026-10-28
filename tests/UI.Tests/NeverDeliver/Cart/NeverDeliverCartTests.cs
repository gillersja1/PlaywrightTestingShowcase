using Allure.Net.Commons;
using Allure.NUnit;
using Allure.NUnit.Attributes;
using NUnit.Framework;
using UI.Tests.Fixtures;
using UI.Tests.PageObjects;

namespace UI.Tests.NeverDeliver.Cart;

[TestFixture]
[AllureNUnit]
[AllureFeature("Cart")]
[AllureSuite("NeverDeliver")]
public class NeverDeliverCartTests : UITestFixtureBase
{
    private NeverDeliverShopPage _shopPage = null!;

    [SetUp]
    public async Task Setup()
    {
        BaseSetUp();
        _shopPage = new NeverDeliverShopPage(Page);
        await _shopPage.GotoAsync();
    }

    /// <summary>
    /// Adds the first <paramref name="count"/> products that have plenty of
    /// stock, so quantity tests aren't capped by a "last one" item.
    /// </summary>
    private async Task<NeverDeliverCartDrawer> AddProductsAndOpenCartAsync(int count)
    {
        for (var i = 0; i < count; i++)
        {
            await _shopPage.AddProductToCartAsync(i + 1);
        }
        var cart = await _shopPage.OpenCartAsync();
        await cart.WaitForItemCountAsync(count);
        return cart;
    }

    [Test]
    [AllureSeverity(SeverityLevel.critical)]
    public async Task RemovingItem_UpdatesCartCount()
    {
        var cart = await AddProductsAndOpenCartAsync(2);

        await cart.RemoveLineAsync(0);

        await cart.WaitForItemCountAsync(1);
        Assert.That(await cart.GetLineCountAsync(), Is.EqualTo(1));
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task RemovingAllItems_ShowsEmptyCart()
    {
        var cart = await AddProductsAndOpenCartAsync(2);

        await cart.RemoveLineAsync(0);
        await cart.RemoveLineAsync(0);

        await cart.WaitForItemCountAsync(0);
        Assert.That(await cart.IsEmptyMessageVisibleAsync(), Is.True, "Empty cart message should be shown");
    }

    [Test]
    [AllureSeverity(SeverityLevel.critical)]
    public async Task SubtotalEqualsSumOfLinePrices()
    {
        var cart = await AddProductsAndOpenCartAsync(3);

        var expected = 0m;
        for (var i = 0; i < 3; i++)
        {
            expected += await cart.GetLineUnitPriceAsync(i);
        }

        Assert.That(await cart.GetSubtotalAsync(), Is.EqualTo(expected));
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task IncreasingQuantity_AddsOneUnitPriceToSubtotal()
    {
        var cart = await AddProductsAndOpenCartAsync(1);
        var unitPrice = await cart.GetLineUnitPriceAsync(0);
        var subtotalBefore = await cart.GetSubtotalAsync();

        await cart.IncreaseQuantityAsync(0);

        await cart.WaitForItemCountAsync(2);
        Assert.That(await cart.GetSubtotalAsync(), Is.EqualTo(subtotalBefore + unitPrice));
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    [Description("Boundary: a product with one unit left can't be increased past its stock.")]
    public async Task LastItemInStock_CannotIncreaseQuantity()
    {
        var index = await _shopPage.FindProductIndexAsync("Only 1 left in stock!");
        await _shopPage.AddProductToCartAsync(index);
        var cart = await _shopPage.OpenCartAsync();

        await cart.WaitForItemCountAsync(1);
        Assert.That(await cart.IncreaseQuantityButton(0).IsDisabledAsync(), Is.True, "+ should be disabled at max stock");
        Assert.That(await cart.GetLineTextAsync(0), Does.Contain("Max stock reached (1)"));
    }

    [Test]
    [AllureSeverity(SeverityLevel.critical)]
    public async Task ProceedToCheckout_OpensDeliveryStep()
    {
        var cart = await AddProductsAndOpenCartAsync(1);

        await cart.ProceedToCheckoutAsync();

        Assert.That(Page.Url, Does.EndWith("/checkout"));
    }
}
