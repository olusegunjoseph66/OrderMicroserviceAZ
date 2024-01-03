﻿using Account.Application.Interfaces.Services;
using Account.Infrastructure.Services;
using KissLog;
using KissLog.AspNetCore;
using KissLog.Formatters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Order.API.ClientServices;
using Order.API.Filters;
using Order.Application.Configurations;
using Order.Application.Configurations.Security;
using Order.Application.Interfaces.Services;
using Order.Application.SerilogService;
using Order.Infrastructure.Services;
using Shared.Data.Contexts;
using Shared.Data.Repository;
using Shared.Data.UnitOfWork;
using Shared.ExternalServices.APIServices;
using Shared.ExternalServices.Configurations;
using Shared.ExternalServices.Interfaces;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Order.API.Extensions
{
    public static class ServiceExtension
    {
        public static void RegisterServices(this IServiceCollection services, IConfiguration config, string allowedSpecificOrigins)
        {
            services.AddWebCoreServices(config, allowedSpecificOrigins);
            services.AddSwaggerExtension();
            services.AddSharedInfrastructure(config);
        }

        private static void AddWebCoreServices(this IServiceCollection services, IConfiguration config, string allowedSpecificOrigins)
        {
            services.AddSingleton<RequestValidationAttributeFilter>();
            services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);
            //Limit model validation error to return just one error message that first occurred
            services.Configure<MvcOptions>(options =>
            {
                options.Filters.AddService<RequestValidationAttributeFilter>();
            });
            services.AddAntiforgery();
            services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                options.JsonSerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
                options.JsonSerializerOptions.IncludeFields = true;
                options.JsonSerializerOptions.AllowTrailingCommas = false;
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.WriteIndented = true;
                options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddHttpContextAccessor();
            services.AddMemoryCache();
            services.AddRouting(options => options.LowercaseUrls = true);
            services.AddCors(options =>
            {
                options.AddPolicy(allowedSpecificOrigins, builder =>
                {
                    builder.WithOrigins("SAMEORIGIN")
                    .AllowAnyMethod()
                    .AllowAnyHeader();
                });
            });
            //Web Api services needed
            string connectionString = config.GetConnectionString("DMSConnection");
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            services.AddStackExchangeRedisCache(setupAction =>
            {
                setupAction.Configuration = config.GetConnectionString("RedisCacheUrl");
            });

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IAsyncRepository<>), typeof(BaseRepository<>));

            services.AddTransient<IFileService, FileService>();
            services.AddTransient<IQueueMessagingService, QueueMessagingService>();
            services.AddScoped<IAuthenticatedUserService, AuthenticatedUserService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IOtpService, OtpService>();
            services.AddScoped<ISapService, SapService>();
            services.AddScoped<ICartService, CartService>();
            services.AddScoped<IPlantService, PlantService>();
            services.AddTransient<ICachingService, CachingService>();
            services.AddTransient<IDeadLetterService, DeadLetterService>();
            services.AddSingleton<OrderLogger>();


            services.AddScoped<IKLogger>((provider) => Logger.Factory.Get());
            services.AddLogging(logging =>
            {
                logging.AddKissLog(options =>
                {
                    options.Formatter = (args) =>
                    {
                        if (args.Exception == null)
                            return args.DefaultValue;

                        string exceptionStr = new ExceptionFormatter().Format(args.Exception, args.Logger);
                        return string.Join(Environment.NewLine, new[] { args.DefaultValue, exceptionStr });
                    };
                });
            });


        }

        private static void AddSwaggerExtension(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                //  c.IncludeXmlComments(string.Format(@"{0}Account.API.xml", System.AppDomain.CurrentDomain.BaseDirectory));
                c.DocumentFilter<CustomSwaggerFilter>();
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = $"v1 - Build:-{Guid.NewGuid().ToString("N").Substring(22)}",
                    Title = "Order Microservice - WebApi",
                    Description = "This Api will be responsible for overall authentication and authorization.",
                    Contact = new OpenApiContact
                    {
                        Name = "DMS - Order Microservice",
                        Email = "dev@verraki.com",
                        Url = new Uri("https://verraki.com/contact"),
                    }
                });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    Description = "Input your Bearer token in this format - Bearer {your token here} to access this API",
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer",
                            },
                            Scheme = "Bearer",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        }, new List<string>()
                    },
                });
            });
        }

        private static IServiceCollection AddSharedInfrastructure(this IServiceCollection services, IConfiguration config)
        {

            services.Configure<JwtSettings>(config.GetSection(nameof(JwtSettings)));
            services.Configure<SerilogConfiguration>(config.GetSection(nameof(SerilogConfiguration)));
            services.Configure<CacheServiceSetting>(config.GetSection(nameof(CacheServiceSetting)));
            services.Configure<MessagingServiceSetting>(config.GetSection(nameof(MessagingServiceSetting)));
            services.Configure<OtpSettings>(config.GetSection(nameof(OtpSettings)));


            services.AddAuthentication(sharedOptions =>
            {
                sharedOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                sharedOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                sharedOptions.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(cfg =>
            {
                cfg.RequireHttpsMetadata = false;

                cfg.SaveToken = true;

                cfg.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = config["JwtSettings:Issuer"],
                    ValidAudience = config["JwtSettings:Audience"],
                    IssuerSigningKey =
                        new SymmetricSecurityKey(
                            Encoding.ASCII.GetBytes(config["JwtSettings:SecretKey"]))
                };
                cfg.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine("OnAuthenticationFailed: " + context.Exception.Message);

                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        Console.WriteLine("OnTokenValidated: " + context.SecurityToken);
                        return Task.CompletedTask;
                    }
                };
            });
            return services;
        }
    }
}
