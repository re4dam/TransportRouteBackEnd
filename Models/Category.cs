using System;

namespace TransportRouteApi.Models
{
    public class Category
    {
        public long Id { get; set; }
        public required string CategoryName { get; set; }
        public List<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    }
}