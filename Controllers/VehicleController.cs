using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransportRoute.Core.Data;
using TransportRoute.Core.Models;
using TransportRouteApi.DTOs;

namespace TransportRouteApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VehicleController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VehicleController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Vehicle
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDto<VehicleResponseDto>>> GetVehicles(
            [FromQuery] string? keyword, 
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 12)
        {
            var query = _context.Vehicles.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(v => v.VehicleName.Contains(keyword));
            }

            var totalCount = await query.CountAsync();

            var vehicles = await query
                .Include(v => v.Category)
                .Include(v => v.TransitRoute)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(v => new VehicleResponseDto
                {
                    Id = v.Id,
                    VehicleName = v.VehicleName,
                    CategoryName = v.Category.CategoryName,
                    RouteName = v.TransitRoute.RouteName
                })
                .ToListAsync();

            return Ok(new PaginatedResponseDto<VehicleResponseDto>
            {
                Items = vehicles,
                TotalCount = totalCount,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        }

        // GET: api/Vehicles/all
        [HttpGet("all")] 
        public async Task<ActionResult<IEnumerable<object>>> GetAllVehiclesForDropdown()
        {
            var dropdownData = await _context.Vehicles
                .Select(v => new 
                { 
                    id = v.Id, 
                    vehicleName = v.VehicleName 
                })
                .ToListAsync();

            return Ok(dropdownData);
        }

        // PUT: api/Vehicle/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [ValidateAntiForgeryToken] // <-- Add this to enforce the shield
        public async Task<IActionResult> PutVehicle(long id, CreateVehicleDto vehicleDto)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null)
            {
                return NotFound();
            }

            // Update the fields based on what the frontend sent
            vehicle.VehicleName = vehicleDto.VehicleName;
            vehicle.CategoryId = vehicleDto.CategoryId;
            vehicle.TransitRouteId = vehicleDto.TransitRouteId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VehicleExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Vehicle
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [ValidateAntiForgeryToken] // <-- Add this to enforce the shield
        public async Task<ActionResult<VehicleResponseDto>> PostVehicle(CreateVehicleDto vehicleDto)
        {
            // 1. Map incoming DTO to raw entity
            var vehicle = new Vehicle
            {
                VehicleName = vehicleDto.VehicleName,
                CategoryId = vehicleDto.CategoryId,
                TransitRouteId = vehicleDto.TransitRouteId
            };

            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            // 2. Fetch it back from the DB to get the human-readable CategoryName and RouteName
            // If we don't do this, we can't return a complete VehicleResponseDto!
            var createdVehicle = await _context.Vehicles
                .Include(v => v.Category)
                .Include(v => v.TransitRoute)
                .FirstOrDefaultAsync(v => v.Id == vehicle.Id);

            var responseDto = new VehicleResponseDto
            {
                Id = createdVehicle.Id,
                VehicleName = createdVehicle.VehicleName,
                CategoryName = createdVehicle.Category.CategoryName,
                RouteName = createdVehicle.TransitRoute.RouteName
            };

            return CreatedAtAction(nameof(GetVehicles), new { id = vehicle.Id }, responseDto);
        }

        // DELETE: api/Vehicle/5
        [HttpDelete("{id}")]
        [ValidateAntiForgeryToken] // <-- Add this to enforce the shield
        [Authorize(Roles = "Admin")] // Only users with the "Admin" role can delete vehicles
        public async Task<IActionResult> DeleteVehicle(long id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null)
            {
                return NotFound();
            }

            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool VehicleExists(long id)
        {
            return _context.Vehicles.Any(e => e.Id == id);
        }
    }
}
