using Microsoft.Playwright;

namespace UI.Tests.PageObjects;

/// <summary>
/// Page object for the saucedemo.com inventory (product listing) page.
/// </summary>
public class InventoryPage
{
    private readonly IPage _page;

    private ILocator PageTitle => _page.Locator(".title");
    private ILocator CartBadge => _page.Locator(".shopping_cart_badge");
    private ILocator InventoryItems => _page.Locator(".inventory_item");

    public InventoryPage(IPage page)
    {
        _page = page;
    }

    public async Task<string> GetPageTitleAsync() => await PageTitle.InnerTextAsync();

    public async Task AddItemToCartAsync(string itemName)
    {
        var item = InventoryItems.Filter(new LocatorFilterOptions { HasText = itemName });
        await item.Locator("button").Filter(new LocatorFilterOptions { HasText = "Add to cart" }).ClickAsync();
    }

    public async Task<int> GetCartCountAsync()
    {
        if (!await CartBadge.IsVisibleAsync())
        {
            return 0;
        }

        var text = await CartBadge.InnerTextAsync();
        return int.Parse(text);
    }

    public async Task<int> GetProductCountAsync()
    {
        return await InventoryItems.CountAsync();
    }

    public async Task<List<decimal>> GetAllProductPricesAsync()
    {
        var prices = new List<decimal>();
        var priceElements = _page.Locator(".inventory_item_price");
        var count = await priceElements.CountAsync();

        for (int i = 0; i < count; i++)
        {
            var priceText = await priceElements.Nth(i).InnerTextAsync();
            // Remove dollar sign and parse
            if (decimal.TryParse(priceText.Replace("$", ""), out var price))
            {
                prices.Add(price);
            }
        }

        return prices;
    }
}
