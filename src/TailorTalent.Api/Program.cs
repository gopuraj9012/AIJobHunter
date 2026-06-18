using Microsoft.EntityFrameworkCore;
using TailorTalent.Api.Data;
using TailorTalent.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to bind to all interfaces (required for container/sandbox)
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5111);
});

// Add controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// OpenAPI / Swagger
builder.Services.AddOpenApi();

// Database - SQLite
builder.Services.AddDbContext<TailorTalentDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=TailorTalent.db"));

// Service DI registration
builder.Services.AddScoped<IResumeService, ResumeService>();
builder.Services.AddScoped<IJobDescriptionService, JobDescriptionService>();
builder.Services.AddScoped<ITailoringSessionService, TailoringSessionService>();

// Resume parsing service
builder.Services.AddScoped<IResumeParsingService, ResumeParsingService>();

// Subscription & credit management service
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();

// AI Integration service - HttpClient configured to call the FastAPI AI service
builder.Services.AddHttpClient<IAiIntegrationService, AiIntegrationService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("AiService:BaseUrl") ?? "http://localhost:8000");
    client.Timeout = TimeSpan.FromSeconds(60);
});

// CORS - allow Angular frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/openapi/v1.json", "TailorTalent API v1");
    c.RoutePrefix = string.Empty;
});

app.UseCors("AllowAngularFrontend");

app.MapControllers();

// Auto-migrate database on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TailorTalentDbContext>();
    db.Database.EnsureCreated();
}

app.Run();