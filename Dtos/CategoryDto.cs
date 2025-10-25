namespace FreakyFashion.Dtos;
public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public List<ProductDto> Products { get; set; } = new();
}