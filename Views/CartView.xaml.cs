using System;
using System.Diagnostics;
using System.Windows;
using course_project_shop_musical_instruments.ViewModels;
using course_project_shop_musical_instruments.Services;

namespace course_project_shop_musical_instruments.Views;

public partial class CartView : Window
{
    public CartView()
    {
        Debug.WriteLine("CartView: Constructor started");
        try
        {
            InitializeComponent();
            Debug.WriteLine("CartView: Component initialized");
            
            var services = ServiceLocator.Instance;
            Debug.WriteLine("CartView: ServiceLocator obtained");
            
            // НЕ устанавливаем текущее окно здесь - это будет сделано в NavigationService
            Debug.WriteLine("CartView: Skipping SetCurrentWindow in constructor");
            
            DataContext = new CartViewModel(services.CartService, services.NavigationService);
            Debug.WriteLine("CartView: DataContext set");
            Debug.WriteLine("CartView: Constructor completed successfully");
        }
        catch (Exception ex)
        {
            var errorMsg = $"Ошибка при открытии корзины:\n{ex.Message}\n\nДетали:\n{ex.StackTrace}";
            Debug.WriteLine($"CartView ERROR: {errorMsg}");
            MessageBox.Show(errorMsg, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CartDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        // Сбрасываем выбор, чтобы строки нельзя было выбрать
        if (sender is System.Windows.Controls.DataGrid dataGrid)
        {
            dataGrid.SelectedItem = null;
        }
    }
}

