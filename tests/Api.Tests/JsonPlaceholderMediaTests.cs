using Allure.Net.Commons;
using Allure.NUnit;
using Allure.NUnit.Attributes;
using Microsoft.Playwright;
using NUnit.Framework;

namespace Api.Tests;

/// <summary>
/// Tests for JSONPlaceholder /albums and /photos endpoints.
/// Demonstrates retrieving album and photo resources.
/// </summary>
[TestFixture]
[AllureNUnit]
[AllureFeature("JSONPlaceholder - Albums & Photos")]
public class JsonPlaceholderMediaTests
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
    public async Task GetAlbums_ReturnsMultipleAlbums()
    {
        var response = await _request.GetAsync("/albums");
        Assert.That(response.Ok, Is.True);

        var body = await response.JsonAsync();
        var albumCount = body!.Value.GetArrayLength();

        Assert.That(albumCount, Is.GreaterThan(0), "Should return multiple albums");
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task GetAlbum_ReturnsAlbumDetails()
    {
        var response = await _request.GetAsync("/albums/1");
        Assert.That(response.Ok, Is.True);

        var body = await response.JsonAsync();
        var albumId = body!.Value.GetProperty("id").GetInt32();
        var albumTitle = body.Value.GetProperty("title").GetString();

        Assert.That(albumId, Is.EqualTo(1));
        Assert.That(albumTitle, Is.Not.Empty);
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task GetAlbumsByUserId_ReturnsFilteredResults()
    {
        var response = await _request.GetAsync("/albums?userId=1");
        Assert.That(response.Ok, Is.True);

        var body = await response.JsonAsync();
        var albums = body!.Value.EnumerateArray();

        foreach (var album in albums)
        {
            var userId = album.GetProperty("userId").GetInt32();
            Assert.That(userId, Is.EqualTo(1), "All albums should belong to user ID 1");
        }
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task GetPhotos_ReturnsMultiplePhotos()
    {
        var response = await _request.GetAsync("/photos");
        Assert.That(response.Ok, Is.True);

        var body = await response.JsonAsync();
        var photoCount = body!.Value.GetArrayLength();

        Assert.That(photoCount, Is.GreaterThan(0), "Should return multiple photos");
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task GetPhoto_ReturnsPhotoDetails()
    {
        var response = await _request.GetAsync("/photos/1");
        Assert.That(response.Ok, Is.True);

        var body = await response.JsonAsync();
        var photoId = body!.Value.GetProperty("id").GetInt32();
        var photoUrl = body.Value.GetProperty("url").GetString();

        Assert.That(photoId, Is.EqualTo(1));
        Assert.That(photoUrl, Does.Contain("placeholder"));
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task GetPhotosByAlbumId_ReturnsFilteredResults()
    {
        var response = await _request.GetAsync("/photos?albumId=1");
        Assert.That(response.Ok, Is.True);

        var body = await response.JsonAsync();
        var photos = body!.Value.EnumerateArray();

        foreach (var photo in photos)
        {
            var albumId = photo.GetProperty("albumId").GetInt32();
            Assert.That(albumId, Is.EqualTo(1), "All photos should belong to album ID 1");
        }
    }
}
