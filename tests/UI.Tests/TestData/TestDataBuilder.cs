namespace UI.Tests.TestData;

/// <summary>
/// Test credentials for SauceDemo.
/// </summary>
public record LoginCredentials(string Username, string Password, string? ExpectedError = null)
{
    public static LoginCredentials ValidUser => new("standard_user", "secret_sauce");
    public static LoginCredentials ProblemUser => new("problem_user", "secret_sauce");
    public static LoginCredentials LockedOutUser => new("locked_out_user", "secret_sauce", "locked out");

    public static IEnumerable<LoginCredentials> InvalidInputs => new[]
    {
        new LoginCredentials("", "secret_sauce", "Username"),
        new LoginCredentials("standard_user", "", "Password"),
        new LoginCredentials("", "", "required"),
    };

    public static IEnumerable<LoginCredentials> BoundaryInputs => new[]
    {
        new LoginCredentials(new string('a', 1000), "secret_sauce", null), // Very long username
        new LoginCredentials("user@#$%", "secret_sauce", null), // Special chars
        new LoginCredentials("user with spaces", "secret_sauce", null),
    };
}

/// <summary>
/// Test data for checkout information.
/// </summary>
public record CheckoutInfo(string FirstName, string LastName, string PostalCode)
{
    public static CheckoutInfo Valid => new("John", "Doe", "12345");
    public static CheckoutInfo WithSpecialChars => new("Jean-Claude", "O'Neill", "12345-6789");
    public static CheckoutInfo Long => new("VeryLongFirstNameHere", "VeryLongLastNameHere", "123456789");

    public static IEnumerable<CheckoutInfo> InvalidInputs => new[]
    {
        new CheckoutInfo("", "Doe", "12345"),
        new CheckoutInfo("John", "", "12345"),
        new CheckoutInfo("John", "Doe", ""),
        new CheckoutInfo("", "", ""),
    };

    public static IEnumerable<(CheckoutInfo Data, string ValidationType)> ValidationScenarios => new[]
    {
        (new CheckoutInfo("", "Doe", "12345"), "FirstName"),
        (new CheckoutInfo("John", "", "12345"), "LastName"),
        (new CheckoutInfo("John", "Doe", ""), "PostalCode"),
        (new CheckoutInfo("a", "b", "1"), "MinLength"), // Very short valid
        (new CheckoutInfo("Jean-Claude", "O'Neill", "12345"), "SpecialChars"),
    };
}

/// <summary>
/// Common test data constants.
/// </summary>
public static class SauceDemoTestData
{
    public const string BaseUrl = "https://www.saucedemo.com";
    public const string LoginUrl = BaseUrl + "/";
    public const string InventoryUrl = BaseUrl + "/inventory.html";
    public const string CartUrl = BaseUrl + "/cart.html";

    public static class Products
    {
        public const string Backpack = "Sauce Labs Backpack";
        public const string BikeLight = "Sauce Labs Bike Light";
        public const string TShirt = "Sauce Labs Bolt T-Shirt";
        public const string Jacket = "Sauce Labs Fleece Jacket";
        public const string Onesie = "Sauce Labs Onesie";
        public const string AllThingsSauceLabsTShirt = "Test.allTheThings() T-Shirt (Red)";

        public static IReadOnlyList<string> All => new[]
        {
            Backpack, BikeLight, TShirt, Jacket, Onesie, AllThingsSauceLabsTShirt
        };
    }

    public static class ErrorMessages
    {
        public const string LockedOut = "locked out";
        public const string InvalidCredentials = "do not match";
        public const string UsernameRequired = "Username";
        public const string PasswordRequired = "Password";
    }
}
