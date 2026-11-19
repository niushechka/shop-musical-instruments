namespace course_project_shop_musical_instruments.Models;

public class CartItem
{
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int Quantity { get; set; }
    public decimal Subtotal => (Product?.Price ?? 0) * Quantity;
}

