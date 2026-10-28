using System.Text.RegularExpressions;
using Microsoft.Playwright;
using UI.Tests.TestData;
using static Microsoft.Playwright.Assertions;

namespace UI.Tests.PageObjects;

/// <summary>
/// Page object for the slide-out cart drawer on never-deliver-shop.vercel.app.
/// The drawer has no dialog role, so it's located through its
/// "Shopping Cart (N)" heading, which is rendered on every page.
/// </summary>
public class NeverDeliverCartDrawer
{
    private readonly IPage _page;

    private ILocator Heading => _page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { NameRegex = new Regex(@"Shopping Cart \(\d+\)") });
    private ILocator Drawer => Heading.Locator("xpath=../..");
    private ILocator CloseButton => _page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close cart drawer" });
    private ILocator RemoveButtons => Drawer.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Remove", Exact = true });
    private ILocator Subtotal => Drawer.GetByText(new Regex(@"Subtotal \(\d+ items?\):")).Locator("xpath=..");
    private ILocator CheckoutLink => Drawer.GetByRole(AriaRole.Link, new LocatorGetByRoleOptions { Name = "Proceed to Checkout" });
    private ILocator EmptyMessage => Drawer.GetByText("Your cart is empty");

    public NeverDeliverCartDrawer(IPage page)
    {
        _page = page;
    }

    public async Task WaitUntilOpenAsync() => await Expect(CloseButton).ToBeVisibleAsync();

    /// <summary>
    /// Total quantity in the cart, read from the "Shopping Cart (N)" heading.
    /// </summary>
    public async Task<int> GetItemCountAsync()
    {
        var text = await Heading.InnerTextAsync();
        return int.Parse(Regex.Match(text, @"\((\d+)\)").Groups[1].Value);
    }

    public async Task WaitForItemCountAsync(int expected) =>
        await Expect(Heading).ToHaveTextAsync($"Shopping Cart ({expected})");

    public async Task<int> GetLineCountAsync() => await RemoveButtons.CountAsync();

    public async Task<decimal> GetSubtotalAsync() => NeverDeliverTestData.ParsePounds(await Subtotal.InnerTextAsync());

    public async Task<bool> IsEmptyMessageVisibleAsync() => await EmptyMessage.IsVisibleAsync();

    private ILocator Line(int index) => RemoveButtons.Nth(index).Locator("xpath=..").Locator("xpath=..");

    /// <summary>
    /// The current (discounted) unit price of a cart line.
    /// </summary>
    public async Task<decimal> GetLineUnitPriceAsync(int index) =>
        NeverDeliverTestData.ParsePounds(await Line(index).InnerTextAsync());

    public async Task<string> GetLineTextAsync(int index) => await Line(index).InnerTextAsync();

    public ILocator IncreaseQuantityButton(int index) =>
        Line(index).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "+", Exact = true });

    public async Task IncreaseQuantityAsync(int index) => await IncreaseQuantityButton(index).ClickAsync();

    public async Task RemoveLineAsync(int index)
    {
        var before = await GetLineCountAsync();
        await RemoveButtons.Nth(index).ClickAsync();
        await Expect(RemoveButtons).ToHaveCountAsync(before - 1);
    }

    public async Task<NeverDeliverCheckoutPage> ProceedToCheckoutAsync()
    {
        await CheckoutLink.ClickAsync();
        var checkout = new NeverDeliverCheckoutPage(_page);
        await checkout.WaitForAddressStepAsync();
        return checkout;
    }
}
