using System;

namespace TransportRouteApi.DTOs;

public class CreateVehicleDto
{
    public required string VehicleName { get; set; }
    public required long CategoryId { get; set; }
    public required long TransitRouteId { get; set; }
}