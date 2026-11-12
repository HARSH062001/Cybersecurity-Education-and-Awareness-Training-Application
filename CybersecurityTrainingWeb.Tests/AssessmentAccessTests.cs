// Checks that signed-out visitors are directed to sign in before assessment access.
using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CybersecurityTrainingWeb.Tests;

public class AssessmentAccessTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AssessmentAccessTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData("/Assessment?moduleId=1")]
    [InlineData("/Results?attemptId=1")]
    [InlineData("/AssessmentHistory")]
    public async Task SignedOutLearner_IsRedirectedToLogin(string path)
    {
        var client = _factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                BaseAddress = new Uri("https://localhost")
            });

        var response = await client.GetAsync(path);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains(
            "/Identity/Account/Login",
            response.Headers.Location?.ToString());
    }
}