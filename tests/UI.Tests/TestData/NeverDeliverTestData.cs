using System.Globalization;
using System.Text.RegularExpressions;

namespace UI.Tests.TestData;

/// <summary>
/// Delivery address used on the never-deliver-shop.vercel.app checkout form.
/// </summary>
public record DeliveryAddress(string FullName, string Street, string City, string County, string Postcode)
{
    public static DeliveryAddress Valid => new("Jane Tester", "1 Test Lane", "Lansing", "Michigan", "48933");
    public static DeliveryAddress WithSpecialChars => new("Siobhán O'Brien-Smith", "12 Rue de l'Église", "Saint-Étienne", "Loire", "42000");
}

public static class NeverDeliverTestData
{
    public const string BaseUrl = "https://never-deliver-shop.vercel.app";
    public const string CheckoutUrl = BaseUrl + "/checkout";
    public const string OrdersUrl = BaseUrl + "/orders";

    /// <summary>
    /// Checkout address fields, keyed by the sample value the form shows as
    /// both its placeholder and its pre-filled value.
    /// </summary>
    public static class AddressFields
    {
        public const string FullName = "Amelia Carter";
        public const string Street = "221B Baker Street";
        public const string City = "London";
        public const string County = "Greater London";
        public const string Postcode = "NW1 6XE";

        public static IEnumerable<string> All => new[] { FullName, Street, City, County, Postcode };
    }

    public const string OrderConfirmedHeading = "Order Confirmed!";
    public static readonly Regex OrderNumberPattern = new(@"NDV-[A-Z0-9]+-[A-Z0-9]+");

    /// <summary>
    /// Parses the first GBP amount in a string, e.g. "£650,004.00" -> 650004.00.
    /// </summary>
    public static decimal ParsePounds(string text)
    {
        var match = Regex.Match(text, @"£([\d,]+\.\d{2})");
        if (!match.Success)
        {
            throw new FormatException($"No £ amount found in \"{text}\"");
        }
        return decimal.Parse(match.Groups[1].Value, NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
    }
}
