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
using Presentation.Hubs;
using Presentation.Services;
using Presistence.Services;

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
                    // ── Allow SignalR WebSocket connections to pass token via query string ──
                    OnMessageReceived = ctx =>
                    {
                        var token = ctx.Request.Query["access_token"];
                        var path  = ctx.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(token) &&
                            path.StartsWithSegments("/hubs", StringComparison.OrdinalIgnoreCase))
                        {
                            ctx.Token = token;
                        }
                        return Task.CompletedTask;
                    },

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

            builder.Services.AddSignalR();

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IServiceManager, ServiceManager>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IAdminService, AdminService>();
            builder.Services.AddScoped<IStudentRegistrationService, StudentRegistrationService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IAcademicYearResetService, AcademicYearResetService>();
            builder.Services.AddScoped<IMaterialResetService, MaterialResetService>();
            builder.Services.AddScoped<IStudentDeletionService, StudentDeletionService>();
            builder.Services.AddScoped<ICourseworkBudgetService, CourseworkBudgetService>();
            builder.Services.AddHostedService<NotificationCleanupService>();

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

                // ── Notification entity extensions ──
                var notifColumns = new (string col, string def)[]
                {
                    ("CourseId",        "INT NULL"),
                    ("QuizId",          "INT NULL"),
                    ("QuizTitle",       "NVARCHAR(MAX) NULL"),
                    ("LectureId",       "INT NULL"),
                    ("LectureTitle",    "NVARCHAR(MAX) NULL"),
                    ("InstructorName",  "NVARCHAR(MAX) NULL"),
                    ("StudentName",     "NVARCHAR(MAX) NULL"),
                    ("StudentCode",     "NVARCHAR(MAX) NULL"),
                    ("TargetStudentId", "NVARCHAR(MAX) NULL"),
                };
                foreach (var (col, def) in notifColumns)
                {
                    await db.Database.ExecuteSqlRawAsync($@"
                        IF NOT EXISTS (
                            SELECT * FROM INFORMATION_SCHEMA.COLUMNS
                            WHERE TABLE_NAME = 'Notifications' AND COLUMN_NAME = '{col}')
                        BEGIN
                            ALTER TABLE [Notifications] ADD [{col}] {def};
                        END
                    ");
                }

                // ── AttemptCount on AssignmentSubmissions (added in previous session) ──
                await db.Database.ExecuteSqlRawAsync(@"
                    IF NOT EXISTS (
                        SELECT * FROM INFORMATION_SCHEMA.COLUMNS
                        WHERE TABLE_NAME = 'AssignmentSubmissions' AND COLUMN_NAME = 'AttemptCount')
                    BEGIN
                        ALTER TABLE [AssignmentSubmissions] ADD [AttemptCount] INT NOT NULL CONSTRAINT [DF_AssignmentSubmissions_AttemptCount] DEFAULT 1;
                    END
                ");

                // ── FinalGrades table ──
                await db.Database.ExecuteSqlRawAsync(@"
                    IF NOT EXISTS (
                        SELECT * FROM INFORMATION_SCHEMA.TABLES
                        WHERE TABLE_NAME = 'FinalGrades')
                    BEGIN
                        CREATE TABLE [FinalGrades] (
                            [Id]              INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                            [StudentId]       NVARCHAR(450)     NOT NULL,
                            [CourseId]        INT               NOT NULL,
                            [FinalScore]      INT               NOT NULL DEFAULT 0,
                            [Bonus]           INT               NOT NULL DEFAULT 0,
                            [AdminFinalTotal] INT               NULL,
                            [Published]       BIT               NOT NULL DEFAULT 0,
                            [RowVersion]      rowversion        NOT NULL
                        );
                    END
                ");

                // ── FinalGrades.RowVersion: optimistic concurrency token (entity has [Timestamp]).
                //     If the table already existed before this column was added to the model,
                //     EF will SELECT it and crash with 'Invalid column name RowVersion'. Add it
                //     idempotently here. SQL Server auto-populates existing rows. ──
                await db.Database.ExecuteSqlRawAsync(@"
                    IF NOT EXISTS (
                        SELECT * FROM INFORMATION_SCHEMA.COLUMNS
                        WHERE TABLE_NAME = 'FinalGrades' AND COLUMN_NAME = 'RowVersion')
                    BEGIN
                        ALTER TABLE [FinalGrades] ADD [RowVersion] rowversion NOT NULL;
                    END
                ");

                // ── FinalGrades.AdminFinalTotal: admin-override total (0-100), nullable. ──
                await db.Database.ExecuteSqlRawAsync(@"
                    IF COL_LENGTH('dbo.FinalGrades', 'AdminFinalTotal') IS NULL
                        ALTER TABLE dbo.FinalGrades ADD AdminFinalTotal INT NULL;
                ");

                // ── FinalGrades unique index on (StudentId, CourseId) per entity attribute. ──
                await db.Database.ExecuteSqlRawAsync(@"
                    IF NOT EXISTS (
                        SELECT 1 FROM sys.indexes
                        WHERE name = 'IX_FinalGrades_StudentId_CourseId'
                          AND object_id = OBJECT_ID('FinalGrades'))
                    BEGIN
                        BEGIN TRY
                            CREATE UNIQUE INDEX [IX_FinalGrades_StudentId_CourseId]
                                ON [FinalGrades] ([StudentId], [CourseId]);
                        END TRY
                        BEGIN CATCH
                            -- Duplicates may exist from prior runs without the unique constraint;
                            -- skip silently so startup does not block. App-level guards already
                            -- handle UNIQUE conflicts on insert.
                        END CATCH
                    END
                ");

                // ── Academic Year Reset: archive flags on Registrations ──
                await db.Database.ExecuteSqlRawAsync(@"
                    IF COL_LENGTH('dbo.Registrations', 'IsArchived') IS NULL
                        ALTER TABLE dbo.Registrations ADD IsArchived BIT NOT NULL CONSTRAINT DF_Registrations_IsArchived DEFAULT 0;
                ");
                await db.Database.ExecuteSqlRawAsync(@"
                    IF COL_LENGTH('dbo.Registrations', 'ArchivedAt') IS NULL
                        ALTER TABLE dbo.Registrations ADD ArchivedAt DATETIME2 NULL;
                ");

                // ── AcademicYearResets (audit log) ──
                await db.Database.ExecuteSqlRawAsync(@"
                    IF NOT EXISTS (
                        SELECT * FROM INFORMATION_SCHEMA.TABLES
                        WHERE TABLE_NAME = 'AcademicYearResets')
                    BEGIN
                        CREATE TABLE [AcademicYearResets] (
                            [Id]                    INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                            [AdminId]               NVARCHAR(450)     NOT NULL,
                            [ExecutedAt]            DATETIME2         NOT NULL,
                            [StudentsCount]         INT               NOT NULL DEFAULT 0,
                            [ForceReset]            BIT               NOT NULL DEFAULT 0,
                            [SelectAll]             BIT               NOT NULL DEFAULT 0,
                            [SourceStudyYearId]     INT               NULL,
                            [SourceSemesterId]      INT               NULL,
                            [TargetStudyYearId]     INT               NULL,
                            [TargetSemesterId]      INT               NULL,
                            [SourceTerm]            NVARCHAR(MAX)     NULL,
                            [TargetTerm]            NVARCHAR(MAX)     NULL,
                            [ArchivedRegistrations] INT               NOT NULL DEFAULT 0,
                            [PassedCount]           INT               NOT NULL DEFAULT 0,
                            [FailedCount]           INT               NOT NULL DEFAULT 0,
                            [UnassignedFailedCount] INT               NOT NULL DEFAULT 0,
                            [FinalGradesPurged]     INT               NOT NULL DEFAULT 0,
                            [QuizAttemptsPurged]    INT               NOT NULL DEFAULT 0,
                            [SubmissionsPurged]     INT               NOT NULL DEFAULT 0,
                            [MidtermsPurged]        INT               NOT NULL DEFAULT 0,
                            [NotificationsSent]     INT               NOT NULL DEFAULT 0,
                            [SummaryJson]           NVARCHAR(MAX)     NULL
                        );
                    END
                ");

                // ── AcademicYearResetSnapshots (per-student JSON backup) ──
                await db.Database.ExecuteSqlRawAsync(@"
                    IF NOT EXISTS (
                        SELECT * FROM INFORMATION_SCHEMA.TABLES
                        WHERE TABLE_NAME = 'AcademicYearResetSnapshots')
                    BEGIN
                        CREATE TABLE [AcademicYearResetSnapshots] (
                            [Id]                 INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                            [ResetId]            INT               NOT NULL,
                            [StudentId]          NVARCHAR(450)     NOT NULL,
                            [CapturedAt]         DATETIME2         NOT NULL,
                            [SourceLevel]        NVARCHAR(MAX)     NULL,
                            [SourceSemester]     NVARCHAR(MAX)     NULL,
                            [TargetLevel]        NVARCHAR(MAX)     NULL,
                            [TargetSemester]     NVARCHAR(MAX)     NULL,
                            [RegistrationsCount] INT               NOT NULL DEFAULT 0,
                            [FinalGradesCount]   INT               NOT NULL DEFAULT 0,
                            [SubmissionsCount]   INT               NOT NULL DEFAULT 0,
                            [QuizAttemptsCount]  INT               NOT NULL DEFAULT 0,
                            [PayloadJson]        NVARCHAR(MAX)     NOT NULL DEFAULT '{{}}'
                        );
                        CREATE INDEX [IX_AcademicYearResetSnapshots_ResetId]
                            ON [AcademicYearResetSnapshots] ([ResetId]);
                        CREATE INDEX [IX_AcademicYearResetSnapshots_StudentId]
                            ON [AcademicYearResetSnapshots] ([StudentId]);
                    END
                ");

                // ── FinalGradeReviews table (admin classification Progress / NotCompleted / Completed) ──
                await db.Database.ExecuteSqlRawAsync(@"
                    IF NOT EXISTS (
                        SELECT * FROM INFORMATION_SCHEMA.TABLES
                        WHERE TABLE_NAME = 'FinalGradeReviews')
                    BEGIN
                        CREATE TABLE [FinalGradeReviews] (
                            [Id]          INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                            [StudentId]   NVARCHAR(450)     NOT NULL,
                            [StudyYearId] INT               NOT NULL,
                            [SemesterId]  INT               NOT NULL,
                            [Status]      NVARCHAR(50)      NOT NULL DEFAULT 'progress',
                            [UpdatedAt]   DATETIME2         NOT NULL DEFAULT SYSUTCDATETIME()
                        );
                        CREATE UNIQUE INDEX [IX_FinalGradeReviews_Student_Term]
                            ON [FinalGradeReviews] ([StudentId], [StudyYearId], [SemesterId]);
                    END
                ");

                // ── Reset Material soft-delete columns on Assignments ──
                await db.Database.ExecuteSqlRawAsync(@"
                    IF COL_LENGTH('dbo.Assignments', 'IsArchived') IS NULL
                        ALTER TABLE dbo.Assignments ADD IsArchived BIT NOT NULL CONSTRAINT DF_Assignments_IsArchived DEFAULT 0;
                ");
                await db.Database.ExecuteSqlRawAsync(@"
                    IF COL_LENGTH('dbo.Assignments', 'DeletedAt') IS NULL
                        ALTER TABLE dbo.Assignments ADD DeletedAt DATETIME2 NULL;
                ");
                await db.Database.ExecuteSqlRawAsync(@"
                    IF COL_LENGTH('dbo.Assignments', 'DeletedById') IS NULL
                        ALTER TABLE dbo.Assignments ADD DeletedById NVARCHAR(450) NULL;
                ");
                await db.Database.ExecuteSqlRawAsync(@"
                    IF COL_LENGTH('dbo.Assignments', 'ResetBatchId') IS NULL
                        ALTER TABLE dbo.Assignments ADD ResetBatchId INT NULL;
                ");
                await db.Database.ExecuteSqlRawAsync(@"
                    IF COL_LENGTH('dbo.Assignments', 'FilePurgedAt') IS NULL
                        ALTER TABLE dbo.Assignments ADD FilePurgedAt DATETIME2 NULL;
                ");
                await db.Database.ExecuteSqlRawAsync(@"
                    IF COL_LENGTH('dbo.Assignments', 'ContentPurgedAt') IS NULL
                        ALTER TABLE dbo.Assignments ADD ContentPurgedAt DATETIME2 NULL;
                ");

                // ── AssignmentSubmissions: only file-purge marker (rows + grades preserved) ──
                await db.Database.ExecuteSqlRawAsync(@"
                    IF COL_LENGTH('dbo.AssignmentSubmissions', 'FilePurgedAt') IS NULL
                        ALTER TABLE dbo.AssignmentSubmissions ADD FilePurgedAt DATETIME2 NULL;
                ");

                // ── Reset Material soft-delete columns on Quizzes ──
                await db.Database.ExecuteSqlRawAsync(@"
                    IF COL_LENGTH('dbo.Quizzes', 'IsArchived') IS NULL
                        ALTER TABLE dbo.Quizzes ADD IsArchived BIT NOT NULL CONSTRAINT DF_Quizzes_IsArchived DEFAULT 0;
                ");
                await db.Database.ExecuteSqlRawAsync(@"
                    IF COL_LENGTH('dbo.Quizzes', 'DeletedAt') IS NULL
                        ALTER TABLE dbo.Quizzes ADD DeletedAt DATETIME2 NULL;
                ");
                await db.Database.ExecuteSqlRawAsync(@"
                    IF COL_LENGTH('dbo.Quizzes', 'DeletedById') IS NULL
                        ALTER TABLE dbo.Quizzes ADD DeletedById NVARCHAR(450) NULL;
                ");
                await db.Database.ExecuteSqlRawAsync(@"
                    IF COL_LENGTH('dbo.Quizzes', 'ResetBatchId') IS NULL
                        ALTER TABLE dbo.Quizzes ADD ResetBatchId INT NULL;
                ");
                await db.Database.ExecuteSqlRawAsync(@"
                    IF COL_LENGTH('dbo.Quizzes', 'FilePurgedAt') IS NULL
                        ALTER TABLE dbo.Quizzes ADD FilePurgedAt DATETIME2 NULL;
                ");

                // ── Quiz GradePerQuestion (points-per-question for coursework budget). ──
                // Default 1 preserves the historical 1-point-per-question behavior for any
                // quizzes that already exist when this column is added.
                await db.Database.ExecuteSqlRawAsync(@"
                    IF COL_LENGTH('dbo.Quizzes', 'GradePerQuestion') IS NULL
                        ALTER TABLE dbo.Quizzes ADD GradePerQuestion INT NOT NULL CONSTRAINT DF_Quizzes_GradePerQuestion DEFAULT 1;
                ");

                // ── MaterialResets (audit log for the Admin Reset Material feature) ──
                await db.Database.ExecuteSqlRawAsync(@"
                    IF NOT EXISTS (
                        SELECT * FROM INFORMATION_SCHEMA.TABLES
                        WHERE TABLE_NAME = 'MaterialResets')
                    BEGIN
                        CREATE TABLE [MaterialResets] (
                            [Id]                       INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                            [CreatedById]              NVARCHAR(450)     NOT NULL,
                            [CreatedAt]                DATETIME2         NOT NULL,
                            [SelectedCourseCount]      INT               NOT NULL DEFAULT 0,
                            [AssignmentCount]          INT               NOT NULL DEFAULT 0,
                            [QuizCount]                INT               NOT NULL DEFAULT 0,
                            [LectureCount]             INT               NOT NULL DEFAULT 0,
                            [SubmissionFilePurgedCount]INT               NOT NULL DEFAULT 0,
                            [InstructorsNotified]      INT               NOT NULL DEFAULT 0,
                            [Status]                   NVARCHAR(50)      NOT NULL DEFAULT 'completed',
                            [ErrorMessage]             NVARCHAR(MAX)     NULL,
                            [SummaryJson]              NVARCHAR(MAX)     NULL
                        );
                    END
                ");

                // ── StudentDeletionAudits (audit log for permanent student delete) ──
                await db.Database.ExecuteSqlRawAsync(@"
                    IF NOT EXISTS (
                        SELECT * FROM INFORMATION_SCHEMA.TABLES
                        WHERE TABLE_NAME = 'StudentDeletionAudits')
                    BEGIN
                        CREATE TABLE [StudentDeletionAudits] (
                            [Id]                            INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                            [DeletedAt]                     DATETIME2         NOT NULL,
                            [DeletedByAdminId]              NVARCHAR(450)     NOT NULL,
                            [DeletedStudentCode]            NVARCHAR(64)      NOT NULL,
                            [DeletedStudentName]            NVARCHAR(256)     NOT NULL,
                            [DeletedStudentEmail]           NVARCHAR(256)     NULL,
                            [RegistrationsRemoved]          INT NOT NULL DEFAULT 0,
                            [FinalGradesRemoved]            INT NOT NULL DEFAULT 0,
                            [MidtermGradesRemoved]          INT NOT NULL DEFAULT 0,
                            [FinalGradeReviewsRemoved]      INT NOT NULL DEFAULT 0,
                            [AssignmentSubmissionsRemoved]  INT NOT NULL DEFAULT 0,
                            [SubmissionFilesRemoved]        INT NOT NULL DEFAULT 0,
                            [QuizAttemptsRemoved]           INT NOT NULL DEFAULT 0,
                            [QuizAnswersRemoved]            INT NOT NULL DEFAULT 0,
                            [NotificationsRemoved]          INT NOT NULL DEFAULT 0,
                            [CourseResultsRemoved]          INT NOT NULL DEFAULT 0,
                            [SemesterGpasRemoved]           INT NOT NULL DEFAULT 0,
                            [UserStudyYearsRemoved]         INT NOT NULL DEFAULT 0,
                            [CourseExceptionsRemoved]       INT NOT NULL DEFAULT 0,
                            [AdminCourseLocksRemoved]       INT NOT NULL DEFAULT 0,
                            [ResetSnapshotsRemoved]         INT NOT NULL DEFAULT 0,
                            [OtpRowsRemoved]                INT NOT NULL DEFAULT 0,
                            [Status]                        NVARCHAR(50)  NOT NULL DEFAULT 'completed',
                            [ErrorMessage]                  NVARCHAR(MAX) NULL
                        );
                    END
                ");

                // ── Auto-fix coursework budget over-budget courses (idempotent) ──
                // Strategy: archive newest overflow items first to bring each course to <= 40.
                // We archive newest quizzes first (since reducing quiz scoring breaks per-question
                // logic), then newest assignments. Final exam (60) and existing grades untouched.
                try
                {
                    const int BUDGET = 40;
                    var courses = await db.Courses.Select(c => c.Id).ToListAsync();
                    foreach (var cid in courses)
                    {
                        int aMax = await db.Assignments.Where(a => a.CourseId == cid && !a.IsArchived).SumAsync(a => (int?)a.Points) ?? 0;
                        // Quiz total per quiz = Questions.Count * GradePerQuestion.
                        var courseQuizzes = await db.Quizzes
                            .Where(q => q.CourseId == cid && !q.IsArchived)
                            .Include(q => q.Questions)
                            .ToListAsync();
                        int qMax = courseQuizzes.Sum(q => (q.Questions?.Count ?? 0) * Math.Max(1, q.GradePerQuestion));
                        int mMax = await db.MidtermGrades.Where(m => m.CourseId == cid).Select(m => (int?)m.Max).MaxAsync() ?? 0;
                        int used = aMax + qMax + mMax;
                        if (used <= BUDGET) continue;

                        // 1) Archive newest quizzes until we fit, or quizzes are exhausted.
                        var quizzes = courseQuizzes.OrderByDescending(q => q.Id).ToList();
                        foreach (var q in quizzes)
                        {
                            if (used <= BUDGET) break;
                            int qPts = (q.Questions?.Count ?? 0) * Math.Max(1, q.GradePerQuestion);
                            q.IsArchived = true;
                            q.DeletedAt = DateTime.UtcNow;
                            q.DeletedById = "auto-fix";
                            used -= qPts;
                        }

                        // 2) Archive newest assignments next.
                        if (used > BUDGET)
                        {
                            var asns = await db.Assignments
                                .Where(a => a.CourseId == cid && !a.IsArchived)
                                .OrderByDescending(a => a.Id)
                                .ToListAsync();
                            foreach (var a in asns)
                            {
                                if (used <= BUDGET) break;
                                a.IsArchived = true;
                                a.DeletedAt = DateTime.UtcNow;
                                a.DeletedById = "auto-fix";
                                used -= a.Points;
                            }
                        }

                        // 3) If midterm Max alone is over the remaining budget after archives,
                        //    cap the highest stored midterm Max rows down. This is rare.
                        if (used > BUDGET)
                        {
                            int over = used - BUDGET;
                            var mts = await db.MidtermGrades
                                .Where(m => m.CourseId == cid)
                                .OrderByDescending(m => m.Max)
                                .ToListAsync();
                            foreach (var m in mts)
                            {
                                if (over <= 0) break;
                                int reduce = Math.Min(over, m.Max);
                                m.Max = m.Max - reduce;
                                if (m.Grade > m.Max) m.Grade = m.Max;
                                over -= reduce;
                            }
                        }
                    }
                    await db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                    logger.LogWarning(ex, "Coursework budget auto-fix skipped (non-fatal).");
                }
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseMiddleware<TokenBlocklistMiddleware>(); // Check logged-out tokens
            app.UseAuthorization();
            app.MapControllers();
            app.MapHub<NotificationHub>("/hubs/notifications");
            app.Run();
        }
    }
}


