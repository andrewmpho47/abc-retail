using ABCRetail.Functions;
using ABCRetail.Functions.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.Configure<AzureStorageSettings>(
            context.Configuration.GetSection("AzureStorageSettings"));

        services.AddSingleton(sp =>
        {
            var settings = new AzureStorageSettings();
            sp.GetRequiredService<IConfiguration>()
                .GetSection("AzureStorageSettings")
                .Bind(settings);
            return settings;
        });

        services.AddScoped<StorageClientFactory>();
    })
    .Build();

host.Run();
