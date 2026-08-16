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
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Application.Features.Assessments.Commands.StartAssessment;
using Infrastructure.Implementation.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<MatrixDbContext>(options =>
	options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
		b => b.MigrationsAssembly("Infrastructure")));

builder.Services.AddManualValidators();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Often needed with cookies/auth
    });
});

builder.Services.AddControllers(options =>
{
    options.Filters.Add<Skill_Matrix_2_0.Filters.ValidationFilter>();
});

builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(StartAssessmentCommand).Assembly);
});

builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key missing")))
    };
})
.AddCookie()
.AddGoogle(googleOptions =>
{
	googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"];
	googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IBadgeEligibilityChecker, BadgeEligibilityChecker>();

builder.Services.Configure<Infrastructure.ExternalServices.EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddHttpClient<IEmailService, Infrastructure.ExternalServices.BrevoEmailService>();

builder.Services.AddHttpClient<IAiService, OpenRouterAiService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(5);
});
builder.Services.AddHttpClient<IAiAnalysisService, AiAnalysisService>();
builder.Services.AddSingleton<Application.Interfaces.Service.ICatalogJobService, Infrastructure.Implementation.Services.CatalogJobService>();

builder.Services.AddHttpClient<ICodeExecutionService, CodeExecutionService>();
builder.Services.AddScoped<IReminderService, ReminderService>();

builder.Services.Configure<Infrastructure.ExternalServices.Cloudinary.CloudinarySettings>(builder.Configuration.GetSection("Cloudinary"));
builder.Services.AddScoped<IPhotoService, Infrastructure.ExternalServices.Cloudinary.CloudinaryPhotoService>();

builder.Services.AddMemoryCache();
builder.Services.AddHostedService<Skill_Matrix_2_0.BackgroundServices.SkillCatalogSeedService>();
// CareerPathSeedService is intentionally removed from startup background services; 
// career path seeding is now event-driven via SkillsAddedNotification when new skills are added.
// builder.Services.AddHostedService<Skill_Matrix_2_0.BackgroundServices.CareerPathSeedService>();

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

app.UseMiddleware<Skill_Matrix_2_0.Middlewares.ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard("/jobs", new DashboardOptions
{
	Authorization = new[] { new Skill_Matrix_2_0.Filters.HangfireDashboardAuthorizationFilter() }
});

RecurringJob.AddOrUpdate<IReminderService>(
	"weekly-assessment-reminders",
	service => service.SendWeeklyRemindersAsync(),
	Cron.Weekly);

app.MapControllers();

app.Run();