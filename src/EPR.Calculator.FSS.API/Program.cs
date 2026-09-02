using System.Configuration;
using System.IO.Compression;
using System.Reflection;
using Azure.Core;
using Azure.Identity;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Azure.Storage.Blobs;
using EPR.Calculator.FSS.API.Configs;
using EPR.Calculator.FSS.API.Data;
using EPR.Calculator.FSS.API.HealthCheck;
using EPR.Calculator.FSS.API.Services;
using EPR.Calculator.FSS.API.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
    .AddEnvironmentVariables();

var applicationInsightsConnectionString =
    builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];

if (!string.IsNullOrWhiteSpace(applicationInsightsConnectionString))
{
    builder.Services
        .AddOpenTelemetry()
        .UseAzureMonitor();
}

builder.Services.AddProblemDetails();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();
builder.Services.AddScoped<IOrganisationService, OrganisationService>();

builder.Services.Configure<EprCalculatorApiSettings>(
    builder.Configuration.GetSection(EprCalculatorApiSettings.SectionName));

builder.Services.AddSingleton<TokenCredential>(
    builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("local")
        ? new AzureCliCredential()
        : new ManagedIdentityCredential(ManagedIdentityId.SystemAssigned));

builder.Services.AddTransient<EprCalculatorApiAuthHandler>();

builder.Services.AddHttpClient<IDownloadService, DownloadService>((serviceProvider, client) =>
{
    var settings = serviceProvider.GetRequiredService<IOptions<EprCalculatorApiSettings>>().Value;

    if (string.IsNullOrWhiteSpace(settings.BaseUrl))
    {
        throw new ConfigurationErrorsException("EprCalculatorApi base URL is not configured.");
    }

    client.BaseAddress = new Uri(settings.BaseUrl);
})
.AddHttpMessageHandler<EprCalculatorApiAuthHandler>();

builder.Services.AddHostedService<CalculatorApiStartupProbe>();

builder.Services.AddDbContext<SynapseDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("SynapseDatabase"));
});

builder.Services.Configure<BlobStorageSettings>(
    builder.Configuration.GetSection("BlobStorage"));

var blobStorageConnectionString =
    builder.Configuration["BlobStorage:ConnectionString"];

if (string.IsNullOrWhiteSpace(blobStorageConnectionString))
{
    throw new ConfigurationErrorsException(
        "Blob Storage connection string is not configured.");
}

builder.Services.AddSingleton(
    new BlobServiceClient(blobStorageConnectionString));

builder.Services.Configure<FeatureManagementSettings>(
    builder.Configuration.GetSection(FeatureManagementSettings.SectionName));

builder.Services.AddValidatorsFromAssemblyContaining<RunIdValidator>();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.SmallestSize;
});

builder.Services.AddRequestDecompression();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("local"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseResponseCompression();
app.UseRequestDecompression();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks(
    "/admin/health",
    HealthCheckOptionsBuilder.Build()).AllowAnonymous();

await app.RunAsync();
