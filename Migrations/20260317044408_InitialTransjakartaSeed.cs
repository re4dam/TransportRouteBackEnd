using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TransportRouteApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialTransjakartaSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TransitRoutes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RouteName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartingPoint = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Destination = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartingHour = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndingHour = table.Column<TimeOnly>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransitRoutes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vehicles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VehicleName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CategoryId = table.Column<long>(type: "bigint", nullable: false),
                    TransitRouteId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vehicles_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Vehicles_TransitRoutes_TransitRouteId",
                        column: x => x.TransitRouteId,
                        principalTable: "TransitRoutes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CategoryName" },
                values: new object[,]
                {
                    { 1L, "BRT" },
                    { 2L, "Non-BRT (MetroTrans)" },
                    { 3L, "Mikrotrans" }
                });

            migrationBuilder.InsertData(
                table: "TransitRoutes",
                columns: new[] { "Id", "Destination", "EndingHour", "RouteName", "StartingHour", "StartingPoint" },
                values: new object[,]
                {
                    { 1L, "Kota", new TimeOnly(22, 0, 0), "Corridor 1", new TimeOnly(5, 0, 0), "Blok M" },
                    { 2L, "Monas", new TimeOnly(22, 0, 0), "Corridor 2", new TimeOnly(5, 0, 0), "Pulo Gadung 1" },
                    { 3L, "Pasar Baru", new TimeOnly(22, 0, 0), "Corridor 3", new TimeOnly(5, 0, 0), "Kalideres" },
                    { 4L, "Dukuh Atas 2", new TimeOnly(22, 0, 0), "Corridor 4", new TimeOnly(5, 0, 0), "Pulo Gadung 2" },
                    { 5L, "Ancol", new TimeOnly(22, 0, 0), "Corridor 5", new TimeOnly(5, 0, 0), "Kampung Melayu" },
                    { 6L, "Dukuh Atas 2", new TimeOnly(22, 0, 0), "Corridor 6", new TimeOnly(5, 0, 0), "Ragunan" },
                    { 7L, "Kampung Melayu", new TimeOnly(22, 0, 0), "Corridor 7", new TimeOnly(5, 0, 0), "Kampung Rambutan" },
                    { 8L, "Pasar Baru", new TimeOnly(22, 0, 0), "Corridor 8", new TimeOnly(5, 0, 0), "Lebak Bulus" },
                    { 9L, "Pluit", new TimeOnly(22, 0, 0), "Corridor 9", new TimeOnly(5, 0, 0), "Pinang Ranti" },
                    { 10L, "PGC 2", new TimeOnly(22, 0, 0), "Corridor 10", new TimeOnly(5, 0, 0), "Tanjung Priok" },
                    { 11L, "Kampung Melayu", new TimeOnly(22, 0, 0), "Corridor 11", new TimeOnly(5, 0, 0), "Pulo Gebang" },
                    { 12L, "Sunter Kelapa Gading", new TimeOnly(22, 0, 0), "Corridor 12", new TimeOnly(5, 0, 0), "Penjaringan" },
                    { 13L, "Tendean", new TimeOnly(22, 0, 0), "Corridor 13", new TimeOnly(5, 0, 0), "Ciledug" },
                    { 14L, "Senen", new TimeOnly(22, 0, 0), "Corridor 14", new TimeOnly(5, 0, 0), "JIS" },
                    { 15L, "Balai Kota", new TimeOnly(22, 0, 0), "Route 1A", new TimeOnly(5, 0, 0), "PIK" },
                    { 16L, "Blok M", new TimeOnly(22, 0, 0), "Route 1P", new TimeOnly(5, 0, 0), "Senen" },
                    { 17L, "Bundaran HI", new TimeOnly(22, 0, 0), "Route 1V", new TimeOnly(5, 0, 0), "Lebak Bulus" },
                    { 18L, "Rawa Buaya", new TimeOnly(22, 0, 0), "Route 2A", new TimeOnly(5, 0, 0), "Pulo Gadung 1" },
                    { 19L, "Senen", new TimeOnly(22, 0, 0), "Route 2P", new TimeOnly(5, 0, 0), "Gondangdia" },
                    { 20L, "Balai Kota", new TimeOnly(22, 0, 0), "Route 2Q", new TimeOnly(5, 0, 0), "Gondangdia" },
                    { 21L, "Sentraland Cengkareng", new TimeOnly(22, 0, 0), "Route 3E", new TimeOnly(5, 0, 0), "Puri Kembangan" },
                    { 22L, "UI", new TimeOnly(22, 0, 0), "Route 4B", new TimeOnly(5, 0, 0), "Stasiun Manggarai" },
                    { 23L, "Bundaran Senayan", new TimeOnly(22, 0, 0), "Route 4C", new TimeOnly(5, 0, 0), "TU Gas" },
                    { 24L, "Juanda", new TimeOnly(22, 0, 0), "Route 5C", new TimeOnly(5, 0, 0), "PGC 1" },
                    { 25L, "Tanah Abang", new TimeOnly(22, 0, 0), "Route 5M", new TimeOnly(5, 0, 0), "Kampung Melayu" },
                    { 26L, "Monas via Kuningan", new TimeOnly(22, 0, 0), "Route 6A", new TimeOnly(5, 0, 0), "Ragunan" },
                    { 27L, "Monas via Semanggi", new TimeOnly(22, 0, 0), "Route 6B", new TimeOnly(5, 0, 0), "Ragunan" },
                    { 28L, "Karet via Patra Kuningan", new TimeOnly(22, 0, 0), "Route 6C", new TimeOnly(5, 0, 0), "Stasiun Tebet" },
                    { 29L, "Bundaran Senayan", new TimeOnly(22, 0, 0), "Route 6D", new TimeOnly(5, 0, 0), "Stasiun Tebet" },
                    { 30L, "Lebak Bulus", new TimeOnly(22, 0, 0), "Route 7A", new TimeOnly(5, 0, 0), "Kampung Rambutan" }
                });

            migrationBuilder.InsertData(
                table: "Vehicles",
                columns: new[] { "Id", "CategoryId", "TransitRouteId", "VehicleName" },
                values: new object[,]
                {
                    { 1L, 2L, 2L, "SAF-001" },
                    { 2L, 3L, 3L, "MYS-002" },
                    { 3L, 1L, 4L, "BMP-003" },
                    { 4L, 2L, 5L, "TJ-004" },
                    { 5L, 3L, 6L, "SAF-005" },
                    { 6L, 1L, 7L, "MYS-006" },
                    { 7L, 2L, 8L, "BMP-007" },
                    { 8L, 3L, 9L, "TJ-008" },
                    { 9L, 1L, 10L, "SAF-009" },
                    { 10L, 2L, 11L, "MYS-010" },
                    { 11L, 3L, 12L, "BMP-011" },
                    { 12L, 1L, 13L, "TJ-012" },
                    { 13L, 2L, 14L, "SAF-013" },
                    { 14L, 3L, 15L, "MYS-014" },
                    { 15L, 1L, 16L, "BMP-015" },
                    { 16L, 2L, 17L, "TJ-016" },
                    { 17L, 3L, 18L, "SAF-017" },
                    { 18L, 1L, 19L, "MYS-018" },
                    { 19L, 2L, 20L, "BMP-019" },
                    { 20L, 3L, 21L, "TJ-020" },
                    { 21L, 1L, 22L, "SAF-021" },
                    { 22L, 2L, 23L, "MYS-022" },
                    { 23L, 3L, 24L, "BMP-023" },
                    { 24L, 1L, 25L, "TJ-024" },
                    { 25L, 2L, 26L, "SAF-025" },
                    { 26L, 3L, 27L, "MYS-026" },
                    { 27L, 1L, 28L, "BMP-027" },
                    { 28L, 2L, 29L, "TJ-028" },
                    { 29L, 3L, 30L, "SAF-029" },
                    { 30L, 1L, 1L, "MYS-030" },
                    { 31L, 2L, 2L, "BMP-031" },
                    { 32L, 3L, 3L, "TJ-032" },
                    { 33L, 1L, 4L, "SAF-033" },
                    { 34L, 2L, 5L, "MYS-034" },
                    { 35L, 3L, 6L, "BMP-035" },
                    { 36L, 1L, 7L, "TJ-036" },
                    { 37L, 2L, 8L, "SAF-037" },
                    { 38L, 3L, 9L, "MYS-038" },
                    { 39L, 1L, 10L, "BMP-039" },
                    { 40L, 2L, 11L, "TJ-040" },
                    { 41L, 3L, 12L, "SAF-041" },
                    { 42L, 1L, 13L, "MYS-042" },
                    { 43L, 2L, 14L, "BMP-043" },
                    { 44L, 3L, 15L, "TJ-044" },
                    { 45L, 1L, 16L, "SAF-045" },
                    { 46L, 2L, 17L, "MYS-046" },
                    { 47L, 3L, 18L, "BMP-047" },
                    { 48L, 1L, 19L, "TJ-048" },
                    { 49L, 2L, 20L, "SAF-049" },
                    { 50L, 3L, 21L, "MYS-050" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_CategoryId",
                table: "Vehicles",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_TransitRouteId",
                table: "Vehicles",
                column: "TransitRouteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Vehicles");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "TransitRoutes");
        }
    }
}
