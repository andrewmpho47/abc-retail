using ABCRetail.Middleware;
using ABCRetail.Models;
using ABCRetail.Services;
using ABCRetail.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add health checks
builder.Services.AddHealthChecks();

// Configure Azure Storage Settings
builder.Services.Configure<AzureStorageSettings>(
    builder.Configuration.GetSection("AzureStorageSettings"));

// Register Azure Storage Settings as a singleton for direct injection
builder.Services.AddSingleton(sp =>
{
    var settings = new AzureStorageSettings();
    builder.Configuration.GetSection("AzureStorageSettings").Bind(settings);
    return settings;
});

// Register storage services
builder.Services.AddScoped<ITableStorageService, TableStorageService>();
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();
builder.Services.AddScoped<IQueueStorageService, QueueStorageService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IStorageErrorLogger, StorageErrorLogger>();

var app = builder.Build();

// Add global exception handling middleware first in the pipeline
app.UseGlobalExceptionHandler();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Map health check endpoint for Azure App Service
app.MapHealthChecks("/health");

app.Run();
