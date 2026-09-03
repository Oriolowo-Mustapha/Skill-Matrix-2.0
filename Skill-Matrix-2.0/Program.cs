using Application.Interfaces.Repository;
using Application.Interfaces.Service;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using Application.Validators;
using Application.Extensions;
using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.PostgreSql;
using Infrastructure.Context;
using Infrastructure.Implementation.Repositories;
using Infrastructure.ExternalServices;
using Infrastructure.ExternalServices.Cloudinary;
using Infrastructure.Implementation.Services;
using Infrastructure.Implementation.Services.CodeHarnesses;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Application.Features.Assessments.Commands.StartAssessment;
using Skill_Matrix_2_0.Filters;
using Skill_Matrix_2_0.Middlewares;
using Skill_Matrix_2_0.BackgroundServices;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<MatrixDbContext>(options =>
	options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
		b => b.MigrationsAssembly("Infrastructure")));

builder.Services.AddManualValidators();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5175")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Often needed with cookies/auth
    });
});

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
});

builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(StartAssessmentCommand).Assembly);
});

builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key missing")))
    };
})
.AddCookie()
.AddGoogle(googleOptions =>
{
	googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? string.Empty;
	googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? string.Empty;
});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IBadgeEligibilityChecker, BadgeEligibilityChecker>();
builder.Services.AddScoped<IActivityLogService, ActivityLogService>();

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddHttpClient<IEmailService, BrevoEmailService>();

builder.Services.AddHttpClient<IAiService, OpenRouterAiService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(5);
});
builder.Services.AddHttpClient<IAiAnalysisService, AiAnalysisService>();
builder.Services.AddSingleton<ICatalogJobService, CatalogJobService>();

// Code Execution Test Harness Builders & Factory
builder.Services.AddScoped<ICodeHarnessBuilder, CSharpHarnessBuilder>();
builder.Services.AddScoped<ICodeHarnessBuilder, PythonHarnessBuilder>();
builder.Services.AddScoped<ICodeHarnessBuilder, JavaScriptHarnessBuilder>();
builder.Services.AddScoped<ICodeHarnessBuilder, TypeScriptHarnessBuilder>();
builder.Services.AddScoped<ICodeHarnessBuilder, JavaHarnessBuilder>();
builder.Services.AddScoped<ICodeHarnessBuilder, CppHarnessBuilder>();
builder.Services.AddScoped<ICodeHarnessFactory, CodeHarnessFactory>();

builder.Services.AddHttpClient<ICodeExecutionService, CodeExecutionService>();
builder.Services.AddScoped<IReminderService, ReminderService>();

builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("Cloudinary"));
builder.Services.AddScoped<IPhotoService, CloudinaryPhotoService>();

builder.Services.AddMemoryCache();
builder.Services.AddHostedService<SkillCatalogSeedService>();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes.Add("Bearer", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter your JWT token"
        });

        foreach (var path in document.Paths.Values)
        {
            foreach (var operation in path.Operations.Values)
            {
                operation.Security.Add(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        Array.Empty<string>()
                    }
                });
            }
        }
        return Task.CompletedTask;
    });
});

builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
	.UseSimpleAssemblyNameTypeSerializer()
	.UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"))));

builder.Services.AddHangfireServer();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
	app.MapScalarApiReference();
}

app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard("/jobs", new DashboardOptions
{
	Authorization = new[] { new HangfireDashboardAuthorizationFilter() }
});

RecurringJob.AddOrUpdate<IReminderService>(
	"weekly-assessment-reminders",
	service => service.SendWeeklyRemindersAsync(),
	Cron.Weekly);

app.MapControllers();

app.Run();