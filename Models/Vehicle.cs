using System;

namespace TransportRouteApi.Models;

public class Vehicle
{
    public long Id { get; set; }
    public required string VehicleName { get; set; }
    public required long CategoryId { get; set; }
    public Category Category { get; set; }

    public required long TransitRouteId { get; set; }
    public TransitRoute TransitRoute { get; set; }
}