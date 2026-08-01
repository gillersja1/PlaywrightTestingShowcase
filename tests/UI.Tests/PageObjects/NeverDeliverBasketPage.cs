using Microsoft.Playwright;

namespace UI.Tests.PageObjects;

/// <summary>
/// Page object for the Never Deliver website basket/cart page.
/// </summary>
public class NeverDeliverBasketPage
{
    private readonly IPage _page;

    private ILocator BasketItems => _page.Locator("[class*='basket-item'], [class*='cart-item']");
    private ILocator ProceedToCheckoutButton => _page.Locator("button:has-text('Proceed to Checkout'), a:has-text('Checkout')");
    private ILocator RemoveButtons => _page.Locator("button:has-text('Remove')");
    private ILocator EmptyBasketMessage => _page.Locator("text=/basket|cart.*empty/i");
    private ILocator TotalPrice => _page.Locator("[class*='total']");

    public NeverDeliverBasketPage(IPage page)
    {
        _page = page;
    }

    public async Task<int> GetBasketItemCountAsync()
    {
        return await BasketItems.CountAsync();
    }

    public async Task<bool> IsBasketEmptyAsync()
    {
        return await EmptyBasketMessage.IsVisibleAsync();
    }

    public async Task RemoveItemAsync(string itemName)
    {
        var item = BasketItems.Filter(new LocatorFilterOptions { HasText = itemName });
        await item.Locator("button:has-text('Remove')").ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task ProceedToCheckoutAsync()
    {
        await ProceedToCheckoutButton.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task<string> GetTotalPriceAsync()
    {
        if (await TotalPrice.IsVisibleAsync())
        {
            return await TotalPrice.InnerTextAsync();
        }
        return string.Empty;
    }

    public async Task<List<string>> GetBasketItemNamesAsync()
    {
        var names = new List<string>();
        var count = await BasketItems.CountAsync();

        for (int i = 0; i < count; i++)
        {
            var name = await BasketItems.Nth(i).Locator("[class*='title'], h3, h4").InnerTextAsync();
            names.Add(name);
        }

        return names;
    }
}
