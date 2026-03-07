
using Domain.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Presistence;
using Presistence.Repositories;
using AYA_UIS.Application.Contracts;
using AYA_UIS.MiddelWares;
using Microsoft.AspNetCore.Mvc;
using AYA_UIS.Factories;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Shared.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Cryptography;
using System.Threading.RateLimiting;
using AYA_UIS.Core.Domain.Entities.Identity;
using AYA_UIS.Core.Services.Implementations;
using AYA_UIS.Application.Mapping;
using Infrastructure.Services;
using System.Text.Json.Serialization;
using Services.Implementatios;
using AYA_UIS.Core.Abstractions.Contracts;
using AYA_UIS.Core.Domain.Contracts;
using Abstraction.Contracts;

namespace AYA_UIS
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins("http://localhost:3000")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            }); ;
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddDbContext<UniversityDbContext>(options =>
               options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
           );

            #region Auth
            builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("JwtOptions"));
            var jwtOptions = builder.Configuration.GetSection("JwtOptions").Get<JwtOptions>();
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(options =>
            {

                var publicKey = File.ReadAllText("Keys/public_key.pem");

                var rsa = RSA.Create();
                rsa.ImportFromPem(publicKey);

                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new RsaSecurityKey(rsa),


                };

            });





            builder.Services.AddAuthorization();

            builder.Services.AddIdentityCore<User>(options =>
            {
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.User.RequireUniqueEmail = true;

            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<UniversityDbContext>()
            .AddDefaultTokenProviders();
            builder.Services.AddScoped<IDataSeeding, DataSeeding>();

            // Rate Limit 
            builder.Services.AddRateLimiter(options =>
            {

                options.AddPolicy("PolicyLimitRate", httpContext =>
                {
                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress!.ToString(),
                        factory: key => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 3,
                            Window = TimeSpan.FromMinutes(1),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 2
                        });
                });
            });



            #endregion

            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = ApiResponseFactory.CustomValidationErrorResponse;
            });

            // MediatR for CQRS
            builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AYA_UIS.Application.AssemblyReference).Assembly));

            // AutoMapper
            builder.Services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });

            // Unit of Work & Repositories
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Service Manager (Auth + Roles)
            builder.Services.AddScoped<IServiceManager, ServiceManager>();
            builder.Services.AddScoped<IUserService, UserService>();

            // Configure Cloudinary Settings
            builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));

            // Local File Service   
            builder.Services.AddScoped<ILocalFileService, LocalFileService>();

            // Infrastructure Services
            builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();

            // Calc Gpa
            builder.Services.AddScoped<IGpaCalculator, GpaCalculator>();

            // GPA Calculation Service

            builder.Services.AddHttpContextAccessor();


            #region Auth In Swagger

            builder.Services.AddSwaggerGen(option =>
            {
                option.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo API", Version = "v1" });
                option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter a valid token",
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



            //builder.Services.Configure<IdentityOptions>(options =>
            //{
            //    options.Lockout.MaxFailedAccessAttempts = 5;
            //    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
            //    options.Lockout.AllowedForNewUsers = true;
            //});

            //builder.Services.AddRateLimiter(options =>
            //{
            //    options.AddFixedWindowLimiter("LoginPolicy", o =>
            //    {
            //        o.Window = TimeSpan.FromSeconds(30);
            //        o.PermitLimit = 5;
            //        o.QueueLimit = 0;
            //    });
            //});



            #endregion

            var app = builder.Build();



            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseCors("AllowFrontend");
            app.UseMiddleware<GlobalExceptionHandlingMiddelWare>();

            var scope = app.Services.CreateScope();
            var dataSeeder = scope.ServiceProvider.GetRequiredService<IDataSeeding>();
            await dataSeeder.SeedDataInfoAsync();
            await dataSeeder.SeedIdentityDataAsync();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.MapControllers();
            //app.UseRateLimiter();
            //app.MapPost("/Login", (HttpContext context) =>
            //{
            //}).RequireRateLimiting("LoginPolicy");

            app.Run();
        }
    }
}
