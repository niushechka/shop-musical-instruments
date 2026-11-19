using course_project_shop_musical_instruments.Models;

namespace course_project_shop_musical_instruments.Services;

public class CartService
{
    private readonly List<CartItem> _cartItems = new();

    public List<CartItem> GetCartItems() => _cartItems;

    public void AddToCart(Product product, int quantity = 1)
    {
        var existingItem = _cartItems.FirstOrDefault(item => item.ProductId == product.Id);
        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            _cartItems.Add(new CartItem
            {
                ProductId = product.Id,
                Product = product,
                Quantity = quantity
            });
        }
    }

    public void RemoveFromCart(int productId)
    {
        var item = _cartItems.FirstOrDefault(item => item.ProductId == productId);
        if (item != null)
        {
            _cartItems.Remove(item);
        }
    }

    public void UpdateQuantity(int productId, int quantity)
    {
        var item = _cartItems.FirstOrDefault(item => item.ProductId == productId);
        if (item != null)
        {
            if (quantity <= 0)
            {
                _cartItems.Remove(item);
            }
            else
            {
                item.Quantity = quantity;
            }
        }
    }

    public void ClearCart()
    {
        _cartItems.Clear();
    }

    public decimal GetTotal()
    {
        return _cartItems.Sum(item => item.Subtotal);
    }

    public int GetItemCount()
    {
        return _cartItems.Sum(item => item.Quantity);
    }
}

