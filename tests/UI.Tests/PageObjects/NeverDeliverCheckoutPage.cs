using Microsoft.Playwright;

namespace UI.Tests.PageObjects;

/// <summary>
/// Page object for the Never Deliver website checkout page.
/// </summary>
public class NeverDeliverCheckoutPage
{
    private readonly IPage _page;

    private ILocator FirstNameInput => _page.Locator("input[name*='first'], input[placeholder*='First']");
    private ILocator LastNameInput => _page.Locator("input[name*='last'], input[placeholder*='Last']");
    private ILocator EmailInput => _page.Locator("input[type='email']");
    private ILocator AddressInput => _page.Locator("input[placeholder*='Address'], input[name*='address']");
    private ILocator PostcodeInput => _page.Locator("input[placeholder*='postcode'], input[placeholder*='zip']");
    private ILocator CompleteOrderButton => _page.Locator("button:has-text('Complete Order'), button:has-text('Place Order')");
    private ILocator SuccessMessage => _page.Locator("text=/thank you|order.*confirm|success/i");

    public NeverDeliverCheckoutPage(IPage page)
    {
        _page = page;
    }

    public async Task FillDeliveryDetailsAsync(string firstName, string lastName, string email, string address, string postcode)
    {
        await FirstNameInput.FillAsync(firstName);
        await LastNameInput.FillAsync(lastName);
        await EmailInput.FillAsync(email);
        await AddressInput.FillAsync(address);
        await PostcodeInput.FillAsync(postcode);
    }

    public async Task CompleteOrderAsync()
    {
        await CompleteOrderButton.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task<bool> IsOrderSuccessfulAsync()
    {
        return await SuccessMessage.IsVisibleAsync();
    }

    public async Task<string> GetSuccessMessageAsync()
    {
        if (await SuccessMessage.IsVisibleAsync())
        {
            return await SuccessMessage.InnerTextAsync();
        }
        return string.Empty;
    }
}
