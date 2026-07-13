using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TailorTalent.Api.Data;
using TailorTalent.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to bind to all interfaces (required for container/sandbox)
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5111);
});

// JWT Authentication
var jwtKey = builder.Configuration.GetValue<string>("Jwt:Key") ?? "TailorTalentSuperSecretKeyThatIsAtLeast32Bytes!";
var jwtIssuer = builder.Configuration.GetValue<string>("Jwt:Issuer") ?? "TailorTalent";
var jwtAudience = builder.Configuration.GetValue<string>("Jwt:Audience") ?? "TailorTalent";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// Add controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// OpenAPI / Swagger
builder.Services.AddOpenApi();

// Database - provider selected via Database:Provider ("Sqlite" | "SqlServer").
// Connection string resolution: ConnectionStrings:<Provider> → ConnectionStrings:DefaultConnection → SQLite file.
var dbProvider = builder.Configuration.GetValue<string>("Database:Provider") ?? "Sqlite";
var connectionString = builder.Configuration.GetConnectionString(dbProvider)
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=TailorTalent.db";

builder.Services.AddDbContext<TailorTalentDbContext>(options =>
{
    if (dbProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
        options.UseSqlServer(connectionString);
    else
        options.UseSqlite(connectionString);
});

// Service DI registration
builder.Services.AddScoped<IResumeService, ResumeService>();
builder.Services.AddScoped<IJobDescriptionService, JobDescriptionService>();
builder.Services.AddScoped<ITailoringSessionService, TailoringSessionService>();

// Auth service
builder.Services.AddScoped<IAuthService, AuthService>();

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Auto-migrate database on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TailorTalentDbContext>();
    db.Database.EnsureCreated();
}

app.Run();

// Exposes the implicit Program class to WebApplicationFactory in the test project.
public partial class Program { }