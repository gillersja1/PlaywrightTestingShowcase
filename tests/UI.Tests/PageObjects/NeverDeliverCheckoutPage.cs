using System.Text.RegularExpressions;
using Microsoft.Playwright;
using UI.Tests.TestData;
using static Microsoft.Playwright.Assertions;

namespace UI.Tests.PageObjects;

/// <summary>
/// Page object for the three-step checkout (Delivery, Payment, Processing)
/// on never-deliver-shop.vercel.app. Payment uses a locked, simulated
/// "card on file", so there are no card fields to fill.
/// </summary>
public class NeverDeliverCheckoutPage
{
    private readonly IPage _page;

    private ILocator Main => _page.Locator("#main-content");
    private ILocator ContinueToPaymentButton => _page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Continue to Payment" });
    private ILocator PlaceOrderButton => _page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Place Order" });
    private ILocator NextDayDeliveryOption => _page.GetByRole(AriaRole.Radio, new PageGetByRoleOptions { NameRegex = new Regex("^Next Day Delivery") });
    private ILocator OrderTotal => Main.GetByText("Order total:").Locator("xpath=..");
    private ILocator ConfirmedHeading => _page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = NeverDeliverTestData.OrderConfirmedHeading });

    public NeverDeliverCheckoutPage(IPage page)
    {
        _page = page;
    }

    public async Task GotoAsync()
    {
        await _page.GotoAsync(NeverDeliverTestData.CheckoutUrl);
        await WaitForAddressStepAsync();
    }

    public async Task WaitForAddressStepAsync() =>
        await Expect(AddressField(NeverDeliverTestData.AddressFields.FullName)).ToBeVisibleAsync();

    /// <summary>
    /// Address inputs have visible labels that aren't programmatically
    /// associated (no for/id), so they can only be found by placeholder.
    /// </summary>
    public ILocator AddressField(string placeholder) =>
        _page.GetByPlaceholder(placeholder, new PageGetByPlaceholderOptions { Exact = true });

    public async Task FillAddressAsync(DeliveryAddress address)
    {
        await AddressField(NeverDeliverTestData.AddressFields.FullName).FillAsync(address.FullName);
        await AddressField(NeverDeliverTestData.AddressFields.Street).FillAsync(address.Street);
        await AddressField(NeverDeliverTestData.AddressFields.City).FillAsync(address.City);
        await AddressField(NeverDeliverTestData.AddressFields.County).FillAsync(address.County);
        await AddressField(NeverDeliverTestData.AddressFields.Postcode).FillAsync(address.Postcode);
    }

    public async Task ClearAddressFieldAsync(string placeholder) => await AddressField(placeholder).FillAsync("");

    public async Task<bool> IsContinueToPaymentEnabledAsync() => await ContinueToPaymentButton.IsEnabledAsync();

    public async Task<string> GetNextDayDeliveryLabelAsync() =>
        await NextDayDeliveryOption.Locator("xpath=ancestor::label[1]").InnerTextAsync();

    public async Task SelectNextDayDeliveryAsync() => await NextDayDeliveryOption.CheckAsync();

    public async Task<decimal> GetOrderTotalAsync() => NeverDeliverTestData.ParsePounds(await OrderTotal.InnerTextAsync());

    public async Task WaitForOrderTotalToChangeFromAsync(decimal previous) =>
        await Expect(OrderTotal).Not.ToHaveTextAsync(new Regex(Regex.Escape(previous.ToString("N2", System.Globalization.CultureInfo.InvariantCulture)) + "$"));

    public async Task ContinueToPaymentAsync()
    {
        await ContinueToPaymentButton.ClickAsync();
        await Expect(PlaceOrderButton).ToBeVisibleAsync();
    }

    public async Task PlaceOrderAsync()
    {
        await PlaceOrderButton.ClickAsync();
        // A simulated "Processing your order" step runs for a couple of seconds first.
        await Expect(ConfirmedHeading).ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions { Timeout = 15000 });
    }

    public async Task<string> GetOrderNumberAsync()
    {
        var text = await Main.InnerTextAsync();
        return NeverDeliverTestData.OrderNumberPattern.Match(text).Value;
    }

    public async Task<string> GetDeliveringToAsync() =>
        await Main.GetByText("Delivering to:").Locator("xpath=..").InnerTextAsync();
}
