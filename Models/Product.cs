using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace course_project_shop_musical_instruments.Models;

[Table("Products")]
public class Product
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }
    
    [Required]
    public int StockQuantity { get; set; }
    
    [Required]
    public int CategoryId { get; set; }
    
    [ForeignKey("CategoryId")]
    public virtual Category? Category { get; set; }
    
    [Required]
    public int ManufacturerId { get; set; }
    
    [ForeignKey("ManufacturerId")]
    public virtual Manufacturer? Manufacturer { get; set; }
    
    [MaxLength(50)]
    public string ArticleNumber { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string ImagePath { get; set; } = string.Empty; // Для обратной совместимости
    
    public byte[]? ImageData { get; set; } // BLOB для хранения изображения
    
    [Required]
    public bool IsAvailable { get; set; } = true;
    
    [Required]
    public bool IsDeleted { get; set; } = false;
    
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    
    // Свойства для обратной совместимости
    [NotMapped]
    public string CategoryName => Category?.Name ?? string.Empty;
    
    [NotMapped]
    public string ManufacturerName => Manufacturer?.Name ?? string.Empty;
}

