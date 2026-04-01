using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using TransportRoute.Core.Data;
using TransportRoute.Core.Models;
using TransportRouteApi.DTOs;

namespace TransportRouteApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TransitRoutesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TransitRoutesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/TransitRoutes
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDto<TransitRouteResponseDto>>> GetTransitRoutes(
            [FromQuery] string? keyword, 
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 12)
        {
            var query = _context.TransitRoutes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(r => r.RouteName.Contains(keyword) || 
                                        r.StartingPoint.Contains(keyword) || 
                                        r.Destination.Contains(keyword));
            }

            var totalCount = await query.CountAsync();

            var routes = await query
                .Include(route => route.Vehicles)
                    .ThenInclude(vehicle => vehicle.Category)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(route => new TransitRouteResponseDto
                {
                    Id = route.Id,
                    RouteName = route.RouteName,
                    StartingPoint = route.StartingPoint,
                    Destination = route.Destination,
                    StartingHour = route.StartingHour,
                    EndingHour = route.EndingHour,
                    Vehicles = route.Vehicles.Select(vehicle => new VehicleResponseDto
                    {
                        Id = vehicle.Id,
                        VehicleName = vehicle.VehicleName,
                        CategoryName = vehicle.Category != null ? vehicle.Category.CategoryName : string.Empty,
                        RouteName = route.RouteName,
                    }).ToList()
                })
                .ToListAsync();

            return Ok(new PaginatedResponseDto<TransitRouteResponseDto>
            {
                Items = routes,
                TotalCount = totalCount,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        }

        // GET: api/TransitRoutes/all
        [HttpGet("all")] 
        public async Task<ActionResult<IEnumerable<object>>> GetAllTransitRoutesForDropdown()
        {
            // We use .Select() to strip out heavy data and ONLY send exactly what the dropdown needs.
            // This dramatically reduces bandwidth and makes the API highly efficient!
            var dropdownData = await _context.TransitRoutes
                .Select(r => new 
                { 
                    id = r.Id, 
                    routeName = r.RouteName,
                    startingPoint = r.StartingPoint,
                    destination = r.Destination
                })
                .ToListAsync();

            return Ok(dropdownData);
        }

        // GET: api/TransitRoutes/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<TransitRouteResponseDto>> GetTransitRoute(long id)
        {
            var route = await _context.TransitRoutes
                .Include(r => r.Vehicles)
                    .ThenInclude(v => v.Category)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (route == null)
            {
                return NotFound();
            }

            var responseDto = new TransitRouteResponseDto
            {
                Id = route.Id,
                RouteName = route.RouteName,
                StartingPoint = route.StartingPoint,
                Destination = route.Destination,
                StartingHour = route.StartingHour,
                EndingHour = route.EndingHour,
                Vehicles = route.Vehicles.Select(vehicle => new VehicleResponseDto
                {
                    Id = vehicle.Id,
                    VehicleName = vehicle.VehicleName,
                    CategoryName = vehicle.Category != null ? vehicle.Category.CategoryName : string.Empty,
                    RouteName = route.RouteName,
                }).ToList()
            };

            return Ok(responseDto);
        }

        // Probably will be looked again, this won't be used for a couple of times
        // GET: api/TransitRoutes/search?keyword=cicaheum&pageNumber=1&pageSize=50
        [HttpGet("search")]
        public async Task<ActionResult<PaginatedResponseDto<TransitRouteResponseDto>>> SearchTransitRoutes(
            [FromQuery] string keyword, 
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 50)
        {
            // 1. Guard Clauses
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return BadRequest("Search keyword cannot be empty.");
            }

            if (pageNumber < 1)
            {
                return BadRequest("Page number must be 1 or greater.");
            }

            // 2. Validate Allowed Page Sizes
            int[] allowedPageSizes = { 10, 25, 50, 100, 200 };
            if (!allowedPageSizes.Contains(pageSize))
            {
                return BadRequest("Invalid page size. Allowed values are 10, 25, 50, 100, or 200.");
            }

            // 3. Build the base query (this doesn't hit the database yet)
            var query = _context.TransitRoutes
                .Where(r => r.RouteName.Contains(keyword) || 
                            r.StartingPoint.Contains(keyword) || 
                            r.Destination.Contains(keyword));

            // 4. Get the total count BEFORE paginating (needed for the frontend math)
            var totalCount = await query.CountAsync();

            // 5. Apply Pagination and fetch the specific slice of data
            var routes = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(route => new TransitRouteResponseDto
                {
                    Id = route.Id,
                    RouteName = route.RouteName,
                    StartingPoint = route.StartingPoint,
                    Destination = route.Destination,
                    StartingHour = route.StartingHour,
                    EndingHour = route.EndingHour
                })
                .ToListAsync();

            // 6. Wrap it all up in our new generic DTO
            var response = new PaginatedResponseDto<TransitRouteResponseDto>
            {
                Items = routes,
                TotalCount = totalCount,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                // Math.Ceiling ensures that if you have 51 items with a page size of 50, you get 2 pages instead of 1
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize) 
            };

            return Ok(response);
        }

        // POST: api/TransitRoutes
        [HttpPost]
        [ValidateAntiForgeryToken] // <-- Add this to enforce the shield
        public async Task<ActionResult<TransitRouteResponseDto>> PostTransitRoute(CreateTransitRouteDto createDto)
        {
            var routeEntity = new TransitRoute
            {
                RouteName = createDto.RouteName,
                StartingPoint = createDto.StartingPoint,
                Destination = createDto.Destination,
                StartingHour = createDto.StartingHour,
                EndingHour = createDto.EndingHour
            };

            _context.TransitRoutes.Add(routeEntity);
            await _context.SaveChangesAsync();

            var responseDto = new TransitRouteResponseDto
            {
                Id = routeEntity.Id,
                RouteName = routeEntity.RouteName,
                StartingPoint = routeEntity.StartingPoint,
                Destination = routeEntity.Destination,
                StartingHour = routeEntity.StartingHour,
                EndingHour = routeEntity.EndingHour
            };

            return CreatedAtAction(nameof(GetTransitRoute), new { id = routeEntity.Id }, responseDto);
        }

        // PUT: api/TransitRoutes/5
        [HttpPut("{id}")]
        [ValidateAntiForgeryToken] // <-- Add this to enforce the shield
        public async Task<IActionResult> PutTransitRoute(long id, CreateTransitRouteDto updateDto)
        {
            var routeEntity = await _context.TransitRoutes.FindAsync(id);
            if (routeEntity == null)
            {
                return NotFound();
            }

            routeEntity.RouteName = updateDto.RouteName;
            routeEntity.StartingPoint = updateDto.StartingPoint;
            routeEntity.Destination = updateDto.Destination;
            routeEntity.StartingHour = updateDto.StartingHour;
            routeEntity.EndingHour = updateDto.EndingHour;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/TransitRoutes/5
        [HttpDelete("{id}")]
        [ValidateAntiForgeryToken] // <-- Add this to enforce the shield
        [Authorize(Roles = "Admin")] // Only users with the "Admin" role can delete transit routes
        public async Task<IActionResult> DeleteTransitRoute(long id)
        {
            var routeEntity = await _context.TransitRoutes.FindAsync(id);
            if (routeEntity == null)
            {
                return NotFound();
            }

            _context.TransitRoutes.Remove(routeEntity);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TransitRouteExists(long id)
        {
            return _context.TransitRoutes.Any(e => e.Id == id);
        }
    }
}