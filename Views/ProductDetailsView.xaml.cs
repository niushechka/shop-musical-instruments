using System;
using System.Windows;
using course_project_shop_musical_instruments.ViewModels;
using course_project_shop_musical_instruments.Services;
using course_project_shop_musical_instruments.Models;

namespace course_project_shop_musical_instruments.Views;

public partial class ProductDetailsView : Window
{
    public ProductDetailsView(Product product)
    {
        InitializeComponent();
        
        var services = ServiceLocator.Instance;
        DataContext = new ProductDetailsViewModel(
            product, 
            services.ProductService, 
            services.CartService, 
            services.NavigationService,
            () => { DialogResult = true; Close(); });
    }
}

