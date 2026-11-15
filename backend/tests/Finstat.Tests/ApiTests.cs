using System.Net;
using Finstat.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Finstat.Tests;

public class ApiTests(ApiFixture api) : IClassFixture<ApiFixture>
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Startup_applies_migrations()
    {
        using var client = api.CreateClient(); // запускает хост, а с ним миграции
        await using var scope = api.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<FinstatDbContext>().Database;

        Assert.Empty(await db.GetPendingMigrationsAsync(Ct));
        Assert.Contains(await db.GetAppliedMigrationsAsync(Ct), m => m.EndsWith("InitialCreate"));
    }

    [Fact]
    public async Task Health_is_ok()
    {
        using var client = api.CreateClient();
        var response = await client.GetAsync("/health", Ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Unknown_route_returns_problem_details_404()
    {
        using var client = api.CreateClient();
        var response = await client.GetAsync("/nope", Ct);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task DomainException_returns_problem_details_400()
    {
        using var client = api.CreateClient();
        var response = await client.GetAsync("/test/domain-error", Ct);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("domain-rule-message", await response.Content.ReadAsStringAsync(Ct));
    }

    [Fact]
    public async Task Unhandled_exception_returns_500_without_details()
    {
        using var client = api.CreateClient();
        var response = await client.GetAsync("/test/crash", Ct);
        var body = await response.Content.ReadAsStringAsync(Ct);
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.DoesNotContain("internal-secret-message", body);
        Assert.DoesNotContain("InvalidOperationException", body);
    }
}
