using Application.Interfaces.Repository;
using Application.Interfaces.Service;
using Application.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.PostgreSql;
using Infrastructure.Context;
using Infrastructure.Implementation.Repositories;
using Infrastructure.Implementation.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<MatrixDbContext>(options =>
	options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
		b => b.MigrationsAssembly("Infrastructure")));

builder.Services.AddValidatorsFromAssemblyContaining<AssesmentDTOValidator>();

builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddControllers();

builder.Services.AddAuthentication(options =>
{
	options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie()
.AddGoogle(googleOptions =>
{
	googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"];
	googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailService, MailKitEmailService>();

builder.Services.AddOpenApi();

builder.Services.AddHangfire(config => config
	.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
	.UseSimpleAssemblyNameTypeSerializer()
	.UseRecommendedSerializerSettings()
	.UsePostgreSqlStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfireServer();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard("/jobs");

app.MapControllers();

app.Run();