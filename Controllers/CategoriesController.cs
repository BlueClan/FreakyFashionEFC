using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FreakyFashion.Data;
using FreakyFashion.Dtos;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _context;

    public CategoriesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetCategories()
    {
        var categories = await _context.Categories
            .Include(c => c.Products)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Image = c.Image,
                Slug = c.Slug,
                Products = c.Products.Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Image = p.Image,
                    UrlSlug = p.UrlSlug
                }).ToList()
            })
            .ToListAsync();
        return Ok(categories);
    }

    
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryDto>> GetCategoryById(int id)
    {
        var category = await _context.Categories
            .Include(c => c.Products)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Image = c.Image,
                Slug = c.Slug,
                Products = c.Products.Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Image = p.Image,
                    UrlSlug = p.UrlSlug
                }).ToList()
            })
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
            return NotFound();

        return Ok(category);
    }

    
    [HttpGet("by-slug")]
    public async Task<ActionResult<List<CategoryDto>>> GetCategoryBySlug([FromQuery] string? slug)
    {
        if (string.IsNullOrEmpty(slug))
            return Ok(new List<CategoryDto>());

        var categories = await _context.Categories
            .Include(c => c.Products)
            .Where(c => c.Slug == slug)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Image = c.Image,
                Slug = c.Slug,
                Products = c.Products.Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Image = p.Image,
                    UrlSlug = p.UrlSlug
                }).ToList()
            })
            .ToListAsync();

        return Ok(categories);
    }

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> CreateCategory(CreateCategoryDto dto)
    {
        if (string.IsNullOrEmpty(dto.Name))
            return BadRequest(new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                title = "One or more validation errors occurred.",
                status = 400,
                errors = new { Name = new[] { "Name is required." } },
                traceId = HttpContext.TraceIdentifier
            });

        var category = new Category
        {
            Name = dto.Name,
            Image = dto.Image,
            Slug = NormalizeSlug(dto.Name)
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var categoryDto = new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Image = category.Image,
            Slug = category.Slug,
            Products = new List<ProductDto>()
        };

        return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, categoryDto);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
            return NotFound();

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private string NormalizeSlug(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        var slug = input.ToLower().Replace(" ", "-");

        slug = Regex.Replace(slug, "[åä]", "a");
        slug = Regex.Replace(slug, "[öø]", "o");
        slug = Regex.Replace(slug, "[éèê]", "e");
        slug = Regex.Replace(slug, "[ü]", "u");

        slug = Regex.Replace(slug, "[^a-z0-9-]", "");

        return slug;
    }
} 