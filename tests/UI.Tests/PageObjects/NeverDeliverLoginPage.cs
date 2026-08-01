using Microsoft.Playwright;

namespace UI.Tests.PageObjects;

/// <summary>
/// Page object for the Never Deliver website login page.
/// </summary>
public class NeverDeliverLoginPage
{
    private readonly IPage _page;

    private ILocator EmailInput => _page.Locator("input[type='email']");
    private ILocator PasswordInput => _page.Locator("input[type='password']");
    private ILocator LoginButton => _page.Locator("button:has-text('Sign in')");
    private ILocator ErrorMessage => _page.Locator("[role='alert']");

    public NeverDeliverLoginPage(IPage page)
    {
        _page = page;
    }

    public async Task GotoAsync()
    {
        await _page.GotoAsync("https://neverdeliver.co.uk/");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task LoginAsync(string email, string password)
    {
        await EmailInput.FillAsync(email);
        await PasswordInput.FillAsync(password);
        await LoginButton.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task<string> GetErrorMessageAsync()
    {
        if (await ErrorMessage.IsVisibleAsync())
        {
            return await ErrorMessage.InnerTextAsync();
        }
        return string.Empty;
    }

    public async Task<bool> IsLoginPageDisplayedAsync()
    {
        return await LoginButton.IsVisibleAsync();
    }
}
