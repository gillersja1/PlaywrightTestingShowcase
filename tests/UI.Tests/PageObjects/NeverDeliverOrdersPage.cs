using Microsoft.Playwright;
using UI.Tests.TestData;
using static Microsoft.Playwright.Assertions;

namespace UI.Tests.PageObjects;

/// <summary>
/// Page object for the "Returns &amp; Orders" order history page on
/// never-deliver-shop.vercel.app. Orders are kept in browser storage, so
/// each test's fresh browser context starts with an empty history.
/// </summary>
public class NeverDeliverOrdersPage
{
    private readonly IPage _page;

    private ILocator Main => _page.Locator("#main-content");
    private ILocator Heading => _page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Your Orders" });
    private ILocator EmptyHeading => _page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "No orders yet" });
    private ILocator FirstOrder => Main.GetByText("ORDER #").First;
    private ILocator StartShoppingLink => Main.GetByRole(AriaRole.Link, new LocatorGetByRoleOptions { Name = "Start Shopping" });

    public NeverDeliverOrdersPage(IPage page)
    {
        _page = page;
    }

    public async Task GotoAsync()
    {
        await _page.GotoAsync(NeverDeliverTestData.OrdersUrl);
        await Expect(Heading).ToBeVisibleAsync();
        // The heading renders before the history loads from storage; wait
        // for either the empty state or the first order card.
        await Expect(EmptyHeading.Or(FirstOrder)).ToBeVisibleAsync();
    }

    public async Task<bool> IsEmptyStateVisibleAsync() => await EmptyHeading.IsVisibleAsync();

    public async Task<string> GetPageTextAsync() => await Main.InnerTextAsync();

    public async Task StartShoppingAsync() => await StartShoppingLink.ClickAsync();
}
