using System.Text.RegularExpressions;
using Microsoft.Playwright;
using NUnit.Framework;
using UI.Tests.TestData;
using static Microsoft.Playwright.Assertions;

namespace UI.Tests.PageObjects;

/// <summary>
/// Page object for the never-deliver-shop.vercel.app product catalog.
/// The site has no data-testid attributes, so locators use ARIA roles and
/// visible text. Category names appear in both the header nav and the
/// filter pill row, so catalog locators are scoped to #main-content.
/// </summary>
public class NeverDeliverShopPage
{
    private readonly IPage _page;

    private ILocator Main => _page.Locator("#main-content");
    private ILocator ResultSummary => _page.GetByText(new Regex(@"\d+ results · Page \d+ of \d+"));
    private ILocator ProductCards => Main.GetByRole(AriaRole.Link)
        .Filter(new LocatorFilterOptions { Has = _page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Add to Cart" }) });
    private ILocator CartButton => _page.GetByRole(AriaRole.Banner)
        .GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { NameRegex = new Regex("Cart") });

    public NeverDeliverShopPage(IPage page)
    {
        _page = page;
    }

    public async Task GotoAsync()
    {
        await _page.GotoAsync(NeverDeliverTestData.BaseUrl + "/");
        // The catalog is fetched client-side from third-party APIs (dummyjson.com,
        // fakestoreapi.com). Under CI's parallel load a fetch occasionally stalls
        // and the catalog never renders, so reload once before failing.
        if (!await CatalogLoadsAsync())
        {
            TestContext.Progress.WriteLine("NeverDeliver catalog did not load within 15s; reloading once");
            await _page.ReloadAsync();
            await Expect(ResultSummary).ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions { Timeout = 15000 });
        }
    }

    private async Task<bool> CatalogLoadsAsync()
    {
        try
        {
            await ResultSummary.WaitForAsync(new LocatorWaitForOptions { Timeout = 15000 });
            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
    }

    public async Task<int> GetResultCountAsync()
    {
        var summary = await ResultSummary.InnerTextAsync();
        return int.Parse(Regex.Match(summary, @"^\d+").Value);
    }

    public async Task FilterByCategoryAsync(string category)
    {
        var before = await GetResultCountAsync();
        await Main.GetByRole(AriaRole.Link, new LocatorGetByRoleOptions { Name = category, Exact = true }).ClickAsync();
        await _page.WaitForURLAsync(new Regex($"[?&]category={Regex.Escape(category)}"));
        // The URL updates before the list re-renders; wait for the count to change.
        await Expect(ResultSummary).Not.ToHaveTextAsync(new Regex($"^{before} results"));
    }

    public async Task<int> GetProductCardCountAsync() => await ProductCards.CountAsync();

    public async Task<string> GetProductNameAsync(int index) =>
        (await ProductCards.Nth(index).Locator("h2, h3").InnerTextAsync()).Trim();

    public async Task<decimal> GetProductPriceAsync(int index) =>
        NeverDeliverTestData.ParsePounds(await ProductCards.Nth(index).InnerTextAsync());

    /// <summary>
    /// Index of the first product card showing the given text, e.g. a stock warning.
    /// </summary>
    public async Task<int> FindProductIndexAsync(string cardText)
    {
        var count = await ProductCards.CountAsync();
        for (var i = 0; i < count; i++)
        {
            if ((await ProductCards.Nth(i).InnerTextAsync()).Contains(cardText))
            {
                return i;
            }
        }
        throw new InvalidOperationException($"No product card contains \"{cardText}\"");
    }

    /// <summary>
    /// Indexes of the first <paramref name="count"/> product cards without a
    /// low-stock warning ("Only N left in stock!"). The live catalog's stock
    /// changes, so tests that raise quantities can't rely on fixed positions.
    /// </summary>
    public async Task<IReadOnlyList<int>> FindWellStockedProductIndexesAsync(int count)
    {
        var indexes = new List<int>();
        var total = await ProductCards.CountAsync();
        for (var i = 0; i < total && indexes.Count < count; i++)
        {
            if (!(await ProductCards.Nth(i).InnerTextAsync()).Contains("left in stock"))
            {
                indexes.Add(i);
            }
        }
        if (indexes.Count < count)
        {
            throw new InvalidOperationException($"Only {indexes.Count} well-stocked products on the page, needed {count}");
        }
        return indexes;
    }

    public async Task AddProductToCartAsync(int index)
    {
        await ProductCards.Nth(index)
            .GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Add to Cart" })
            .ClickAsync();
    }

    public async Task<NeverDeliverCartDrawer> OpenCartAsync()
    {
        await CartButton.ClickAsync();
        var drawer = new NeverDeliverCartDrawer(_page);
        await drawer.WaitUntilOpenAsync();
        return drawer;
    }
}
