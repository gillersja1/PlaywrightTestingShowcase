using Allure.Net.Commons;
using Allure.NUnit;
using Allure.NUnit.Attributes;
using Microsoft.Playwright;
using NUnit.Framework;
using System.Diagnostics;
using UI.Tests.Fixtures;
using UI.Tests.PageObjects;
using UI.Tests.TestData;

namespace UI.Tests.SauceDemo.Performance;

[TestFixture]
[AllureNUnit]
[AllureFeature("Performance")]
[AllureSuite("SauceDemo")]
public class SauceDemoPerformanceTests : UITestFixtureBase
{
    private LoginPage _loginPage = null!;
    private InventoryPage _inventoryPage = null!;
    private CartPage _cartPage = null!;

    [SetUp]
    public void Setup()
    {
        BaseSetUp();
        _loginPage = new LoginPage(Page);
        _inventoryPage = new InventoryPage(Page);
        _cartPage = new CartPage(Page);
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task LoginPageLoadsWithinAcceptableTime()
    {
        var stopwatch = Stopwatch.StartNew();
        await _loginPage.GotoAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        stopwatch.Stop();

        TestContext.WriteLine($"Login page load time: {stopwatch.ElapsedMilliseconds}ms");
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(5000), "Login page should load within 5 seconds");
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task InventoryPageLoadsWithinAcceptableTime()
    {
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync(LoginCredentials.ValidUser.Username, LoginCredentials.ValidUser.Password);

        var stopwatch = Stopwatch.StartNew();
        await Page.ReloadAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        stopwatch.Stop();

        TestContext.WriteLine($"Inventory page load time: {stopwatch.ElapsedMilliseconds}ms");
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(5000));
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task AddToCartResponseTimeIsAcceptable()
    {
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync(LoginCredentials.ValidUser.Username, LoginCredentials.ValidUser.Password);

        var stopwatch = Stopwatch.StartNew();
        await _inventoryPage.AddItemToCartAsync(SauceDemoTestData.Products.Backpack);
        stopwatch.Stop();

        TestContext.WriteLine($"Add to cart response time: {stopwatch.ElapsedMilliseconds}ms");
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(2000));
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task CartPageLoadsWithinAcceptableTime()
    {
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync(LoginCredentials.ValidUser.Username, LoginCredentials.ValidUser.Password);
        await _inventoryPage.AddItemToCartAsync(SauceDemoTestData.Products.Backpack);

        var stopwatch = Stopwatch.StartNew();
        await _inventoryPage.GoToCartAsync();
        stopwatch.Stop();

        TestContext.WriteLine($"Cart page load time: {stopwatch.ElapsedMilliseconds}ms");
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(3000));
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task ProductsLoadWithImages()
    {
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync(LoginCredentials.ValidUser.Username, LoginCredentials.ValidUser.Password);

        var stopwatch = Stopwatch.StartNew();
        var displayed = await _inventoryPage.AreAllProductsDisplayedAsync();
        stopwatch.Stop();

        TestContext.WriteLine($"Product visibility check time: {stopwatch.ElapsedMilliseconds}ms");
        Assert.That(displayed, Is.True);
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(3000));
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task CheckoutPageLoadsWithinAcceptableTime()
    {
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync(LoginCredentials.ValidUser.Username, LoginCredentials.ValidUser.Password);
        await _inventoryPage.AddItemToCartAsync(SauceDemoTestData.Products.Backpack);
        await _inventoryPage.GoToCartAsync();

        var stopwatch = Stopwatch.StartNew();
        await _cartPage.ClickCheckoutAsync();
        stopwatch.Stop();

        TestContext.WriteLine($"Checkout page load time: {stopwatch.ElapsedMilliseconds}ms");
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(3000));
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task MultipleAddToCartOperationsAreConsistent()
    {
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync(LoginCredentials.ValidUser.Username, LoginCredentials.ValidUser.Password);

        var times = new List<long>();

        foreach (var product in SauceDemoTestData.Products.All.Take(3))
        {
            var stopwatch = Stopwatch.StartNew();
            await _inventoryPage.AddItemToCartAsync(product);
            stopwatch.Stop();
            times.Add(stopwatch.ElapsedMilliseconds);
        }

        // All operations should complete within similar timeframe
        var average = times.Average();
        var maxTime = times.Max();

        TestContext.WriteLine($"Average add to cart time: {average}ms, Max: {maxTime}ms");
        Assert.That(maxTime, Is.LessThan(average + 1000), "Add to cart performance should be consistent");
    }
}
