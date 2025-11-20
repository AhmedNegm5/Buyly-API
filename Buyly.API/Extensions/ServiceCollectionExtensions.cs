using System.Text;
using System.Text.Json;
using Buyly.API.Filters;
using Buyly.API.Models;
using Buyly.Application.Configurations;
using Buyly.Application.Constants;
using Buyly.Application.Interfaces;
using Buyly.Application.Services;
using Buyly.Domain.Entities;
using Buyly.Infrastructure.Configurations;
using Buyly.Infrastructure.Data;
using Buyly.Infrastructure.HostedServices;
using Buyly.Infrastructure.Repositories;
using Buyly.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PayPalCheckoutSdk.Core;
using System.Threading.RateLimiting;

namespace Buyly.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddControllersWithValidation(this IServiceCollection services)
        {
            services.AddControllers(options =>
            {
                options.Filters.Add<ValidationFilter>();
            }).AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
            });

            return services;
        }

        public static IServiceCollection AddConfiguredOptions(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<JwtSettings>()
                .Bind(configuration.GetSection("JwtSettings"))
                .ValidateDataAnnotations()
                .Validate(s => !string.IsNullOrWhiteSpace(s.SecretKey), "JwtSettings:SecretKey is required.")
                .ValidateOnStart();

            services.AddOptions<RateLimitSettings>()
                .Bind(configuration.GetSection("RateLimiting"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddOptions<ResetPasswordSettings>()
                .Bind(configuration.GetSection("ResetPasswordSettings"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddOptions<CleanupSettings>()
                .Bind(configuration.GetSection("CleanupSettings"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddOptions<PayPalSettings>()
                .Bind(configuration.GetSection("PayPal"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddOptions<EmailSettings>()
                .Bind(configuration.GetSection("EmailSettings"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            return services;
        }

        public static IServiceCollection AddRateLimitingPolicies(this IServiceCollection services, IConfiguration configuration)
        {
            var rateSettings = configuration.GetSection("RateLimiting").Get<RateLimitSettings>()
                ?? throw new InvalidOperationException("RateLimiting configuration section is missing.");

            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.ContentType = "application/json";
                    var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var span) ? span.ToString() : "60";
                    var payload = JsonSerializer.Serialize(new ApiResponse
                    {
                        Success = false,
                        Message = $"Rate limit exceeded. Try again in {retryAfter} seconds."
                    });
                    await context.HttpContext.Response.WriteAsync(payload, token);
                };
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
                {
                    var key = ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                    return RateLimitPartition.GetFixedWindowLimiter(
                        key,
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = rateSettings.GlobalLimit.PermitLimit,
                            Window = TimeSpan.FromSeconds(rateSettings.GlobalLimit.WindowSeconds),
                            QueueLimit = rateSettings.GlobalLimit.QueueLimit,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                        });
                });

                options.AddPolicy(RateLimitPolicies.Strict, ctx =>
                {
                    var key = ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                    return RateLimitPartition.GetFixedWindowLimiter(
                        key,
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = rateSettings.StrictLimit.PermitLimit,
                            Window = TimeSpan.FromSeconds(rateSettings.StrictLimit.WindowSeconds),
                            QueueLimit = rateSettings.StrictLimit.QueueLimit,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                        });
                });
            });

            return services;
        }

        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.EnableAnnotations();
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Buyly E-Commerce API",
                    Version = "v1",
                    Description = "API for shopping cart, checkout, orders, payments, and admin features. Use the two-step PayPal flow: create order -> redirect user -> capture.",
                    Contact = new Microsoft.OpenApi.Models.OpenApiContact
                    {
                        Name = "Buyly Support",
                        Email = "support@buyly.local"
                    }
                });

                c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT"
                });

                c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            return services;
        }

        public static IServiceCollection AddDatabaseAndIdentity(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not set in appsettings.json");

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            });

            services.AddIdentity<User, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            return services;
        }

        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>()
                ?? throw new InvalidOperationException("JwtSettings configuration section is missing.");

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = !environment.IsDevelopment();
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";
                        var response = ApiResponse.Unauthorized("You are not authorized to access this resource");
                        var jsonOptions = new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        };
                        var result = JsonSerializer.Serialize(response, jsonOptions);
                        return context.Response.WriteAsync(result);
                    },
                    OnForbidden = context =>
                    {
                        context.Response.StatusCode = 403;
                        context.Response.ContentType = "application/json";
                        var response = ApiResponse.Error("You do not have permission to access this resource");
                        var jsonOptions = new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        };
                        var result = JsonSerializer.Serialize(response, jsonOptions);
                        return context.Response.WriteAsync(result);
                    }
                };
            });

            return services;
        }

        public static IServiceCollection AddApplicationDependencies(this IServiceCollection services)
        {
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ICartRepository, CartRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IReviewRepository, ReviewRepository>();

            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICartService, CartService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IEmailSender, EmailSender>();

            services.AddSingleton<PayPalHttpClient>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<PayPalSettings>>().Value;
                PayPalEnvironment environment = settings.Environment.Equals("live", StringComparison.OrdinalIgnoreCase)
                    ? new LiveEnvironment(settings.ClientId, settings.ClientSecret)
                    : new SandboxEnvironment(settings.ClientId, settings.ClientSecret);
                return new PayPalHttpClient(environment);
            });

            services.AddHostedService<CartOrderCleanupService>();

            return services;
        }
    }
}

