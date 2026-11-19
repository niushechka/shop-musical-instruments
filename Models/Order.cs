using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace course_project_shop_musical_instruments.Models;

[Table("Orders")]
public class Order
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Required]
    public int UserId { get; set; }
    
    [ForeignKey("UserId")]
    public virtual User? User { get; set; }
    
    [Required]
    public DateTime OrderDate { get; set; } = DateTime.Now;
    
    [Required]
    [Column(TypeName = "date")]
    public DateTime DeliveryDate { get; set; }
    
    [Required]
    [Column(TypeName = "time")]
    public TimeSpan DeliveryTime { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }
    
    [Required]
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    
    [Required]
    [MaxLength(500)]
    public string DeliveryAddress { get; set; } = string.Empty;
    
    public string? Notes { get; set; }
    
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}

public enum OrderStatus
{
    Pending,
    Confirmed,
    Processing,
    Shipped,
    Delivered,
    Cancelled
}

