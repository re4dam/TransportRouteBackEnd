using Microsoft.EntityFrameworkCore;
using TransportRoute.Core.Data;
using TransportRoute.Core.Models;
using TransportRoute.Core.Converters;
using Bogus; // Meant to generate data

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new TimeOnlyJsonConverter());
    });

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
    
// Read the allowed URL from appsettings.json, falling back to localhost if not found
var allowedFrontendUrl = builder.Configuration.GetValue<string>("AllowedOrigins:Frontend") 
                         ?? "http://localhost:3000";

builder.Services.AddCors(options =>
{
    options.AddPolicy("SecurePolicy", policy =>
    {
        policy.WithOrigins(allowedFrontendUrl)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Function on generating 100 data of routes
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
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

app.UseHttpsRedirection();
app.UseRouting();

// Add this line exactly here
app.UseCors("SecurePolicy");

app.UseAuthorization();
app.MapControllers();

app.Run();
