using System.Text.Json.Serialization;
using JobTracker.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o => o.SwaggerDoc("v1", new()
{
    Title = "Job Application Tracker API",
    Version = "v1",
    Description = "Track companies, applications, interview stages, and status history."
}));

// Provider is swapped to SqlServer via configuration when running against Azure SQL.
var provider = builder.Configuration.GetValue("Database:Provider", "Sqlite")!;
var connectionString = builder.Configuration.GetConnectionString("Default");

builder.Services.AddDbContext<JobTrackerDbContext>(options =>
{
    if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
    {
        options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure());
    }
    else
    {
        options.UseSqlite(connectionString ?? "Data Source=jobtracker.db");
    }
});

builder.Services.AddHealthChecks().AddDbContextCheck<JobTrackerDbContext>();

const string CorsPolicy = "frontend";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(o => o.AddPolicy(CorsPolicy, policy =>
{
    if (allowedOrigins.Length > 0)
    {
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
    }
}));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<JobTrackerDbContext>();
    await db.Database.MigrateAsync();
}

app.UseSwagger();
app.UseSwaggerUI(o =>
{
    o.SwaggerEndpoint("/swagger/v1/swagger.json", "Job Application Tracker API v1");
    o.RoutePrefix = "swagger";
});

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors(CorsPolicy);
app.MapControllers();
app.MapHealthChecks("/health");
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.Run();
