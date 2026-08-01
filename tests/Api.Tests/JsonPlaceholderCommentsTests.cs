using Allure.Net.Commons;
using Allure.NUnit;
using Allure.NUnit.Attributes;
using Microsoft.Playwright;
using NUnit.Framework;

namespace Api.Tests;

/// <summary>
/// Tests for JSONPlaceholder /comments endpoint.
/// Demonstrates retrieving and creating comments.
/// </summary>
[TestFixture]
[AllureNUnit]
[AllureFeature("JSONPlaceholder - Comments")]
public class JsonPlaceholderCommentsTests
{
    private IPlaywright _playwright = null!;
    private IAPIRequestContext _request = null!;

    [OneTimeSetUp]
    public async Task GlobalSetup()
    {
        _playwright = await Playwright.CreateAsync();
        var contextOptions = new APIRequestNewContextOptions
        {
            BaseURL = "https://jsonplaceholder.typicode.com",
        };

        _request = await _playwright.APIRequest.NewContextAsync(contextOptions);
    }

    [OneTimeTearDown]
    public async Task GlobalTeardown()
    {
        await _request.DisposeAsync();
        _playwright.Dispose();
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task GetComments_ReturnsMultipleComments()
    {
        var response = await _request.GetAsync("/comments");
        Assert.That(response.Ok, Is.True);

        var body = await response.JsonAsync();
        var commentCount = body!.Value.GetArrayLength();

        Assert.That(commentCount, Is.GreaterThan(0), "Should return multiple comments");
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task GetCommentsByPostId_ReturnsFilteredResults()
    {
        var response = await _request.GetAsync("/comments?postId=1");
        Assert.That(response.Ok, Is.True);

        var body = await response.JsonAsync();
        var comments = body!.Value.EnumerateArray();

        foreach (var comment in comments)
        {
            var postId = comment.GetProperty("postId").GetInt32();
            Assert.That(postId, Is.EqualTo(1), "All comments should belong to post ID 1");
        }
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task CreateComment_ReturnsCreatedStatus()
    {
        var response = await _request.PostAsync("/comments", new APIRequestContextOptions
        {
            DataObject = new { postId = 1, name = "Test Comment", email = "test@example.com", body = "This is a test comment" }
        });

        Assert.That(response.Status, Is.EqualTo(201));

        var commentBody = await response.JsonAsync();
        Assert.That(commentBody!.Value.GetProperty("name").GetString(), Is.EqualTo("Test Comment"));
    }
}
