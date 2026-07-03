using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TourPlannerAPI.Data;

var builder = WebApplication.CreateBuilder(args);
const string AngularCorsPolicy = "AngularDevClient";

// Route all ILogger<T> output through log4net. The rolling-file directory comes
// from configuration (not hard-coded) via a log4net context property.
var logDirectory = builder.Configuration["Logging:LogDirectory"] ?? "Logs";
Directory.CreateDirectory(logDirectory);
log4net.GlobalContext.Properties["LogDirectory"] = logDirectory;
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddLog4Net("log4net.config");

// Add services to the container.
builder.Services.AddDbContext<TourPlannerDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddPolicy(AngularCorsPolicy, policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddHttpClient();

// Data access layer (Repository pattern)
builder.Services.AddScoped<TourPlannerAPI.Repositories.ITourRepository, TourPlannerAPI.Repositories.TourRepository>();
builder.Services.AddScoped<TourPlannerAPI.Repositories.ITourLogRepository, TourPlannerAPI.Repositories.TourLogRepository>();
builder.Services.AddScoped<TourPlannerAPI.Repositories.IUserRepository, TourPlannerAPI.Repositories.UserRepository>();

// Business logic layer
builder.Services.Configure<TourPlannerAPI.Configuration.ComputedAttributeOptions>(
    builder.Configuration.GetSection(TourPlannerAPI.Configuration.ComputedAttributeOptions.SectionName));
builder.Services.AddScoped<TourPlannerAPI.Services.ITourAttributeCalculator, TourPlannerAPI.Services.TourAttributeCalculator>();
builder.Services.AddScoped<TourPlannerAPI.Services.ITourService, TourPlannerAPI.Services.TourService>();
builder.Services.AddScoped<TourPlannerAPI.Services.ITourLogService, TourPlannerAPI.Services.TourLogService>();
builder.Services.AddScoped<TourPlannerAPI.Services.IStatisticsService, TourPlannerAPI.Services.StatisticsService>();
builder.Services.AddScoped<TourPlannerAPI.Services.IUserService, TourPlannerAPI.Services.UserService>();
builder.Services.AddScoped<TourPlannerAPI.Services.IRouteService, TourPlannerAPI.Services.RouteService>();
builder.Services.AddScoped<TourPlannerAPI.Services.IJwtTokenService, TourPlannerAPI.Services.JwtTokenService>();

builder.Services.Configure<TourPlannerAPI.Configuration.ImageStorageOptions>(
    builder.Configuration.GetSection(TourPlannerAPI.Configuration.ImageStorageOptions.SectionName));
builder.Services.AddSingleton<TourPlannerAPI.Services.IImageStorageService, TourPlannerAPI.Services.ImageStorageService>();

// JWT bearer authentication (all parameters come from configuration)
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"]
    ?? throw new InvalidOperationException("Jwt:Key is not configured.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddControllers();

var app = builder.Build();

// Auto-apply migrations on startup so a fresh container is ready immediately
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TourPlannerDbContext>();
    db.Database.Migrate();
}

// Translate domain exceptions into ProblemDetails responses (first in the pipeline)
app.UseMiddleware<TourPlannerAPI.Middleware.ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseCors(AngularCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
