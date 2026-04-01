using Microsoft.EntityFrameworkCore;
using TransportRoute.Core.Data;
using TransportRoute.Core.Models;
using TransportRoute.Core.Converters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using TransportRoute.Security.Hashing;
using TransportRoute.Security.Tokens;
using TransportRoute.Security.Interfaces;
using System.Text;
using Bogus; // Meant to generate data

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new TimeOnlyJsonConverter());
    });

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
    
// Read allowed frontend origins from config and normalize trailing slash.
// Include common local loopback variants as a safe fallback for local development.
var allowedFrontendOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
                            ??
                            [
                                "http://localhost:3000",
                                "https://localhost:3000",
                                "http://localhost:5500",
                                "https://localhost:5500"
                            ];

allowedFrontendOrigins = allowedFrontendOrigins
    .Where(origin => !string.IsNullOrWhiteSpace(origin))
    .Select(origin => origin.Trim().TrimEnd('/'))
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToArray();

builder.Services.AddCors(options =>
{
    options.AddPolicy("SecurePolicy", policy =>
    {
        policy.WithOrigins(allowedFrontendOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Register reusable Security services
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtProvider, JwtProvider>();

// Configure JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.ContainsKey("jwt"))
                {
                    context.Token = context.Request.Cookies["jwt"];
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// Tells the backend to look for a header named 'X-CSRF-TOKEN'
builder.Services.AddAntiforgery(options => 
{
    options.HeaderName = "X-CSRF-TOKEN";
});

var app = builder.Build();

// Function on generating 100 data of routes
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Temporary schema guard: create Users table when missing.
    context.Database.ExecuteSqlRaw(@"
IF OBJECT_ID(N'[dbo].[Users]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Users]
    (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Username] NVARCHAR(450) NOT NULL,
        [PasswordHash] NVARCHAR(MAX) NOT NULL,
        [Role] NVARCHAR(450) NOT NULL
    );

    CREATE UNIQUE INDEX [IX_Users_Username] ON [dbo].[Users]([Username]);
END");
    
    // Only generate data if the table is completely empty
    if (!context.TransitRoutes.Any())
    {
        Console.WriteLine("Database is empty. Seeding 100 realistic transit routes...");

        // A list of local transit hubs to make the data look highly authentic
        var locations = new[] { 
            "Terminal Cicaheum", "Terminal Ledeng", "Bundaran Cibiru", "Cicadas", 
            "Alun-alun", "Dago", "Cihampelas", "Cibaduyut", "Leuwi Panjang", "Stasiun Hall",
            "Antapani", "Ujung Berung", "Pasteur", "Buah Batu", "Dipatiukur"
        };

        // Configure the Bogus Faker with the Indonesian locale
        var routeFaker = new Faker<TransitRoute>("id_ID")
            .RuleFor(r => r.StartingPoint, f => f.PickRandom(locations))
            .RuleFor(r => r.Destination, f => f.PickRandom(locations))
            // Combine them for the route name
            .RuleFor(r => r.RouteName, (f, r) => $"{r.StartingPoint} - {r.Destination}")
            // Generate realistic operating hours
            .RuleFor(r => r.StartingHour, f => new TimeOnly(f.Random.Int(4, 7), 0))
            .RuleFor(r => r.EndingHour, f => new TimeOnly(f.Random.Int(18, 23), 0));

        // Generate 100 fake routes
        var fakeRoutes = routeFaker.Generate(100);

        // Quick cleanup: Ensure no route starts and ends at the exact same place
        foreach (var route in fakeRoutes)
        {
            if (route.StartingPoint == route.Destination)
            {
                route.Destination = "Kebon Kelapa"; // Fallback destination
                route.RouteName = $"{route.StartingPoint} - {route.Destination}";
            }
        }

        // Bulk insert into SQL Server
        context.TransitRoutes.AddRange(fakeRoutes);
        context.SaveChanges();
        
        Console.WriteLine("Seeding complete!");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUi(options =>
    {
        options.DocumentPath = "/openapi/v1.json";
    });
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseRouting();

// Add this line exactly here
app.UseCors("SecurePolicy");

app.UseAntiforgery(); // Enable CSRF protection middleware

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
