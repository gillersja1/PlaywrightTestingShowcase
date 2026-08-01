using Allure.Net.Commons;
using Allure.NUnit;
using Allure.NUnit.Attributes;
using Microsoft.Playwright;
using NUnit.Framework;

namespace Api.Tests;

/// <summary>
/// Tests for JSONPlaceholder /todos endpoint.
/// Demonstrates retrieving and filtering todos.
/// </summary>
[TestFixture]
[AllureNUnit]
[AllureFeature("JSONPlaceholder - Todos")]
public class JsonPlaceholderTodosTests
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
    public async Task GetTodos_ReturnsMultipleTodos()
    {
        var response = await _request.GetAsync("/todos");
        Assert.That(response.Ok, Is.True);

        var body = await response.JsonAsync();
        var todoCount = body!.Value.GetArrayLength();

        Assert.That(todoCount, Is.GreaterThan(0), "Should return multiple todos");
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task GetTodosByUserId_ReturnsFilteredResults()
    {
        var response = await _request.GetAsync("/todos?userId=1");
        Assert.That(response.Ok, Is.True);

        var body = await response.JsonAsync();
        var todos = body!.Value.EnumerateArray();

        foreach (var todo in todos)
        {
            var userId = todo.GetProperty("userId").GetInt32();
            Assert.That(userId, Is.EqualTo(1), "All todos should belong to user ID 1");
        }
    }

    [Test]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task GetTodo_ReturnsExpectedFields()
    {
        var response = await _request.GetAsync("/todos/1");
        Assert.That(response.Ok, Is.True);

        var body = await response.JsonAsync();
        var todo = body!.Value;

        Assert.That(todo.TryGetProperty("id", out _), Is.True, "Should have id");
        Assert.That(todo.TryGetProperty("userId", out _), Is.True, "Should have userId");
        Assert.That(todo.TryGetProperty("title", out _), Is.True, "Should have title");
        Assert.That(todo.TryGetProperty("completed", out _), Is.True, "Should have completed status");
    }
}
