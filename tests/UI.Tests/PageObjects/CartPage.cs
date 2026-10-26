using Microsoft.Playwright;
using UI.Tests.TestData;

namespace UI.Tests.PageObjects;

/// <summary>
/// Page object for the cart page on saucedemo.com
/// </summary>
public class CartPage
{
    private readonly IPage _page;

    private ILocator PageTitle => _page.Locator(".title");
    private ILocator CartList => _page.Locator(".cart_list");
    private ILocator CartItems => _page.Locator(".cart_item");
    private ILocator CheckoutButton => _page.Locator("#checkout");
    private ILocator ContinueShoppingButton => _page.Locator("#continue-shopping");

    public CartPage(IPage page)
    {
        _page = page;
    }

    public async Task GotoAsync()
    {
        await _page.ClickAsync(".shopping_cart_link");
        await PageTitle.Filter(new LocatorFilterOptions { HasText = "Your Cart" })
            .WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await CartList.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
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
        // Wait until cart item count decreases
        await _page.WaitForFunctionAsync($"() => document.querySelectorAll('.cart_item').length === {before - 1}");
    }

    public async Task ClickCheckoutAsync()
    {
        await CheckoutButton.ClickAsync();
        await _page.Locator("#first-name")
            .WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    public async Task ClickContinueShoppingAsync()
    {
        await ContinueShoppingButton.ClickAsync();
        await PageTitle.Filter(new LocatorFilterOptions { HasText = "Products" })
            .WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    public async Task<bool> IsCartEmptyAsync()
    {
        return await GetItemCountAsync() == 0;
    }

    public async Task<List<string>> GetCartItemNamesAsync()
    {
        var names = new List<string>();
        var count = await CartItems.CountAsync();

        for (int i = 0; i < count; i++)
        {
            var name = await CartItems.Nth(i).Locator(".inventory_item_name").InnerTextAsync();
            names.Add(name);
        }

        return names;
    }
}
