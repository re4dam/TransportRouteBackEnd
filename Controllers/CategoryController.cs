using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransportRoute.Core.Data;
using TransportRoute.Core.Models;
using TransportRouteApi.DTOs;

namespace TransportRouteApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoryController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Category
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryResponseDto>>> GetCategories()
        {
            var categories = await _context.Categories
                .Include(category => category.Vehicles)
                    .ThenInclude(vehicle => vehicle.TransitRoute)
                .Select(category => new CategoryResponseDto
                {
                    Id = category.Id,
                    CategoryName = category.CategoryName,
                    Vehicles = category.Vehicles.Select(vehicle => new VehicleResponseDto
                    {
                        Id = vehicle.Id,
                        VehicleName = vehicle.VehicleName,
                        CategoryName = category.CategoryName,
                        RouteName = vehicle.TransitRoute.RouteName
                    }).ToList()
                })
                .ToListAsync();

            // FIXED: Return the mapped DTOs, not the raw database context!
            return Ok(categories);
        }

        // GET: api/Category/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryResponseDto>> GetCategory(long id)
        {
            // We use .Where() to find the specific ID before mapping to the DTO
            var category = await _context.Categories
                .Include(c => c.Vehicles)
                    .ThenInclude(v => v.TransitRoute)
                .Where(c => c.Id == id)
                .Select(c => new CategoryResponseDto
                {
                    Id = c.Id,
                    CategoryName = c.CategoryName,
                    Vehicles = c.Vehicles.Select(v => new VehicleResponseDto
                    {
                        Id = v.Id,
                        VehicleName = v.VehicleName,
                        CategoryName = c.CategoryName,
                        RouteName = v.TransitRoute.RouteName
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (category == null)
            {
                return NotFound();
            }

            return Ok(category);
        }

        // POST: api/Category
        [HttpPost]
        public async Task<ActionResult<CreateCategoryDto>> PostCategory(CreateCategoryDto categoryDto)
        {
            // 1. Map the incoming DTO to a raw Entity so EF Core can save it
            var category = new Category
            {
                CategoryName = categoryDto.CategoryName
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            // 2. Return a 201 Created status, pointing to the GetCategory endpoint
            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, new CreateCategoryDto 
            {
                CategoryName = category.CategoryName,
                // Vehicles = new List<VehicleResponseDto>() // Brand new category has no vehicles yet
            });
        }

        // PUT: api/Category/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory(long id, CreateCategoryDto categoryDto)
        {
            // 1. Find the existing entity in the database
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            // 2. Update only the allowed fields from the DTO
            category.CategoryName = categoryDto.CategoryName;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(id))
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

        // DELETE: api/Category/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(long id)
        {
            // Delete methods usually don't need DTOs since they just take an ID
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CategoryExists(long id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}