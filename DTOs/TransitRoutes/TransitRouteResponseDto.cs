using System;

namespace TransportRouteApi.DTOs;

public class TransitRouteResponseDto
{
    public long Id { get; set; }
    public required string RouteName { get; set; }
    public required string StartingPoint { get; set; }
    public required string Destination { get; set; }
    public TimeOnly StartingHour { get; set; }
    public TimeOnly EndingHour { get; set; }

    public List<VehicleResponseDto> Vehicles { get; set; } = new List<VehicleResponseDto>();
}