using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using course_project_shop_musical_instruments.ViewModels;
using course_project_shop_musical_instruments.Services;

namespace course_project_shop_musical_instruments.Views;

public partial class AdminMainView : Window
{
    public AdminMainView()
    {
        Debug.WriteLine("AdminMainView: Constructor started");
        
        try
        {
            Debug.WriteLine("AdminMainView: Initializing component");
            InitializeComponent();
            Debug.WriteLine("AdminMainView: Component initialized");
            
            var services = ServiceLocator.Instance;
            Debug.WriteLine("AdminMainView: ServiceLocator obtained");
            
            // НЕ устанавливаем текущее окно здесь - это будет сделано в NavigationService
            // services.NavigationService.SetCurrentWindow(this);
            Debug.WriteLine("AdminMainView: Skipping SetCurrentWindow in constructor");
            
            Debug.WriteLine("AdminMainView: Creating ViewModels");
            var adminProductsViewModel = new AdminProductsViewModel(services.ProductService, services.NavigationService);
            Debug.WriteLine("AdminMainView: AdminProductsViewModel created");
            
            var adminOrdersViewModel = new AdminOrdersViewModel(services.OrderService, services.NavigationService);
            Debug.WriteLine("AdminMainView: AdminOrdersViewModel created");
            
            var adminCustomersViewModel = new AdminCustomersViewModel(services.UserRepository, services.OrderService, services.NavigationService);
            Debug.WriteLine("AdminMainView: AdminCustomersViewModel created");
            
            DataContext = new
            {
                AdminProductsViewModel = adminProductsViewModel,
                AdminOrdersViewModel = adminOrdersViewModel,
                AdminCustomersViewModel = adminCustomersViewModel,
                NavigateToProfileCommand = new CommunityToolkit.Mvvm.Input.RelayCommand(() =>
                {
                    services.NavigationService.NavigateToAdminProfile();
                }),
                LogoutCommand = new CommunityToolkit.Mvvm.Input.RelayCommand(() =>
                {
                    services.AuthService.Logout();
                    services.NavigationService.NavigateToLogin();
                })
            };
            Debug.WriteLine("AdminMainView: DataContext set");
            
            // Убеждаемся, что окно не закрывается автоматически
            this.Loaded += async (s, e) =>
            {
                Debug.WriteLine("AdminMainView: Loaded event fired");
                this.Activate();
                this.Focus();
                this.Show();
                Debug.WriteLine("AdminMainView: Window activated and shown");
                
                // Загружаем товары после полной загрузки окна
                if (DataContext != null)
                {
                    var contextType = DataContext.GetType();
                    var property = contextType.GetProperty("AdminProductsViewModel");
                    if (property?.GetValue(DataContext) is AdminProductsViewModel viewModel)
                    {
                        Debug.WriteLine("AdminMainView: Loading products after window loaded");
                        await viewModel.LoadProductsAsync();
                    }
                }
            };
            
            // Обновляем товары при активации окна (как в CatalogView)
            this.Activated += async (s, e) =>
            {
                if (DataContext != null)
                {
                    var contextType = DataContext.GetType();
                    var property = contextType.GetProperty("AdminProductsViewModel");
                    if (property?.GetValue(DataContext) is AdminProductsViewModel viewModel)
                    {
                        Debug.WriteLine("AdminMainView: Window activated, reloading products");
                        await viewModel.LoadProductsAsync();
                    }
                }
            };
            
            // Обработчик Closing будет установлен в NavigationService
            
            Debug.WriteLine("AdminMainView: Constructor completed successfully");
        }
        catch (Exception ex)
        {
            var errorMsg = $"Ошибка при открытии админ-панели:\n{ex.Message}\n\nДетали:\n{ex.StackTrace}";
            Debug.WriteLine($"AdminMainView ERROR: {errorMsg}");
            
            MessageBox.Show(
                errorMsg,
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            
            // Не завершаем приложение сразу, даем пользователю возможность увидеть ошибку
            this.Closing += (s, e) => 
            {
                if (Application.Current != null)
                {
                    Application.Current.Shutdown();
                }
            };
        }
    }

    private void ImageDropArea_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            try
            {
                // Получаем ViewModel через рефлексию
                var contextType = DataContext?.GetType();
                if (contextType != null)
                {
                    var property = contextType.GetProperty("AdminProductsViewModel");
                    if (property?.GetValue(DataContext) is AdminProductsViewModel viewModel)
                    {
                        viewModel.HandleDrop(files);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error handling drop: {ex.Message}");
            }
        }
    }

    private void ImageDropArea_DragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.Copy;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
        e.Handled = true;
    }

    private void ImageDropArea_DragLeave(object sender, DragEventArgs e)
    {
        // Можно добавить визуальную обратную связь
    }

    private void PriceTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (sender is System.Windows.Controls.TextBox textBox)
        {
            string currentText = textBox.Text ?? string.Empty;
            int selectionStart = textBox.SelectionStart;
            int selectionLength = textBox.SelectionLength;
            string newText = currentText.Substring(0, selectionStart) + e.Text + currentText.Substring(selectionStart + selectionLength);

            Regex regex = new Regex(@"^[0-9]*\.?[0-9]*$");
            if (!regex.IsMatch(newText))
            {
                e.Handled = true;
                return;
            }

            int dotCount = newText.Count(c => c == '.');
            if (dotCount > 1)
            {
                e.Handled = true;
            }
        }
    }

    private void PriceTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
    {
        if (e.DataObject.GetDataPresent(typeof(string)))
        {
            string pastedText = (string)e.DataObject.GetData(typeof(string));
            Regex regex = new Regex(@"^[0-9]*\.?[0-9]*$");
            if (!regex.IsMatch(pastedText))
            {
                e.CancelCommand();
            }
        }
        else
        {
            e.CancelCommand();
        }
    }

    private void DataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        // Сбрасываем выбор, чтобы строки нельзя было выбрать
        if (sender is System.Windows.Controls.DataGrid dataGrid)
        {
            dataGrid.SelectedItem = null;
        }
    }

    private void DataGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        // Передаем событие прокрутки в ScrollViewer
        if (ProductsScrollViewer != null)
        {
            var offset = ProductsScrollViewer.VerticalOffset - (e.Delta / 3.0);
            ProductsScrollViewer.ScrollToVerticalOffset(Math.Max(0, Math.Min(offset, ProductsScrollViewer.ScrollableHeight)));
            e.Handled = true;
        }
    }

    private void Card_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        // Передаем событие прокрутки в ScrollViewer
        if (ProductsScrollViewer != null)
        {
            var offset = ProductsScrollViewer.VerticalOffset - (e.Delta / 3.0);
            ProductsScrollViewer.ScrollToVerticalOffset(Math.Max(0, Math.Min(offset, ProductsScrollViewer.ScrollableHeight)));
            e.Handled = true;
        }
    }
}

