using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Text.RegularExpressions;
using System.Linq;
using System.Windows.Data;
using course_project_shop_musical_instruments.ViewModels;
using course_project_shop_musical_instruments.Services;
using course_project_shop_musical_instruments.Models;

namespace course_project_shop_musical_instruments.Views;

public partial class CatalogView : Window
{
    public CatalogView()
    {
        Debug.WriteLine("CatalogView: Constructor started");
        
        try
        {
            Debug.WriteLine("CatalogView: Initializing component");
            InitializeComponent();
            Debug.WriteLine("CatalogView: Component initialized");
            
            var services = ServiceLocator.Instance;
            Debug.WriteLine("CatalogView: ServiceLocator obtained");
            
            // Устанавливаем текущее окно в NavigationService
            services.NavigationService.SetCurrentWindow(this);
            Debug.WriteLine("CatalogView: SetCurrentWindow called");
            
            // Создаем ViewModel после инициализации компонентов
            Debug.WriteLine("CatalogView: Creating ViewModel");
            DataContext = new CatalogViewModel(services.ProductService, services.CartService, services.AuthService, services.NavigationService);
            Debug.WriteLine("CatalogView: ViewModel created");
            
            // Убеждаемся, что окно не закрывается автоматически
            this.Loaded += async (s, e) =>
            {
                try
                {
                    Debug.WriteLine("CatalogView: Loaded event fired");
                    this.Activate();
                    this.Focus();
                    this.Show();
                    Debug.WriteLine("CatalogView: Window activated and shown");
                    
                    // Загружаем товары после полной загрузки окна
                    if (DataContext is CatalogViewModel viewModel)
                    {
                        Debug.WriteLine("CatalogView: Loading products after window loaded");
                        viewModel.UpdateAuthenticationStatus();
                        await viewModel.LoadProductsAsync();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"CatalogView Loaded ERROR: {ex.Message}\n{ex.StackTrace}");
                    MessageBox.Show(
                        $"Ошибка при загрузке каталога:\n{ex.Message}",
                        "Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            };

            // Обновляем статус авторизации при активации окна
            this.Activated += (s, e) =>
            {
                if (DataContext is CatalogViewModel viewModel)
                {
                    viewModel.UpdateAuthenticationStatus();
                }
            };
            
            // Обработчик Closing будет установлен в NavigationService
            
            // Предотвращаем закрытие окна при ошибках
            this.Unloaded += (s, e) =>
            {
                Debug.WriteLine("CatalogView: Unloaded event fired");
            };
            
            Debug.WriteLine("CatalogView: Constructor completed successfully");
        }
        catch (Exception ex)
        {
            var errorMsg = $"Ошибка при открытии каталога:\n{ex.Message}\n\nДетали:\n{ex.StackTrace}";
            Debug.WriteLine($"CatalogView ERROR: {errorMsg}");
            
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

    private void ProductCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element && element.DataContext is Product product)
        {
            if (DataContext is CatalogViewModel viewModel)
            {
                viewModel.ViewProductDetailsCommand.Execute(product);
            }
        }
    }

    private void PriceTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (sender is System.Windows.Controls.TextBox textBox)
        {
            // Разрешаем только цифры и точку
            // Получаем текущий текст и позицию курсора
            string currentText = textBox.Text ?? string.Empty;
            int selectionStart = textBox.SelectionStart;
            int selectionLength = textBox.SelectionLength;
            
            // Формируем новый текст с учетом выделения
            string newText = currentText.Substring(0, selectionStart) + 
                           e.Text + 
                           currentText.Substring(selectionStart + selectionLength);
            
            // Регулярное выражение: только цифры и одна точка
            Regex regex = new Regex(@"^[0-9]*\.?[0-9]*$");
            
            if (!regex.IsMatch(newText))
            {
                e.Handled = true;
                return;
            }
            
            // Проверяем, что точка не дублируется
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
}

