using Microsoft.Playwright;
using UI.Tests.TestData;

namespace UI.Tests.PageObjects;

/// <summary>
/// Page object for the cart page on saucedemo.com
/// </summary>
public class CartPage
{
    private readonly IPage _page;

    private ILocator CartItems => _page.Locator(".cart_item");
    private ILocator CheckoutButton => _page.Locator("#checkout");
    private ILocator ContinueShoppingButton => _page.Locator("#continue-shopping");
    private ILocator SubtotalLabel => _page.Locator(".subtotal_label");
    private ILocator TaxLabel => _page.Locator(".tax_label");
    private ILocator TotalLabel => _page.Locator(".total_label");

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
        // Wait until cart item count decreases
        await _page.WaitForFunctionAsync($"() => document.querySelectorAll('.cart_item').length === {before - 1}");
    }

    public async Task ClickCheckoutAsync()
    {
        await CheckoutButton.ClickAsync();
    }

    public async Task ClickContinueShoppingAsync()
    {
        await ContinueShoppingButton.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task<bool> IsCartEmptyAsync()
    {
        return await GetItemCountAsync() == 0;
    }

    public async Task<decimal> GetSubtotalAsync()
    {
        var text = await SubtotalLabel.InnerTextAsync();
        return ExtractPrice(text);
    }

    public async Task<decimal> GetTaxAsync()
    {
        var text = await TaxLabel.InnerTextAsync();
        return ExtractPrice(text);
    }

    public async Task<decimal> GetTotalAsync()
    {
        var text = await TotalLabel.InnerTextAsync();
        return ExtractPrice(text);
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

    private static decimal ExtractPrice(string text)
    {
        var match = System.Text.RegularExpressions.Regex.Match(text, @"\$([0-9.]+)");
        if (match.Success && decimal.TryParse(match.Groups[1].Value, out var price))
        {
            return price;
        }
        return 0;
    }
}
