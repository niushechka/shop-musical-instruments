using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace course_project_shop_musical_instruments.Models;

[Table("Manufacturers")]
public class Manufacturer
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public int CategoryId { get; set; }
    
    [ForeignKey("CategoryId")]
    public virtual Category? Category { get; set; }
    
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}

