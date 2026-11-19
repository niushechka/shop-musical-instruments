using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace course_project_shop_musical_instruments.Models;

[Table("Categories")]
public class Category
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public virtual ICollection<Manufacturer> Manufacturers { get; set; } = new List<Manufacturer>();
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}

