// Copyright (c) Ahmed Mohamed Abdel Fattah
using Domain.Contracts;
using Microsoft.EntityFrameworkCore;
using Presistence;
using Presistence.Repositories;
using AYA_UIS.MiddelWares;
using Microsoft.AspNetCore.Mvc;
using AYA_UIS.Factories;
using Microsoft.AspNetCore.Identity;
using Shared.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Threading.RateLimiting;
using AYA_UIS.Core.Domain.Entities.Identity;
using AYA_UIS.Core.Services.Implementations;
using AYA_UIS.Application.Mapping;
using Infrastructure.Services;
using System.Text.Json.Serialization;
using Services.Implementatios;
using AYA_UIS.Application.Contracts;
using AYA_UIS.Core.Abstractions.Contracts;
using AYA_UIS.Core.Domain.Contracts;
using Abstraction.Contracts;
using AYA_UIS.API.Filters;

namespace AYA_UIS.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins(
                              "http://localhost:3000",
                              "http://localhost:5173",
                              "https://localhost:5173"
                          )
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            builder.Services.AddControllers()
                .AddApplicationPart(
                    typeof(Presentation.Controllers.AuthenticationController).Assembly)
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    options.JsonSerializerOptions.DefaultIgnoreCondition =
                        JsonIgnoreCondition.WhenWritingNull;
                });

            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(option =>
            {
                option.SwaggerDoc("v1", new OpenApiInfo { Title = "AYA UIS API", Version = "v1" });
                option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Enter: Bearer {token}",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });
                option.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        Array.Empty<string>()
                    }
                });

                // Handle complex types and generic dictionaries
                option.UseInlineDefinitionsForEnums();

                // Use fully qualified names to avoid conflicts
                option.CustomSchemaIds(type => type.FullName);

                // Try-catch to handle schema generation errors gracefully
                option.SchemaFilter<EnumSchemaFilter>();
            });

            builder.Services.AddDbContext<UniversityDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    sql => sql.CommandTimeout(60))
            );

            builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("JwtOptions"));
            var jwtOptions = builder.Configuration.GetSection("JwtOptions").Get<JwtOptions>()
                ?? throw new InvalidOperationException("JwtOptions missing in appsettings.json");

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var publicKey = File.ReadAllText("Keys/public_key.pem");
                var rsa = RSA.Create();
                rsa.ImportFromPem(publicKey);

                options.SaveToken = true; // store raw token so we can retrieve it reliably

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer           = true,
                    ValidateAudience         = true,
                    ValidateLifetime         = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer              = jwtOptions.Issuer,
                    ValidAudience            = jwtOptions.Audience,
                    IssuerSigningKey         = new RsaSecurityKey(rsa),
                    ClockSkew                = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    // ── Check blocklist by jti right after JWT signature/expiry validation ──
                    OnTokenValidated = async ctx =>
                    {
                        var jti = ctx.Principal?.FindFirstValue("jti");
                        if (string.IsNullOrWhiteSpace(jti))
                            return;

                        var blocklist = ctx.HttpContext.RequestServices
                            .GetRequiredService<ITokenBlocklistService>();

                        if (await blocklist.IsTokenBlockedAsync(jti))
                        {
                            ctx.Fail("Token has been revoked.");

                            ctx.HttpContext.Response.StatusCode  = 401;
                            ctx.HttpContext.Response.ContentType = "application/json";
                            await ctx.HttpContext.Response.WriteAsync(
                                "{\"success\":false,\"error\":{\"code\":\"TOKEN_REVOKED\",\"message\":\"Token has been revoked. Please log in again.\"}}");
                        }
                    },

                    // ── Suppress duplicate handling when OnTokenValidated already wrote 401 ──
                    OnAuthenticationFailed = ctx =>
                    {
                        if (ctx.Exception?.Message == "Token has been revoked." &&
                            ctx.HttpContext.Response.HasStarted)
                        {
                            ctx.NoResult();
                        }
                        return Task.CompletedTask;
                    },

                    // ── Standard 401 for truly unauthenticated requests ──
                    OnChallenge = ctx =>
                    {
                        if (ctx.HttpContext.Response.HasStarted)
                            return Task.CompletedTask;

                        ctx.HandleResponse();
                        ctx.Response.StatusCode  = 401;
                        ctx.Response.ContentType = "application/json";
                        return ctx.Response.WriteAsync(
                            "{\"success\":false,\"error\":{\"code\":\"UNAUTHORIZED\",\"message\":\"Not authenticated.\"}}");
                    },

                    // ── Standard 403 for authenticated but unauthorized requests ──
                    OnForbidden = ctx =>
                    {
                        if (ctx.HttpContext.Response.HasStarted)
                            return Task.CompletedTask;

                        ctx.Response.StatusCode  = 403;
                        ctx.Response.ContentType = "application/json";
                        return ctx.Response.WriteAsync(
                            "{\"success\":false,\"error\":{\"code\":\"FORBIDDEN\",\"message\":\"You do not have permission to access this resource.\"}}");
                    }
                };
            });

            builder.Services.AddAuthorization();

            builder.Services.AddIdentityCore<User>(options =>
            {
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireDigit           = true;
                options.Password.RequireUppercase       = true;
                options.Password.RequireLowercase       = true;
                options.Password.RequiredLength         = 8;
                options.User.RequireUniqueEmail         = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<UniversityDbContext>()
            .AddDefaultTokenProviders();

            builder.Services.AddScoped<IDataSeeding, DataSeeding>();

            builder.Services.AddRateLimiter(options =>
            {
                options.AddPolicy("PolicyLimitRate", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 100,
                            Window = TimeSpan.FromMinutes(1),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit           = 2
                        }));
                options.RejectionStatusCode = 429;
            });

            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = ApiResponseFactory.CustomValidationErrorResponse;
            });

            builder.Services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(AYA_UIS.Application.AssemblyReference).Assembly));

            builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IServiceManager, ServiceManager>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IAdminService, AdminService>();
            builder.Services.AddScoped<IStudentRegistrationService, StudentRegistrationService>();

            // Token blocklist for logout support (singleton — shared state across requests)
            builder.Services.AddSingleton<ITokenBlocklistService, InMemoryTokenBlocklistService>();

            builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));
            builder.Services.AddScoped<ILocalFileService, LocalFileService>();
            builder.Services.AddHttpClient();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
            builder.Services.AddScoped<IGpaCalculator, GpaCalculator>();
            builder.Services.AddHttpContextAccessor();

            var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "AYA UIS API v1");
                c.RoutePrefix = "swagger";
            });

            app.UseCors("AllowFrontend");
            app.UseMiddleware<GlobalExceptionHandlingMiddelWare>();
            app.UseRateLimiter();

            await using (var scope = app.Services.CreateAsyncScope())
            {
                var dataSeeder = scope.ServiceProvider.GetRequiredService<IDataSeeding>();
                await dataSeeder.SeedDataInfoAsync();
                await dataSeeder.SeedIdentityDataAsync();

                // ── Idempotent schema fixup for known drift ──
                var db = scope.ServiceProvider.GetRequiredService<UniversityDbContext>();
                await db.Database.ExecuteSqlRawAsync(@"
                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'StudentCourseExceptions')
                    BEGIN
                        CREATE TABLE [StudentCourseExceptions] (
                            [Id]          INT            IDENTITY(1,1) NOT NULL,
                            [UserId]      NVARCHAR(450)  NOT NULL,
                            [CourseId]    INT            NOT NULL,
                            [StudyYearId] INT            NOT NULL,
                            [SemesterId]  INT            NOT NULL,
                            CONSTRAINT [PK_StudentCourseExceptions] PRIMARY KEY ([Id])
                        );
                    END
                ");
                await db.Database.ExecuteSqlRawAsync(@"
                    IF NOT EXISTS (
                        SELECT * FROM INFORMATION_SCHEMA.COLUMNS
                        WHERE TABLE_NAME = 'AdminCourseLocks' AND COLUMN_NAME = 'Reason')
                    BEGIN
                        ALTER TABLE [AdminCourseLocks] ADD [Reason] NVARCHAR(MAX) NULL;
                    END
                ");

                // ── Academic Setup: add IsEquivalency + NumericTotal to Registrations ──
                await db.Database.ExecuteSqlRawAsync(@"
                    IF NOT EXISTS (
                        SELECT * FROM INFORMATION_SCHEMA.COLUMNS
                        WHERE TABLE_NAME = 'Registrations' AND COLUMN_NAME = 'IsEquivalency')
                    BEGIN
                        ALTER TABLE [Registrations] ADD [IsEquivalency] BIT NOT NULL CONSTRAINT [DF_Registrations_IsEquivalency] DEFAULT 0;
                    END
                ");
                await db.Database.ExecuteSqlRawAsync(@"
                    IF NOT EXISTS (
                        SELECT * FROM INFORMATION_SCHEMA.COLUMNS
                        WHERE TABLE_NAME = 'Registrations' AND COLUMN_NAME = 'NumericTotal')
                    BEGIN
                        ALTER TABLE [Registrations] ADD [NumericTotal] INT NULL;
                    END
                ");
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseMiddleware<TokenBlocklistMiddleware>(); // Check logged-out tokens
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}


