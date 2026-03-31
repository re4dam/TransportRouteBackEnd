using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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
    public class CategoryController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoryController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Category
        [HttpGet]
        [AllowAnonymous] 
        public async Task<ActionResult<PaginatedResponseDto<CategoryResponseDto>>> GetCategories(
            [FromQuery] string? keyword, 
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 12)
        {
            var query = _context.Categories.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(c => c.CategoryName.Contains(keyword));
            }

            var totalCount = await query.CountAsync();

            var categories = await query
                .OrderBy(c => c.Id)
                .Include(category => category.Vehicles)
                    .ThenInclude(vehicle => vehicle.TransitRoute)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
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

            return Ok(new PaginatedResponseDto<CategoryResponseDto>
            {
                Items = categories,
                TotalCount = totalCount,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        }

        // GET: api/Category/all
        [HttpGet("all")] 
        public async Task<ActionResult<IEnumerable<object>>> GetAllCategoriesForDropdown()
        {
            // We use .Select() to strip out heavy data and ONLY send exactly what the dropdown needs.
            // This dramatically reduces bandwidth and makes the API highly efficient!
            var dropdownData = await _context.Categories
                .Select(c => new 
                { 
                    id = c.Id, 
                    categoryName = c.CategoryName 
                })
                .ToListAsync();

            return Ok(dropdownData);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategory(long id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            return category; 
        }

        // POST: api/Category
        [HttpPost]
        [ValidateAntiForgeryToken] // <-- Add this to enforce the shield
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
            return CreatedAtAction(nameof(GetCategories), new { id = category.Id }, new CreateCategoryDto 
            {
                CategoryName = category.CategoryName,
                // Vehicles = new List<VehicleResponseDto>() // Brand new category has no vehicles yet
            });
        }

        // PUT: api/Category/5
        [HttpPut("{id}")]
        [ValidateAntiForgeryToken] // <-- Add this to enforce the shield
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
        [ValidateAntiForgeryToken] // <-- Add this to enforce the shield
        [Authorize(Roles = "Admin")] // Only users with the "Admin" role can delete categories
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