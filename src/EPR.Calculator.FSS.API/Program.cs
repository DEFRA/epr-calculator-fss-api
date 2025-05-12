using Azure.Storage.Blobs;
using EPR.Calculator.API.Data;
using EPR.Calculator.FSS.API;
using EPR.Calculator.FSS.API.Common;
using EPR.Calculator.FSS.API.Constants;
using EPR.Calculator.FSS.API.HealthCheck;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);
var environmentName = builder.Environment.EnvironmentName?.ToLower() ?? string.Empty;

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddHealthChecks();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddScoped<IBillingService, BillingService>();
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();

// Configure the database context.
builder.Services.AddDbContext<ApplicationDBContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Configure blob storage settings.
builder.Services.Configure<BlobStorageSettings>(
    builder.Configuration.GetSection("BlobStorage"));

builder.Services.AddSingleton<BlobServiceClient>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetSection("BlobStorage:ConnectionString").Value;
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new ConfigurationErrorsException("Blob Storage connection string is not configured.");
    }

    return new BlobServiceClient(connectionString);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || environmentName == EPR.Calculator.FSS.API.Constants.Environment.Local.ToLower())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/admin/health", HealthCheckOptionsBuilder.Build()).AllowAnonymous();

app.Run();
