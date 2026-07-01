using Microsoft.EntityFrameworkCore;
using TourPlannerAPI.Data;

var builder = WebApplication.CreateBuilder(args);
const string AngularCorsPolicy = "AngularDevClient";

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
builder.Services.AddScoped<TourPlannerAPI.Services.ITourService, TourPlannerAPI.Services.TourService>();
builder.Services.AddScoped<TourPlannerAPI.Services.ITourLogService, TourPlannerAPI.Services.TourLogService>();
builder.Services.AddScoped<TourPlannerAPI.Services.IUserService, TourPlannerAPI.Services.UserService>();
builder.Services.AddScoped<TourPlannerAPI.Services.IRouteService, TourPlannerAPI.Services.RouteService>();

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

app.UseAuthorization();

app.MapControllers();

app.Run();
