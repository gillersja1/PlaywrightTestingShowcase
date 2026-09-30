using Microsoft.Playwright;
using UI.Tests.TestData;

namespace UI.Tests.PageObjects;

/// <summary>
/// Page object for the saucedemo.com login page.
/// </summary>
public class LoginPage
{
    private readonly IPage _page;

    private ILocator UsernameInput => _page.Locator("#user-name");
    private ILocator PasswordInput => _page.Locator("#password");
    private ILocator LoginButton => _page.Locator("#login-button");
    private ILocator ErrorMessage => _page.Locator("[data-test='error']");
    private ILocator ErrorContainer => _page.Locator("[data-test='error-container']");

    public LoginPage(IPage page)
    {
        _page = page;
    }

    public async Task GotoAsync()
    {
        await _page.GotoAsync(SauceDemoTestData.LoginUrl);
    }

    public async Task LoginAsync(string username, string password)
    {
        await UsernameInput.FillAsync(username);
        await PasswordInput.FillAsync(password);
        await LoginButton.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task<string> GetErrorMessageAsync()
    {
        try
        {
            // Try to get error text if it exists
            await ErrorMessage.WaitForAsync(new LocatorWaitForOptions { Timeout = 2000 });
            return await ErrorMessage.InnerTextAsync();
        }
        catch
        {
            // If element doesn't exist or timeout, return empty
            return string.Empty;
        }
    }

    public async Task<bool> IsErrorMessageVisibleAsync()
    {
        try
        {
            // Check if error message element exists and has text
            await ErrorMessage.WaitForAsync(new LocatorWaitForOptions { Timeout = 2000 });
            var text = await ErrorMessage.InnerTextAsync();
            return !string.IsNullOrEmpty(text);
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> GetUsernameFieldValueAsync()
    {
        return await UsernameInput.InputValueAsync();
    }

    public async Task<string> GetPasswordFieldValueAsync()
    {
        return await PasswordInput.InputValueAsync();
    }

    public async Task<bool> IsLoginButtonEnabledAsync()
    {
        return await LoginButton.IsEnabledAsync();
    }

    public async Task<bool> IsLoginPageDisplayedAsync()
    {
        return await UsernameInput.IsVisibleAsync() && await PasswordInput.IsVisibleAsync();
    }

    public async Task ClearFieldsAsync()
    {
        await UsernameInput.ClearAsync();
        await PasswordInput.ClearAsync();
    }

    public async Task LoginViaEnterKeyAsync(string username, string password)
    {
        await UsernameInput.FillAsync(username);
        await PasswordInput.FillAsync(password);
        await PasswordInput.PressAsync("Enter");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }
}
