using Microsoft.Playwright;

namespace UI.Tests.PageObjects;

/// <summary>
/// Page object for the cart page on saucedemo.com
/// </summary>
public class CartPage
{
    private readonly IPage _page;

    private ILocator CartItems => _page.Locator(".cart_item");
    private ILocator CheckoutButton => _page.Locator("#checkout");

    public CartPage(IPage page)
    {
        _page = page;
    }

    public async Task GotoAsync()
    {
        await _page.ClickAsync(".shopping_cart_link");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task<int> GetItemCountAsync()
    {
        return await CartItems.CountAsync();
    }

    public async Task RemoveItemFromCartAsync(string itemName)
    {
        var item = CartItems.Filter(new LocatorFilterOptions { HasText = itemName });
        var before = await CartItems.CountAsync();
        await item.Locator("button").Filter(new LocatorFilterOptions { HasText = "Remove" }).ClickAsync();
        // wait until the cart item count decreases by one
        await _page.WaitForFunctionAsync($"() => document.querySelectorAll('.cart_item').length === {before - 1}");
    }

    public async Task ClickCheckoutAsync()
    {
        await CheckoutButton.ClickAsync();
    }
}
