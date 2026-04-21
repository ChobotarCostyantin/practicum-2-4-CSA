using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nimble.Modulith.Reporting.Data;
using Nimble.Modulith.Reporting.Services;

namespace Nimble.Modulith.Reporting;

public static class ReportingModuleExtensions
{
    public static IHostApplicationBuilder AddReportingModuleServices(this IHostApplicationBuilder builder, Serilog.ILogger logger)
    {
        builder.AddSqlServerDbContext<ReportingDbContext>("reportingdb");
        builder.Services.AddScoped<IReportService, ReportService>();
        return builder;
    }

    public static async Task<WebApplication> EnsureReportingModuleDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ReportingDbContext>();
        var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();

        if (!env.IsDevelopment()) return app;
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        return app;
    }
}