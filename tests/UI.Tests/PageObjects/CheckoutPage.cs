using Microsoft.Playwright;

namespace UI.Tests.PageObjects;

/// <summary>
/// Page object for checkout page on saucedemo.com
/// </summary>
public class CheckoutPage
{
    private readonly IPage _page;

    private ILocator FirstName => _page.Locator("#first-name");
    private ILocator LastName => _page.Locator("#last-name");
    private ILocator PostalCode => _page.Locator("#postal-code");
    private ILocator ContinueButton => _page.Locator("#continue");
    private ILocator FinishButton => _page.Locator("#finish");
    private ILocator CompleteHeader => _page.Locator(".complete-header");
    private ILocator ErrorContainer => _page.Locator("[data-test='error-container']");
    private ILocator ErrorMessage => _page.Locator("[data-test='error']");
    private ILocator BackButton => _page.Locator("#back-to-products");
    private ILocator CartItems => _page.Locator(".cart_item");

    public CheckoutPage(IPage page)
    {
        _page = page;
    }

    public async Task FillInformationAsync(string firstName, string lastName, string postalCode)
    {
        await FirstName.FillAsync(firstName);
        await LastName.FillAsync(lastName);
        await PostalCode.FillAsync(postalCode);
    }

    public async Task ClickContinueAsync()
    {
        await ContinueButton.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task ClickFinishAsync()
    {
        await FinishButton.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task<string> GetCompleteHeaderAsync()
    {
        return await CompleteHeader.InnerTextAsync();
    }

    public async Task<string> GetErrorMessageAsync()
    {
        if (!await ErrorContainer.IsVisibleAsync())
            return string.Empty;
        return await ErrorMessage.InnerTextAsync();
    }

    public async Task<bool> IsErrorMessageVisibleAsync()
    {
        return await ErrorContainer.IsVisibleAsync();
    }

    public async Task<string> GetFirstNameValueAsync()
    {
        return await FirstName.InputValueAsync();
    }

    public async Task<string> GetLastNameValueAsync()
    {
        return await LastName.InputValueAsync();
    }

    public async Task<string> GetPostalCodeValueAsync()
    {
        return await PostalCode.InputValueAsync();
    }

    public async Task ClearAllFieldsAsync()
    {
        await FirstName.ClearAsync();
        await LastName.ClearAsync();
        await PostalCode.ClearAsync();
    }

    public async Task ClickBackAsync()
    {
        await BackButton.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task<int> GetCartItemCountAsync()
    {
        return await CartItems.CountAsync();
    }
}
