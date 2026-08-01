using Microsoft.Playwright;

namespace UI.Tests.PageObjects;

/// <summary>
/// Page object for the Never Deliver website product catalog/shop page.
/// </summary>
public class NeverDeliverShopPage
{
    private readonly IPage _page;

    private ILocator ProductCards => _page.Locator("[class*='product']");
    private ILocator AddToBasketButtons => _page.Locator("button:has-text('Add to basket')");
    private ILocator BasketBadge => _page.Locator("[class*='basket'] [class*='badge']");
    private ILocator BasketIcon => _page.Locator("a[href*='basket'], button[class*='basket']");
    private ILocator PageTitle => _page.Locator("h1, h2");

    public NeverDeliverShopPage(IPage page)
    {
        _page = page;
    }

    public async Task GotoAsync()
    {
        await _page.GotoAsync("https://neverdeliver.co.uk/shop");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task<int> GetProductCountAsync()
    {
        return await ProductCards.CountAsync();
    }

    public async Task AddProductToBasketAsync(string productName)
    {
        var product = ProductCards.Filter(new LocatorFilterOptions { HasText = productName });
        await product.Locator("button:has-text('Add to basket')").ClickAsync();
    }

    public async Task<int> GetBasketCountAsync()
    {
        if (await BasketBadge.IsVisibleAsync())
        {
            var text = await BasketBadge.InnerTextAsync();
            return int.Parse(text);
        }
        return 0;
    }

    public async Task GoToBasketAsync()
    {
        await BasketIcon.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task<List<string>> GetProductNamesAsync()
    {
        var names = new List<string>();
        var count = await ProductCards.CountAsync();

        for (int i = 0; i < count; i++)
        {
            var name = await ProductCards.Nth(i).Locator("[class*='title'], h3").InnerTextAsync();
            names.Add(name);
        }

        return names;
    }

    public async Task<List<decimal>> GetProductPricesAsync()
    {
        var prices = new List<decimal>();
        var priceLocators = _page.Locator("[class*='price']");
        var count = await priceLocators.CountAsync();

        for (int i = 0; i < count; i++)
        {
            var priceText = await priceLocators.Nth(i).InnerTextAsync();
            // Try to parse price (handle currency symbols)
            var cleanPrice = System.Text.RegularExpressions.Regex.Replace(priceText, @"[^\d.]", "");
            if (decimal.TryParse(cleanPrice, out var price))
            {
                prices.Add(price);
            }
        }

        return prices;
    }
}
