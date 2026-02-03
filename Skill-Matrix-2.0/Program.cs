using Application.Interfaces.Repository;
using Application.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.PostgreSql;
using Infrastructure.Context;
using Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<MatrixDbContext>(options =>
	options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
		b => b.MigrationsAssembly("Infrastructure")));

builder.Services.AddValidatorsFromAssemblyContaining<AssesmentDTOValidator>();

builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddControllers();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddOpenApi();

builder.Services.AddHangfire(config => config
	.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
	.UseSimpleAssemblyNameTypeSerializer()
	.UseRecommendedSerializerSettings()
	.UsePostgreSqlStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfireServer();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseHangfireDashboard("/jobs");

app.MapControllers();

app.Run();