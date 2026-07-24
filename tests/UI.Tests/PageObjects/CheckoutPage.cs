using Microsoft.Playwright;

namespace UI.Tests.PageObjects;

public class CheckoutPage
{
    private readonly IPage _page;

    private ILocator FirstName => _page.Locator("#first-name");
    private ILocator LastName => _page.Locator("#last-name");
    private ILocator PostalCode => _page.Locator("#postal-code");
    private ILocator ContinueButton => _page.Locator("#continue");
    private ILocator FinishButton => _page.Locator("#finish");
    private ILocator CompleteHeader => _page.Locator(".complete-header");

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
    }

    public async Task ClickFinishAsync()
    {
        await FinishButton.ClickAsync();
    }

    public async Task<string> GetCompleteHeaderAsync()
    {
        return await CompleteHeader.InnerTextAsync();
    }
}
