using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransportRouteApi.Data;
using TransportRouteApi.Models;
using TransportRouteApi.DTOs;

namespace TransportRouteApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransitRoutesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TransitRoutesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/TransitRoutes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TransitRouteResponseDto>>> GetTransitRoutes()
        {
            var routes = await _context.TransitRoutes
                .Include(route => route.Vehicles)
                .ThenInclude(vehicle => vehicle.Category)
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
                        CategoryName = vehicle.Category.CategoryName,
                        RouteName = route.RouteName,
                    }).ToList()
                })
                .ToListAsync();

            return Ok(routes);
        }

        // GET: api/TransitRoutes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TransitRouteResponseDto>> GetTransitRoute(long id)
        {
            var route = await _context.TransitRoutes.FindAsync(id);

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
                EndingHour = route.EndingHour
            };

            return Ok(responseDto);
        }

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