using System;

using ChangeGuard.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ChangeGuard.Application.ChangeRequests.Abstractions;
using ChangeGuard.Infrastructure.Persistence.Repositories;

namespace ChangeGuard.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString(
                "ChangeGuardDatabase");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'ChangeGuardDatabase' was not found.");
        }

        services.AddDbContext<ChangeGuardDbContext>(
            options =>
                options.UseSqlServer(
                    connectionString,
                    sqlOptions =>
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(10),
                            errorNumbersToAdd: null)));
        services.AddScoped<
            IChangeRequestRepository,
            ChangeRequestRepository>();
        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>(
                "database",
                tags: ["ready"]);

        return services;
    }
}
