using Finstat.Application.Repositories;
using Finstat.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Finstat.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Finstat");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "Не задана строка подключения ConnectionStrings:Finstat (переменная окружения ConnectionStrings__Finstat).");

        services.AddDbContext<FinstatDbContext>(o => o.UseNpgsql(connectionString));
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IExpenseRepository, ExpenseRepository>();
        return services;
    }
}
