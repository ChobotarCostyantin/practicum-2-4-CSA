namespace Nimble.Modulith.Products.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DateCreated { get; set; }
    public string CreatedByUser { get; set; } = string.Empty;
}