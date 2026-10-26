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
    private ILocator ErrorMessage => _page.Locator("[data-test='error']");
    private ILocator BackButton => _page.Locator("#back-to-products");
    private ILocator CartItems => _page.Locator(".cart_item");
    private ILocator SubtotalLabel => _page.Locator(".summary_subtotal_label");
    private ILocator TaxLabel => _page.Locator(".summary_tax_label");
    private ILocator TotalLabel => _page.Locator(".summary_total_label");

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
        await _page.WaitForFunctionAsync(
            "() => document.querySelector('.summary_info') || document.querySelector('[data-test=\"error\"]')");
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
        if (!await ErrorMessage.IsVisibleAsync())
            return string.Empty;
        return await ErrorMessage.InnerTextAsync();
    }

    public async Task<bool> IsErrorMessageVisibleAsync()
    {
        return await ErrorMessage.IsVisibleAsync();
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

    public async Task<decimal> GetSubtotalAsync() => ExtractPrice(await SubtotalLabel.InnerTextAsync());

    public async Task<decimal> GetTaxAsync() => ExtractPrice(await TaxLabel.InnerTextAsync());

    public async Task<decimal> GetTotalAsync() => ExtractPrice(await TotalLabel.InnerTextAsync());

    private static decimal ExtractPrice(string text)
    {
        var match = System.Text.RegularExpressions.Regex.Match(text, @"\$([0-9.]+)");
        return match.Success && decimal.TryParse(match.Groups[1].Value, out var price) ? price : 0;
    }
}
