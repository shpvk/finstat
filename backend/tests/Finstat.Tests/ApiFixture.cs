using Finstat.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace Finstat.Tests;

/// <summary>API на свежей PostgreSQL в контейнере; один контейнер на класс тестов.</summary>
public sealed class ApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _db = new PostgreSqlBuilder("postgres:17-alpine").Build();

    public async ValueTask InitializeAsync() => await _db.StartAsync();

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _db.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Finstat", _db.GetConnectionString());
        builder.ConfigureServices(s => s.AddTransient<IStartupFilter, TestEndpoints>());
    }

    // Тестовые эндпоинты для проверки обработки ошибок; в продакшн-коде их нет.
    private sealed class TestEndpoints : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
        {
            next(app);
            app.UseRouting();
            app.UseEndpoints(routes =>
            {
                routes.MapGet("/test/domain-error", () => { throw new DomainException("domain-rule-message"); });
                routes.MapGet("/test/crash", () => { throw new InvalidOperationException("internal-secret-message"); });
            });
        };
    }
}
