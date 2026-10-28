using Allure.Net.Commons;
using Allure.NUnit;
using Allure.NUnit.Attributes;
using Microsoft.Playwright;
using NUnit.Framework;
using UI.Tests.Fixtures;
using UI.Tests.PageObjects;
using UI.Tests.TestData;

namespace UI.Tests.SauceDemo.Accessibility;

/// <summary>
/// Automated accessibility checks with axe-core (the engine behind Deque's
/// axe DevTools and Lighthouse's accessibility audit), plus keyboard and
/// screen-reader behavior that a rules engine can't verify on its own.
/// </summary>
[TestFixture]
[AllureNUnit]
[AllureFeature("Accessibility")]
[AllureSuite("SauceDemo")]
public class SauceDemoAccessibilityTests : UITestFixtureBase
{
    private LoginPage _loginPage = null!;
    private InventoryPage _inventoryPage = null!;
    private CartPage _cartPage = null!;
    private CheckoutPage _checkoutPage = null!;

    [SetUp]
    public void Setup()
    {
        BaseSetUp();
        _loginPage = new LoginPage(Page);
        _inventoryPage = new InventoryPage(Page);
        _cartPage = new CartPage(Page);
        _checkoutPage = new CheckoutPage(Page);
    }

    public enum Screen { Login, LoginError, Inventory, ProductDetails, Cart, CheckoutInformation, CheckoutOverview, CheckoutComplete }

    /// <summary>
    /// Walks the purchase flow up to the requested screen, so every page in
    /// the flow is scanned in the state a real user would see it.
    /// </summary>
    private async Task NavigateToAsync(Screen screen)
    {
        await _loginPage.GotoAsync();
        if (screen == Screen.Login) return;

        if (screen == Screen.LoginError)
        {
            await _loginPage.LoginAsync(LoginCredentials.ValidUser.Username, "wrong_password");
            return;
        }

        await _loginPage.LoginAsync(LoginCredentials.ValidUser.Username, LoginCredentials.ValidUser.Password);
        if (screen == Screen.Inventory) return;

        if (screen == Screen.ProductDetails)
        {
            await _inventoryPage.ClickProductAsync(SauceDemoTestData.Products.Backpack);
            return;
        }

        await _inventoryPage.AddItemToCartAsync(SauceDemoTestData.Products.Backpack);
        await _inventoryPage.GoToCartAsync();
        if (screen == Screen.Cart) return;

        await _cartPage.ClickCheckoutAsync();
        if (screen == Screen.CheckoutInformation) return;

        await _checkoutPage.FillInformationAsync(CheckoutInfo.Valid.FirstName, CheckoutInfo.Valid.LastName, CheckoutInfo.Valid.PostalCode);
        await _checkoutPage.ClickContinueAsync();
        if (screen == Screen.CheckoutOverview) return;

        await _checkoutPage.ClickFinishAsync();
    }

    [Test]
    [AllureSeverity(SeverityLevel.critical)]
    [Description("Every screen in the purchase flow has zero WCAG 2.1 A/AA violations.")]
    public async Task Screen_HasNoWcagAaViolations([Values] Screen screen)
    {
        await NavigateToAsync(screen);

        var violations = await AxeScanner.ScanAsync(Page, AxeScanner.WcagAaTags);

        Assert.That(violations, Is.Empty, $"WCAG 2.1 AA violations on {screen}:{Environment.NewLine}{AxeScanner.Describe(violations)}");
    }

    [Test]
    [Category("KnownDefect")]
    [AllureSeverity(SeverityLevel.minor)]
    [AllureIssue("SD-A11Y-1")]
    [Description("Known defect SD-A11Y-1: axe best-practice rules fail - no <h1> on any page, " +
                 "the login logo sits outside a landmark, and the error <h3> carries role=\"alert\".")]
    public async Task Screen_MeetsAxeBestPractices([Values(Screen.Login, Screen.LoginError, Screen.Inventory)] Screen screen)
    {
        await NavigateToAsync(screen);

        var violations = await AxeScanner.ScanAsync(Page, AxeScanner.BestPracticeTags);

        Assert.That(violations, Is.Empty, $"Best-practice violations on {screen}:{Environment.NewLine}{AxeScanner.Describe(violations)}");
    }

    [Test]
    [AllureSeverity(SeverityLevel.critical)]
    [Description("A keyboard-only user can log in: fields are focusable in order and Enter submits.")]
    public async Task KeyboardOnlyUser_CanLogIn()
    {
        await _loginPage.GotoAsync();

        await Page.Locator("#user-name").FocusAsync();
        await Page.Keyboard.TypeAsync(LoginCredentials.ValidUser.Username);
        await Page.Keyboard.PressAsync("Tab");
        await Expect(Page.Locator("#password")).ToBeFocusedAsync();
        await Page.Keyboard.TypeAsync(LoginCredentials.ValidUser.Password);
        await Page.Keyboard.PressAsync("Enter");

        await Expect(Page).ToHaveURLAsync(SauceDemoTestData.InventoryUrl);
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    [Description("A keyboard-only user can add a product to the cart with Enter.")]
    public async Task KeyboardOnlyUser_CanAddItemToCart()
    {
        await NavigateToAsync(Screen.Inventory);

        await Page.Locator("[data-test='add-to-cart-sauce-labs-backpack']").FocusAsync();
        await Page.Keyboard.PressAsync("Enter");

        await Expect(Page.Locator(".shopping_cart_badge")).ToHaveTextAsync("1");
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    [Description("Login errors are exposed as an ARIA alert, so screen readers announce them without moving focus.")]
    public async Task LoginError_IsAnnouncedToScreenReaders()
    {
        await NavigateToAsync(Screen.LoginError);

        var alert = Page.GetByRole(AriaRole.Alert);
        await Expect(alert).ToBeVisibleAsync();
        await Expect(alert).ToContainTextAsync(SauceDemoTestData.ErrorMessages.InvalidCredentials);
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    [Description("The error's dismiss button is an icon, so it needs an accessible name.")]
    public async Task LoginErrorDismissButton_HasAccessibleName()
    {
        await NavigateToAsync(Screen.LoginError);

        await Page.GetByRole(AriaRole.Button, new() { Name = "Dismiss error" }).ClickAsync();

        await Expect(Page.GetByRole(AriaRole.Alert)).ToHaveCountAsync(0);
    }
}
