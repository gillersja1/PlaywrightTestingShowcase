using Allure.Net.Commons;
using Allure.NUnit;
using Allure.NUnit.Attributes;

using NUnit.Framework;
using UI.Tests.Fixtures;
using UI.Tests.PageObjects;
using UI.Tests.TestData;

namespace UI.Tests.NeverDeliver.Checkout;

[TestFixture]
[AllureNUnit]
[AllureFeature("Checkout")]
[AllureSuite("NeverDeliver")]
public class NeverDeliverCheckoutTests : UITestFixtureBase
{
    private NeverDeliverCheckoutPage _checkoutPage = null!;

    [SetUp]
    public async Task Setup()
    {
        BaseSetUp();
        var shopPage = new NeverDeliverShopPage(Page);
        await shopPage.GotoAsync();
        await shopPage.AddProductToCartAsync(1);
        var cart = await shopPage.OpenCartAsync();
        _checkoutPage = await cart.ProceedToCheckoutAsync();
    }

    [Test]
    [AllureSeverity(SeverityLevel.blocker)]
    public async Task CompletingCheckout_ShowsConfirmationWithOrderNumber()
    {
        await _checkoutPage.FillAddressAsync(DeliveryAddress.Valid);
        await _checkoutPage.ContinueToPaymentAsync();
        await _checkoutPage.PlaceOrderAsync();

        Assert.That(await _checkoutPage.GetOrderNumberAsync(), Does.Match(NeverDeliverTestData.OrderNumberPattern.ToString()));
        await new NeverDeliverCartDrawer(Page).WaitForItemCountAsync(0);
    }

    [Test]
    [AllureSeverity(SeverityLevel.critical)]
    public async Task Confirmation_ShowsEnteredAddressWithSpecialCharacters()
    {
        var address = DeliveryAddress.WithSpecialChars;

        await _checkoutPage.FillAddressAsync(address);
        await _checkoutPage.ContinueToPaymentAsync();
        await _checkoutPage.PlaceOrderAsync();

        var deliveringTo = await _checkoutPage.GetDeliveringToAsync();
        Assert.Multiple(() =>
        {
            Assert.That(deliveringTo, Does.Contain(address.FullName));
            Assert.That(deliveringTo, Does.Contain(address.Street));
            Assert.That(deliveringTo, Does.Contain(address.City));
            Assert.That(deliveringTo, Does.Contain(address.Postcode));
        });
    }

    [TestCaseSource(nameof(RequiredAddressFields))]
    [AllureSeverity(SeverityLevel.critical)]
    public async Task ClearingRequiredField_DisablesContinueToPayment(string field)
    {
        Assert.That(await _checkoutPage.IsContinueToPaymentEnabledAsync(), Is.True, "Pre-filled form should be submittable");

        await _checkoutPage.ClearAddressFieldAsync(field);

        Assert.That(await _checkoutPage.IsContinueToPaymentEnabledAsync(), Is.False, $"Continue should be disabled without '{field}'");
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task NextDayDelivery_IncreasesOrderTotal()
    {
        var standardTotal = await _checkoutPage.GetOrderTotalAsync();

        await _checkoutPage.SelectNextDayDeliveryAsync();
        await _checkoutPage.WaitForOrderTotalToChangeFromAsync(standardTotal);

        Assert.That(await _checkoutPage.GetOrderTotalAsync(), Is.GreaterThan(standardTotal));
    }

    [Test]
    [Category("KnownDefect")]
    [AllureSeverity(SeverityLevel.minor)]
    [AllureIssue("ND-1")]
    [Description("Known defect ND-1: the Next Day Delivery surcharge is shown as '+$9.99' while the store currency is GBP.")]
    public async Task NextDayDeliveryPrice_UsesStoreCurrency()
    {
        var label = await _checkoutPage.GetNextDayDeliveryLabelAsync();

        Assert.That(label, Does.Contain("£").And.Not.Contain("$"));
    }

    [Test]
    [Category("KnownDefect")]
    [AllureSeverity(SeverityLevel.normal)]
    [AllureIssue("ND-2")]
    [Description("Known defect ND-2: address labels aren't associated with their inputs, so screen readers can't announce them (WCAG 1.3.1 / 4.1.2).")]
    public async Task AddressFields_HaveAccessibleLabels()
    {
        var labels = new[] { "Full name", "Street address", "Town/City", "County", "Postcode" };
        var matches = new Dictionary<string, int>();
        foreach (var label in labels)
        {
            matches[label] = await Page.GetByLabel(label, new() { Exact = true }).CountAsync();
        }

        Assert.Multiple(() =>
        {
            foreach (var label in labels)
            {
                Assert.That(matches[label], Is.EqualTo(1), $"'{label}' should label one input");
            }
        });
    }

    private static IEnumerable<string> RequiredAddressFields() => NeverDeliverTestData.AddressFields.All;
}
