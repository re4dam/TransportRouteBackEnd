using System;

namespace TransportRouteApi.Models
{
    public class TransitRoute
    {
        // Notice we completely removed the parameterized constructor.
        // The compiler will now automatically provide a hidden parameterless one,
        // which perfectly matches how our Controller is trying to create it!

        public long Id { get; set; }
        public required string RouteName { get; set; }
        public required string StartingPoint { get; set; }
        public required string Destination { get; set; }
        public TimeOnly StartingHour { get; set; }
        public TimeOnly EndingHour { get; set; }

        public List<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    }
}