using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using Allure.Net.Commons;
using Allure.NUnit;
using Allure.NUnit.Attributes;
using Microsoft.Playwright;
using NUnit.Framework;
using UI.Tests.Fixtures;
using UI.Tests.PageObjects;
using UI.Tests.TestData;

namespace UI.Tests.SauceDemo.UserContracts;

/// <summary>
/// saucedemo ships demo accounts with deliberately planted bugs. Every user
/// runs the same contract: the behavior standard_user gets. A check that a
/// user is known to break is tagged KnownDefect, so CI stays green while the
/// reports show a defect matrix of exactly which bugs each account exposes,
/// and flag it if a planted bug ever disappears or a new one appears.
/// </summary>
[TestFixture]
[AllureNUnit]
[AllureFeature("User Contracts")]
[AllureSuite("SauceDemo")]
public class SauceDemoUserContractTests : UITestFixtureBase
{
    public enum Check
    {
        ProductImagesAreDistinct,
        PricesMatchCatalog,
        SortByPriceLowToHigh,
        AddingThreeItemsShowsThree,
        RemovingItemEmptiesCart,
        ProductLinkOpensThatProduct,
        CheckoutFormKeepsEnteredValues,
        CheckoutCompletes,
        LoginIsResponsive
    }

    /// <summary>
    /// The planted defects observed for each account. Everything not listed
    /// here must behave exactly like standard_user.
    /// </summary>
    private static readonly Dictionary<string, Check[]> KnownDefects = new()
    {
        ["standard_user"] = Array.Empty<Check>(),
        ["problem_user"] = new[]
        {
            Check.ProductImagesAreDistinct, Check.SortByPriceLowToHigh, Check.AddingThreeItemsShowsThree,
            Check.RemovingItemEmptiesCart, Check.ProductLinkOpensThatProduct,
            Check.CheckoutFormKeepsEnteredValues, Check.CheckoutCompletes
        },
        ["error_user"] = new[]
        {
            Check.SortByPriceLowToHigh, Check.AddingThreeItemsShowsThree, Check.RemovingItemEmptiesCart,
            Check.CheckoutFormKeepsEnteredValues, Check.CheckoutCompletes
        },
        ["visual_user"] = new[] { Check.ProductImagesAreDistinct, Check.PricesMatchCatalog, Check.SortByPriceLowToHigh },
        ["performance_glitch_user"] = new[] { Check.LoginIsResponsive },
    };

    /// <summary>Catalog prices in the default Name (A to Z) order, as standard_user sees them.</summary>
    private static readonly decimal[] CatalogPrices = { 29.99m, 9.99m, 15.99m, 49.99m, 7.99m, 15.99m };

    private const int ResponsiveLoginMs = 3000;

    private static IEnumerable<TestCaseData> Contract()
    {
        foreach (var (user, defects) in KnownDefects)
        {
            foreach (var check in Enum.GetValues<Check>())
            {
                var testCase = new TestCaseData(user, check).SetName($"{check}({user})");
                if (defects.Contains(check))
                {
                    testCase.SetCategory("KnownDefect").SetDescription($"Planted defect: {user} breaks {check}");
                }
                yield return testCase;
            }
        }
    }

    private LoginPage _loginPage = null!;

    [SetUp]
    public void Setup()
    {
        BaseSetUp();
        _loginPage = new LoginPage(Page);
    }

    [TestCaseSource(nameof(Contract))]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task User_HonorsContract(string user, Check check)
    {
        await _loginPage.GotoAsync();
        var stopwatch = Stopwatch.StartNew();
        await _loginPage.SubmitCredentialsAsync(user, LoginCredentials.ValidUser.Password);
        await Expect(Page).ToHaveURLAsync(SauceDemoTestData.InventoryUrl, new() { Timeout = 15000 });
        await Expect(Page.Locator(".inventory_item").First).ToBeVisibleAsync(new() { Timeout = 15000 });
        stopwatch.Stop();

        switch (check)
        {
            case Check.ProductImagesAreDistinct:
                var sources = await Page.Locator(".inventory_item img").EvaluateAllAsync<string[]>("imgs => imgs.map(i => i.getAttribute('src'))");
                Assert.That(sources, Has.None.Contains("sl-404"), "No product should show the 404 placeholder image");
                Assert.That(sources, Is.Unique, "Each product should have its own image");
                break;

            case Check.PricesMatchCatalog:
                Assert.That(await ReadPricesAsync(), Is.EqualTo(CatalogPrices));
                break;

            case Check.SortByPriceLowToHigh:
                await Page.Locator("[data-test='product-sort-container']").SelectOptionAsync("lohi");
                Assert.That(await ReadPricesAsync(), Is.Ordered.Ascending);
                break;

            case Check.AddingThreeItemsShowsThree:
                await AddToCartAsync("sauce-labs-backpack", "sauce-labs-bike-light", "sauce-labs-bolt-t-shirt");
                await Expect(Page.Locator(".shopping_cart_badge")).ToHaveTextAsync("3");
                break;

            case Check.RemovingItemEmptiesCart:
                await AddToCartAsync("sauce-labs-backpack");
                await Page.Locator("[data-test='remove-sauce-labs-backpack']").ClickAsync(new() { Timeout = 5000 });
                await Expect(Page.Locator(".shopping_cart_badge")).ToHaveCountAsync(0);
                break;

            case Check.ProductLinkOpensThatProduct:
                await Page.Locator(".inventory_item_name", new() { HasText = SauceDemoTestData.Products.Backpack }).ClickAsync();
                // The URL changes before the list unmounts, and the list shares the
                // details page's data-test, so match the class only the details page uses.
                await Expect(Page).ToHaveURLAsync(new Regex(@"/inventory-item\.html\?id=\d+"));
                await Expect(Page.Locator(".inventory_details_name"))
                    .ToHaveTextAsync(SauceDemoTestData.Products.Backpack);
                break;

            case Check.CheckoutFormKeepsEnteredValues:
                await Page.GotoAsync(SauceDemoTestData.BaseUrl + "/checkout-step-one.html");
                await FillCheckoutInformationAsync();
                var entered = new[]
                {
                    await Page.InputValueAsync("#first-name"),
                    await Page.InputValueAsync("#last-name"),
                    await Page.InputValueAsync("#postal-code")
                };
                Assert.That(entered, Is.EqualTo(new[] { CheckoutInfo.Valid.FirstName, CheckoutInfo.Valid.LastName, CheckoutInfo.Valid.PostalCode }),
                    "First name, last name and postal code should keep what was typed");
                break;

            case Check.CheckoutCompletes:
                await AddToCartAsync("sauce-labs-onesie");
                await Page.Locator(".shopping_cart_link").ClickAsync();
                await Page.Locator("#checkout").ClickAsync();
                await FillCheckoutInformationAsync();
                await Page.Locator("#continue").ClickAsync();
                await Page.Locator("#finish").ClickAsync(new() { Timeout = 5000 });
                await Expect(Page).ToHaveURLAsync(SauceDemoTestData.BaseUrl + "/checkout-complete.html");
                break;

            case Check.LoginIsResponsive:
                Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(ResponsiveLoginMs), $"Login took {stopwatch.ElapsedMilliseconds}ms");
                break;
        }
    }

    private async Task<decimal[]> ReadPricesAsync() =>
        (await Page.Locator(".inventory_item_price").AllInnerTextsAsync())
            .Select(p => decimal.Parse(p.TrimStart('$'), CultureInfo.InvariantCulture))
            .ToArray();

    private async Task AddToCartAsync(params string[] productIds)
    {
        foreach (var id in productIds)
        {
            await Page.Locator($"[data-test='add-to-cart-{id}']").ClickAsync(new() { Timeout = 5000 });
        }
    }

    private async Task FillCheckoutInformationAsync()
    {
        // Type key by key, like a user, so input-handling bugs surface.
        await Page.Locator("#first-name").PressSequentiallyAsync(CheckoutInfo.Valid.FirstName);
        await Page.Locator("#last-name").PressSequentiallyAsync(CheckoutInfo.Valid.LastName);
        await Page.Locator("#postal-code").PressSequentiallyAsync(CheckoutInfo.Valid.PostalCode);
    }
}
