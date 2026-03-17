using System;

namespace TransportRouteApi.DTOs;

public class VehicleResponseDto
{
    public long Id { get; set; }
    public required string VehicleName { get; set; }
    public required string CategoryName { get; set; } // Flattened!
    public required string RouteName { get; set; }    // Flattened!
}