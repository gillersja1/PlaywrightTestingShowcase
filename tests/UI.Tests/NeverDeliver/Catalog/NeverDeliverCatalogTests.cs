using Allure.Net.Commons;
using Allure.NUnit;
using Allure.NUnit.Attributes;
using NUnit.Framework;
using UI.Tests.Fixtures;
using UI.Tests.PageObjects;

namespace UI.Tests.NeverDeliver.Catalog;

[TestFixture]
[AllureNUnit]
[AllureFeature("Catalog")]
[AllureSuite("NeverDeliver")]
public class NeverDeliverCatalogTests : UITestFixtureBase
{
    private NeverDeliverShopPage _shopPage = null!;

    [SetUp]
    public async Task Setup()
    {
        BaseSetUp();
        _shopPage = new NeverDeliverShopPage(Page);
        await _shopPage.GotoAsync();
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task CatalogShowsResultCountAndProducts()
    {
        Assert.That(await _shopPage.GetResultCountAsync(), Is.GreaterThan(0), "Result summary should report products");
        Assert.That(await _shopPage.GetProductCardCountAsync(), Is.GreaterThan(0), "Product cards should be displayed");
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task ProductCardsShowNameAndPositivePrice()
    {
        var cardsToCheck = Math.Min(5, await _shopPage.GetProductCardCountAsync());
        var cards = new List<(string Name, decimal Price)>();
        for (var i = 0; i < cardsToCheck; i++)
        {
            cards.Add((await _shopPage.GetProductNameAsync(i), await _shopPage.GetProductPriceAsync(i)));
        }

        Assert.Multiple(() =>
        {
            for (var i = 0; i < cards.Count; i++)
            {
                Assert.That(cards[i].Name, Is.Not.Empty, $"Card {i} should have a name");
                Assert.That(cards[i].Price, Is.GreaterThan(0m), $"Card {i} should have a positive price");
            }
        });
    }

    [TestCase("beauty")]
    [TestCase("laptops")]
    [TestCase("groceries")]
    [AllureSeverity(SeverityLevel.critical)]
    public async Task CategoryFilter_NarrowsResults(string category)
    {
        var totalBefore = await _shopPage.GetResultCountAsync();

        await _shopPage.FilterByCategoryAsync(category);

        var totalAfter = await _shopPage.GetResultCountAsync();
        Assert.That(totalAfter, Is.GreaterThan(0), $"'{category}' should still have products");
        Assert.That(totalAfter, Is.LessThan(totalBefore), $"Filtering by '{category}' should narrow the catalog");
    }

    [Test]
    [AllureSeverity(SeverityLevel.critical)]
    public async Task AddingProducts_UpdatesCartCount()
    {
        var cart = new NeverDeliverCartDrawer(Page);
        await cart.WaitForItemCountAsync(0);

        await _shopPage.AddProductToCartAsync(0);
        await cart.WaitForItemCountAsync(1);

        await _shopPage.AddProductToCartAsync(1);
        await _shopPage.AddProductToCartAsync(2);
        await cart.WaitForItemCountAsync(3);
    }
}
