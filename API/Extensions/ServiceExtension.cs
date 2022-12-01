using System.Text;
using API.Context;
using API.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace API.Extensions;

public static class ServiceExtension
{
    public static void ConfigureDatabaseContext(this IServiceCollection services)
    {
        services.AddDbContext<ApiDbContext>(options =>
            options
                .UseNpgsql("Name=ConnectionStrings:DefaultConnection")
                .UseSnakeCaseNamingConvention()
                .UseLoggerFactory(LoggerFactory.Create(loggerFactory => loggerFactory.AddConsole()))
                .EnableSensitiveDataLogging()
        );
        services.AddDatabaseDeveloperPageExceptionFilter();

        // run migrations
        using var serviceScope = services.BuildServiceProvider().CreateScope();
        using var context = serviceScope.ServiceProvider.GetService<ApiDbContext>();
        context?.Database.Migrate();
    }

    public static void ConfigureAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentity<User, IdentityRole>()
            .AddEntityFrameworkStores<ApiDbContext>();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(o =>
        {
            o.SaveToken = true;
            o.RequireHttpsMetadata = false;
            o.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ValidAudience = configuration["JWT:ValidAudience"],
                ValidIssuer = configuration["JWT:ValidIssuer"],
                IssuerSigningKey =
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"] ??
                                                                    throw new InvalidOperationException())),
                ClockSkew = TimeSpan.Zero
            };
            o.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        context.Response.Headers.Add("IS-TOKEN-EXPIRED", "true");
                    return Task.CompletedTask;
                }
            };
        });
        services.AddAuthorization();
    }

    public static void ConfigureSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Fun Quiz Api",
                Version = "v1",
                Description = "Fun Quiz Api Services.",
                Contact = new OpenApiContact
                {
                    Name = "ElXreno"
                }
            });
            c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme."
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
    }
}