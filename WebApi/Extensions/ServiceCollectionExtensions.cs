using Application.Contracts;
using Application.Services;
using Commons;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Infrastructure.Repositories;
using Domain;
using Application.HostedServices;
using Commons.Classes;
using Models;
using Infrastructure.DataAccess;
using Application.Mapper;
using Newtonsoft.Json;

namespace WebApi.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static readonly ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddDebug());
        public static void ConfigureSqlContext(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<ExpenceManagementSystemContext>(o => o
            .UseLazyLoadingProxies(false)
            .UseSqlServer()
#if DEBUG
            .UseLoggerFactory(loggerFactory)
#endif
            , ServiceLifetime.Scoped);
        }
        public static void ConfigureCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder
                    .WithOrigins("http://localhost:4200", "https://expence-managment-system.azurewebsites.net")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                     .AllowCredentials());
            });
        }

        public static void ConfigureHttpContext(this IServiceCollection services, IConfiguration config)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<IRepository, Repository>();

            //services.AddScoped<IEMSContextService, EMSContextService>();
            services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();
        }

        public static void ConfigureRepository(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<IRepository, Repository>();
            //services.AddScoped<IRepository, Repository>();
        }

        public static void ConfigureBusinessServices(this IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddSingleton<ICacheService, CacheService>(); // Register as singleton
            services.AddScoped<IBlobService, BlobService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<ICardHolderService, CardHolderService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<IFixedAllocationService, FixedAllocationService>();
            services.AddScoped<IDashboardService, DashboardService>();

            //services.AddHostedService<DemoHostedService>();
            services.AddAutoMapper(typeof(MappingProfile));

        }

        public static void ConfigureSwagger(this IServiceCollection services)
        {
            services.AddApiVersioning(o =>
            {
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
                o.ReportApiVersions = true;
            });

            services.AddVersionedApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            services.AddSwaggerGen(option =>
            {
                //option.SwaggerDoc("v1", new OpenApiInfo { Title = "Customer Portal API", Version = "v1" });
                option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });
                option.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type=ReferenceType.SecurityScheme,
                                Id="Bearer"
                            }
                        },
                        new string[]{}
                    }
                });
            });

            services.ConfigureOptions<ConfigureSwaggerOptions>();
        }

        public static void CongigureAppSettings(this IServiceCollection services, IConfiguration configuration)
        {
            configuration.GetSection(nameof(AppSettings)).Get<AppSettings>();
        }

        public static void ConfigureAuthentication(this IServiceCollection services)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                {
                    ValidateIssuerSigningKey = AppSettings.Current.JwtSettings.ValidateIssuerSigningKey,
                    IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(AppSettings.Current.JwtSettings.IssuerSigningKey)),
                    ValidateIssuer = AppSettings.Current.JwtSettings.ValidateIssuer,
                    ValidIssuer = AppSettings.Current.JwtSettings.ValidIssuer,
                    ValidateAudience = AppSettings.Current.JwtSettings.ValidateAudience,
                    ValidAudience = AppSettings.Current.JwtSettings.ValidAudience,
                    RequireExpirationTime = AppSettings.Current.JwtSettings.RequireExpirationTime,
                    //ValidateLifetime = AppSettings.Current.JwtSettings.RequireExpirationTime,
                    //ClockSkew = TimeSpan.FromDays(1),
                };
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";
                        APIResponse response = new APIResponse
                        {
                            Errors = new List<string>()
                            {
                                "Unauthorized: No valid token provided"
                            },
                            Status = System.Net.HttpStatusCode.Unauthorized
                        };
                        return context.Response.WriteAsync(JsonConvert.SerializeObject(response));
                    }
                };
            });

            //services.AddAuthorization();
            services.AddAuthentication("Bearer");
        }

    }
}
