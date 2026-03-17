using Microsoft.EntityFrameworkCore;
using TransportRouteApi.Models;

namespace TransportRouteApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet<TransitRoute> TransitRoutes { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. Seed Categories
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, CategoryName = "BRT" },
            new Category { Id = 2, CategoryName = "Non-BRT (MetroTrans)" },
            new Category { Id = 3, CategoryName = "Mikrotrans" }
        );

        // 2. Seed 30 Authentic Transjakarta Routes
        modelBuilder.Entity<TransitRoute>().HasData(
            new TransitRoute { Id = 1, RouteName = "Corridor 1", StartingPoint = "Blok M", Destination = "Kota", StartingHour = new TimeOnly(5, 0), EndingHour = new TimeOnly(22, 0) },
            new TransitRoute { Id = 2, RouteName = "Corridor 2", StartingPoint = "Pulo Gadung 1", Destination = "Monas", StartingHour = new TimeOnly(5, 0), EndingHour = new TimeOnly(22, 0) },
            new TransitRoute { Id = 3, RouteName = "Corridor 3", StartingPoint = "Kalideres", Destination = "Pasar Baru", StartingHour = new TimeOnly(5, 0), EndingHour = new TimeOnly(22, 0) },
            new TransitRoute { Id = 4, RouteName = "Corridor 4", StartingPoint = "Pulo Gadung 2", Destination = "Dukuh Atas 2", StartingHour = new TimeOnly(5, 0), EndingHour = new TimeOnly(22, 0) },
            new TransitRoute { Id = 5, RouteName = "Corridor 5", StartingPoint = "Kampung Melayu", Destination = "Ancol", StartingHour = new TimeOnly(5, 0), EndingHour = new TimeOnly(22, 0) },
            new TransitRoute { Id = 6, RouteName = "Corridor 6", StartingPoint = "Ragunan", Destination = "Dukuh Atas 2", StartingHour = new TimeOnly(5, 0), EndingHour = new TimeOnly(22, 0) },
            new TransitRoute { Id = 7, RouteName = "Corridor 7", StartingPoint = "Kampung Rambutan", Destination = "Kampung Melayu", StartingHour = new TimeOnly(5, 0), EndingHour = new TimeOnly(22, 0) },
            new TransitRoute { Id = 8, RouteName = "Corridor 8", StartingPoint = "Lebak Bulus", Destination = "Pasar Baru", StartingHour = new TimeOnly(5, 0), EndingHour = new TimeOnly(22, 0) },
            new TransitRoute { Id = 9, RouteName = "Corridor 9", StartingPoint = "Pinang Ranti", Destination = "Pluit", StartingHour = new TimeOnly(5, 0), EndingHour = new TimeOnly(22, 0) },
            new TransitRoute { Id = 10, RouteName = "Corridor 10", StartingPoint = "Tanjung Priok", Destination = "PGC 2", StartingHour = new TimeOnly(5, 0), EndingHour = new TimeOnly(22, 0) },
            new TransitRoute { Id = 11, RouteName = "Corridor 11", StartingPoint = "Pulo Gebang", Destination = "Kampung Melayu", StartingHour = new TimeOnly(5, 0), EndingHour = new TimeOnly(22, 0) },
            new TransitRoute { Id = 12, RouteName = "Corridor 12", StartingPoint = "Penjaringan", Destination = "Sunter Kelapa Gading", StartingHour = new TimeOnly(5, 0), EndingHour = new TimeOnly(22, 0) },
            new TransitRoute { Id = 13, RouteName = "Corridor 13", StartingPoint = "Ciledug", Destination = "Tendean", StartingHour = new TimeOnly(5, 0), EndingHour = new TimeOnly(22, 0) },
            new TransitRoute { Id = 14, RouteName = "Corridor 14", StartingPoint = "JIS", Destination = "Senen", StartingHour = new TimeOnly(5, 0), EndingHour = new TimeOnly(22, 0) },
            new TransitRoute { Id = 15, RouteName = "Route 1A", StartingPoint = "PIK", Destination = "Balai Kota", StartingHour = new TimeOnly(5, 0), EndingHour = new TimeOnly(22, 0) },
            new TransitRoute { Id = 16, RouteName = "Route 1P", StartingPoint = "Senen", Destination = "Blok M", StartingHour = new TimeOnly(5, 0), EndingHour = new TimeOnly(22, 0) },
            new TransitRoute { Id = 17, RouteName = "Route 1V", StartingPoint = "Lebak Bulus", Destination = "Bundaran HI", StartingHour = new TimeOnly(5, 0), EndingHour = new TimeOnly(22, 0) },
            new TransitRoute { Id = 18, RouteName = "Route 2A", StartingPoint = "Pulo Gadung 1", Destination = "Rawa Buaya", StartingHour = new TimeOnly(5, 0), EndingHour = new TimeOnly(22, 0) },
            new TransitRoute { Id = 19, RouteName = "Route 2P", StartingPoint = "Gondangdia", Destination = "Senen", StartingHour = new TimeOnly(5, 0), EndingHour = new TimeOnly(22, 0) },
            new TransitRoute { Id = 20, RouteName = "Route 2Q", StartingPoint = "Gondangdia", Destination = "Balai Kota", StartingHour = new TimeOnly(5, 0), EndingHour = new TimeOnly(22, 0) },
            new TransitRoute { Id = 21, RouteName = "Route 3E", StartingPoint = "Puri Kembangan", Destination = "Sentraland Cengkareng", StartingHour = new TimeOnly(5, 0), EndingHour = new TimeOnly(22, 0) },
            new TransitRoute { Id = 22, RouteName = "Route 4B", StartingPoint = "Stasiun Manggarai", Destination = "UI", StartingHour = new TimeOnly(5, 0), EndingHour = new TimeOnly(22, 0) },
            new TransitRoute { Id = 23, RouteName = "Route 4C", StartingPoint = "TU Gas", Destination = "Bundaran Senayan", StartingHour = new TimeOnly(5, 0), EndingHour = new TimeOnly(22, 0) },
            new TransitRoute { Id = 24, RouteName = "Route 5C", StartingPoint = "PGC 1", Destination = "Juanda", StartingHour = new TimeOnly(5, 0), EndingHour = new TimeOnly(22, 0) },
            new TransitRoute { Id = 25, RouteName = "Route 5M", StartingPoint = "Kampung Melayu", Destination = "Tanah Abang", StartingHour = new TimeOnly(5, 0), EndingHour = new TimeOnly(22, 0) },
            new TransitRoute { Id = 26, RouteName = "Route 6A", StartingPoint = "Ragunan", Destination = "Monas via Kuningan", StartingHour = new TimeOnly(5, 0), EndingHour = new TimeOnly(22, 0) },
            new TransitRoute { Id = 27, RouteName = "Route 6B", StartingPoint = "Ragunan", Destination = "Monas via Semanggi", StartingHour = new TimeOnly(5, 0), EndingHour = new TimeOnly(22, 0) },
            new TransitRoute { Id = 28, RouteName = "Route 6C", StartingPoint = "Stasiun Tebet", Destination = "Karet via Patra Kuningan", StartingHour = new TimeOnly(5, 0), EndingHour = new TimeOnly(22, 0) },
            new TransitRoute { Id = 29, RouteName = "Route 6D", StartingPoint = "Stasiun Tebet", Destination = "Bundaran Senayan", StartingHour = new TimeOnly(5, 0), EndingHour = new TimeOnly(22, 0) },
            new TransitRoute { Id = 30, RouteName = "Route 7A", StartingPoint = "Kampung Rambutan", Destination = "Lebak Bulus", StartingHour = new TimeOnly(5, 0), EndingHour = new TimeOnly(22, 0) }
        );

        // 3. Seed 50 Vehicles using a C# loop
        var seededVehicles = new List<Vehicle>();
        var random = new Random(42); // Seeded for consistency

        for (int i = 1; i <= 50; i++)
        {
            // Generates names like "TJ-014", "SAF-022", "MYS-045"
            string[] operators = { "TJ", "SAF", "MYS", "BMP" };
            string prefix = operators[i % 4];
            string busName = $"{prefix}-{i:D3}";

            seededVehicles.Add(new Vehicle
            {
                Id = i,
                VehicleName = busName,
                CategoryId = (i % 3) + 1,        // Loops 1, 2, 3
                TransitRouteId = (i % 30) + 1    // Distributes buses evenly across the 30 routes
            });
        }

        modelBuilder.Entity<Vehicle>().HasData(seededVehicles);
    }  
}