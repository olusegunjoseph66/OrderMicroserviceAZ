using AutoMapper;
using Azure.Identity;
using DinkToPdf;
using DinkToPdf.Contracts;
using KissLog;
using KissLog.AspNetCore;
using KissLog.CloudListeners.Auth;
using KissLog.CloudListeners.RequestLogsListener;
using Order.API.Extensions;
using Order.API.Middlewares;
using Order.Application.AutoMapperSettings;
using Order.Application.Interfaces.Messaging;
using Order.Infrastructure.Services.Messaging;
using Shared.Data.Contexts;
using Shared.ExternalServices.APIServices;
using Shared.ExternalServices.DTOs;
using Shared.ExternalServices.Interfaces;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

string myAllowSpecificOrigins = "_myAllowSpecificOrigins";
IConfiguration config = builder.Configuration;

var appConnectionString = builder.Configuration.GetConnectionString("AppConfig");
var useAppConfiguration = !string.IsNullOrWhiteSpace(appConnectionString);

if (useAppConfiguration)
{
    builder.Host.ConfigureAppConfiguration(cfg =>
    {
        cfg.AddAzureAppConfiguration(options =>
        {
            options.Connect(appConnectionString)
                .ConfigureRefresh(refresh =>
                {
                    refresh.Register("Test").SetCacheExpiration(
                        TimeSpan.FromSeconds(20));
                });

            if (Convert.ToBoolean(builder.Configuration["UsingAzureKeyVault"]))
                options.ConfigureKeyVault(kv =>
                {
                    kv.SetCredential(new DefaultAzureCredential());
                });
        });
    });

    builder.Services.AddAzureAppConfiguration();
}


builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.WriteIndented = true;
    options.JsonSerializerOptions.Converters.Add(new DecimalConverter());
});
builder.Services.AddHttpClient<ISapService, SapService>();
builder.Services.AddDbContext<ApplicationDbContext>();
builder.Services.AddSingleton(config);
IMapper mapper = MappingConfig.RegisterMaps().CreateMapper();
ServiceExtension.RegisterServices(builder.Services, config, myAllowSpecificOrigins);
builder.Services.AddScoped<IAzureServiceBusConsumer, AzureServiceBusConsumer>();
//builder.Services.AddScoped<IProcessMessage, ProcessMessage>();
builder.Services.AddEndpointsApiExplorer();

//var context = new CustomAssemblyLoadContext();
//context.LoadUnmanagedLibrary(Path.Combine(Directory.GetCurrentDirectory(), "libwkhtmltox.dll"));

builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
var urlSection = builder.Configuration.GetSection("ServiceUrls");
SD.SapAPIBase = urlSection["SapAPI"];
SD.RDATAAPIBase = urlSection["RDATA_BASE_URL"];
void ConfigureKissLog(IOptionsBuilder options)
{
    KissLogConfiguration.Listeners
        .Add(new RequestLogsApiListener(new Application(config["KissLog.OrganizationId"], config["KissLog.ApplicationId"]))
        {
            ApiUrl = config["KissLog.ApiUrl"]
        });
}

var app = builder.Build();

// Configure the HTTP request pipeline.

if (useAppConfiguration)
    app.UseAzureAppConfiguration();

app.Use((ctx, next) =>
{
    var headers = ctx.Response.Headers;

    headers.Add("X-Frame-Options", "DENY");
    headers.Add("X-XSS-Protection", "1; mode=block");
    headers.Add("X-Content-Type-Options", "nosniff");
    headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
    headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
    headers.Add("Pragma", "no-cache");

    headers.Remove("X-Powered-By");
    headers.Remove("x-aspnet-version");

    // Some headers won't remove
    headers.Remove("Server");

    return next();
});


app.UseAuthentication();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "OrderMicroservice v1");
    c.RoutePrefix = string.Empty;
});


app.UseMiddleware<ExceptionMiddleware>();
app.UseKissLogMiddleware(options => ConfigureKissLog(options));
app.UseHttpsRedirection();


app.UseRouting();
app.UseCors(myAllowSpecificOrigins);

app.UseAuthorization();

app.MapControllers();

app.UseAzureServiceBusConsume();

app.Run();
