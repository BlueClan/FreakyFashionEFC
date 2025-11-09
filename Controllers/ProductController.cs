using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FreakyFashion.Data;
using FreakyFashion.Dtos;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProductDto>>> GetProducts()
    {
        var products = await _context.Products
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Image = p.Image,
                UrlSlug = p.UrlSlug
            })
            .ToListAsync();
        return Ok(products);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetProductById(int id)
    {
        var product = await _context.Products
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Image = p.Image,
                UrlSlug = p.UrlSlug
            })
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
            return NotFound();

        return Ok(product);
    }

    [HttpGet("by-slug")]
    public async Task<ActionResult<List<ProductDto>>> GetProductBySlug([FromQuery] string? slug)
    {
        if (string.IsNullOrEmpty(slug))
            return Ok(new List<ProductDto>());

        var products = await _context.Products
            .Where(p => p.UrlSlug == slug)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Image = p.Image,
                UrlSlug = p.UrlSlug
            })
            .ToListAsync();

        return Ok(products);
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductDto dto)
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

        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Image = dto.Image,
            UrlSlug = NormalizeSlug(dto.Name),
            Categories = await _context.Categories
                .Where(c => dto.Categories.Contains(c.Id))
                .ToListAsync()
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var productDto = new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,s
            Image = product.Image,
            UrlSlug = product.UrlSlug
        };

        return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, productDto);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return NotFound();

        _context.Products.Remove(product);
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