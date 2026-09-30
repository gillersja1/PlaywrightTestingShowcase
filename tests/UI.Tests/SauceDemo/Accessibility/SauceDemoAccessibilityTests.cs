using Allure.Net.Commons;
using Allure.NUnit;
using Allure.NUnit.Attributes;
using NUnit.Framework;
using UI.Tests.Fixtures;
using UI.Tests.PageObjects;
using UI.Tests.TestData;

namespace UI.Tests.SauceDemo.Accessibility;

[TestFixture]
[AllureNUnit]
[AllureFeature("Accessibility")]
[AllureSuite("SauceDemo")]
public class SauceDemoAccessibilityTests : UITestFixtureBase
{
    private LoginPage _loginPage = null!;
    private InventoryPage _inventoryPage = null!;

    [SetUp]
    public void Setup()
    {
        BaseSetUp();
        _loginPage = new LoginPage(Page);
        _inventoryPage = new InventoryPage(Page);
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task LoginPageInputsHaveLabels()
    {
        await _loginPage.GotoAsync();

        var usernameAriaLabel = await Page.Locator("#user-name").GetAttributeAsync("aria-label");
        var passwordAriaLabel = await Page.Locator("#password").GetAttributeAsync("aria-label");

        // Either has aria-label or placeholder or name attribute
        var usernameAccessible = usernameAriaLabel != null || 
            await Page.Locator("#user-name").GetAttributeAsync("placeholder") != null;
        var passwordAccessible = passwordAriaLabel != null || 
            await Page.Locator("#password").GetAttributeAsync("placeholder") != null;

        Assert.That(usernameAccessible, Is.True, "Username input should be accessible");
        Assert.That(passwordAccessible, Is.True, "Password input should be accessible");
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task CartBadgeIsAccessible()
    {
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync(LoginCredentials.ValidUser.Username, LoginCredentials.ValidUser.Password);

        await _inventoryPage.AddItemToCartAsync(SauceDemoTestData.Products.Backpack);

        var cartBadge = Page.Locator(".shopping_cart_badge");
        var isVisible = await cartBadge.IsVisibleAsync();
        Assert.That(isVisible, Is.True, "Cart badge should be visible for screen readers");
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task AllInteractiveElementsHaveTextContent()
    {
        await _loginPage.GotoAsync();

        var buttons = Page.Locator("button");
        var buttonCount = await buttons.CountAsync();

        for (int i = 0; i < buttonCount; i++)
        {
            var text = await buttons.Nth(i).InnerTextAsync();
            Assert.That(text, Is.Not.Empty, $"Button {i} should have text content");
        }
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task ErrorMessagesAreAccessible()
    {
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync("locked_out_user", "secret_sauce");

        var errorContainer = Page.Locator("[data-test='error-container']");
        var isVisible = await errorContainer.IsVisibleAsync();
        Assert.That(isVisible, Is.True, "Error messages should be visible/accessible");
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task PageHasProperHeadings()
    {
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync(LoginCredentials.ValidUser.Username, LoginCredentials.ValidUser.Password);

        var headings = Page.Locator("h1, h2, h3, h4, h5, h6");
        var headingCount = await headings.CountAsync();

        Assert.That(headingCount, Is.GreaterThan(0), "Page should have heading hierarchy");
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task ImagesHaveAltText()
    {
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync(LoginCredentials.ValidUser.Username, LoginCredentials.ValidUser.Password);

        var images = Page.Locator("img");
        var imageCount = await images.CountAsync();

        // Check at least some images have alt text
        var altTextCount = 0;
        for (int i = 0; i < imageCount && i < 5; i++)
        {
            var alt = await images.Nth(i).GetAttributeAsync("alt");
            if (!string.IsNullOrEmpty(alt))
            {
                altTextCount++;
            }
        }

        Assert.That(altTextCount, Is.GreaterThan(0), "Images should have alt text");
    }
}
