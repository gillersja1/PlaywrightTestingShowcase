using Allure.Net.Commons;
using Allure.NUnit;
using Allure.NUnit.Attributes;
using Microsoft.Playwright;
using NUnit.Framework;

namespace Api.Tests;

/// <summary>
/// Tests for JSONPlaceholder /posts endpoint.
/// Demonstrates CRUD operations on blog posts.
/// </summary>
[TestFixture]
[AllureNUnit]
[AllureFeature("JSONPlaceholder - Posts")]
public class JsonPlaceholderPostsTests
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
    [AllureSeverity(SeverityLevel.critical)]
    public async Task GetPost_ReturnsExpectedPost()
    {
        var response = await _request.GetAsync("/posts/1");
        Assert.That(response.Ok, Is.True);

        var body = await response.JsonAsync();
        var postId = body!.Value.GetProperty("id").GetInt32();
        var userId = body.Value.GetProperty("userId").GetInt32();

        Assert.That(postId, Is.EqualTo(1));
        Assert.That(userId, Is.GreaterThan(0));
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task GetAllPosts_ReturnsMultiplePosts()
    {
        var response = await _request.GetAsync("/posts");
        Assert.That(response.Ok, Is.True);

        var body = await response.JsonAsync();
        var postCount = body!.Value.GetArrayLength();

        Assert.That(postCount, Is.GreaterThan(0), "Should return multiple posts");
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task GetPostsByUserId_ReturnsFilteredResults()
    {
        var response = await _request.GetAsync("/posts?userId=1");
        Assert.That(response.Ok, Is.True);

        var body = await response.JsonAsync();
        var posts = body!.Value.EnumerateArray();

        foreach (var post in posts)
        {
            var userId = post.GetProperty("userId").GetInt32();
            Assert.That(userId, Is.EqualTo(1), "All posts should belong to user ID 1");
        }
    }

    [Test]
    [AllureSeverity(SeverityLevel.critical)]
    public async Task CreatePost_ReturnsCreatedWithId()
    {
        var response = await _request.PostAsync("/posts", new APIRequestContextOptions
        {
            DataObject = new { title = "Test Post", body = "Test Body", userId = 1 }
        });

        Assert.That(response.Status, Is.EqualTo(201), "POST should return 201 Created");

        var postBody = await response.JsonAsync();
        var id = postBody!.Value.GetProperty("id").GetInt32();

        Assert.That(id, Is.GreaterThan(100), "New post should have an ID greater than 100");
        Assert.That(postBody.Value.GetProperty("title").GetString(), Is.EqualTo("Test Post"));
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task UpdatePost_ReturnsUpdatedData()
    {
        var response = await _request.PutAsync("/posts/1", new APIRequestContextOptions
        {
            DataObject = new { title = "Updated Title", body = "Updated Body", userId = 1, id = 1 }
        });

        Assert.That(response.Ok, Is.True);

        var postBody = await response.JsonAsync();
        Assert.That(postBody!.Value.GetProperty("title").GetString(), Is.EqualTo("Updated Title"));
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task PatchPost_ReturnsPartialUpdate()
    {
        var response = await _request.PatchAsync("/posts/1", new APIRequestContextOptions
        {
            DataObject = new { title = "Patched Title" }
        });

        Assert.That(response.Ok, Is.True);

        var postBody = await response.JsonAsync();
        Assert.That(postBody!.Value.GetProperty("title").GetString(), Is.EqualTo("Patched Title"));
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task DeletePost_Returns200()
    {
        var response = await _request.DeleteAsync("/posts/1");
        Assert.That(response.Ok, Is.True, "DELETE should return success status");
    }

    [Test]
    [AllureSeverity(SeverityLevel.minor)]
    public async Task GetNonExistentPost_Returns404()
    {
        var response = await _request.GetAsync("/posts/99999");
        Assert.That(response.Status, Is.EqualTo(404));
    }
}
