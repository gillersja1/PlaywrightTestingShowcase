using Microsoft.Playwright;
using UI.Tests.TestData;

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
    private ILocator CartLink => _page.Locator(".shopping_cart_link");
    private ILocator SortDropdown => _page.Locator(".page_wrapper .select_container select");
    private ILocator ProductImages => _page.Locator(".inventory_item_img");

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
            return 0;

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
            if (decimal.TryParse(priceText.Replace("$", ""), out var price))
            {
                prices.Add(price);
            }
        }

        return prices;
    }

    public async Task RemoveItemFromCartAsync(string itemName)
    {
        var item = InventoryItems.Filter(new LocatorFilterOptions { HasText = itemName });
        await item.Locator("button").Filter(new LocatorFilterOptions { HasText = "Remove" }).ClickAsync();
    }

    public async Task<bool> IsItemInCartAsync(string itemName)
    {
        var item = InventoryItems.Filter(new LocatorFilterOptions { HasText = itemName });
        var button = item.Locator("button").Filter(new LocatorFilterOptions { HasText = "Remove" });
        return await button.IsVisibleAsync();
    }

    public async Task SortByAsync(string sortOption)
    {
        await SortDropdown.SelectOptionAsync(sortOption);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task<List<string>> GetAllProductNamesAsync()
    {
        var names = new List<string>();
        var count = await InventoryItems.CountAsync();

        for (int i = 0; i < count; i++)
        {
            var name = await InventoryItems.Nth(i).Locator(".inventory_item_name").InnerTextAsync();
            names.Add(name);
        }

        return names;
    }

    public async Task<bool> AreAllProductsDisplayedAsync()
    {
        var imageCount = await ProductImages.CountAsync();
        var itemCount = await GetProductCountAsync();
        return imageCount == itemCount && imageCount > 0;
    }

    public async Task ClickProductAsync(string productName)
    {
        var item = InventoryItems.Filter(new LocatorFilterOptions { HasText = productName });
        await item.Locator(".inventory_item_name").ClickAsync();
    }

    public async Task GoToCartAsync()
    {
        await CartLink.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }
}
