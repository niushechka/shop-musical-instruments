using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.RegularExpressions;
using course_project_shop_musical_instruments.ViewModels;
using course_project_shop_musical_instruments.Services;

namespace course_project_shop_musical_instruments.Views;

public partial class CheckoutView : Window
{
    public CheckoutView()
    {
        Debug.WriteLine("CheckoutView: Constructor started");
        try
        {
            InitializeComponent();
            Debug.WriteLine("CheckoutView: Component initialized");
            
            var services = ServiceLocator.Instance;
            Debug.WriteLine("CheckoutView: ServiceLocator obtained");
            
            // НЕ устанавливаем текущее окно здесь - это будет сделано в NavigationService
            Debug.WriteLine("CheckoutView: Skipping SetCurrentWindow in constructor");
            
            DataContext = new CheckoutViewModel(services.CartService, services.OrderService, services.AuthService, services.NavigationService);
            Debug.WriteLine("CheckoutView: DataContext set");
            Debug.WriteLine("CheckoutView: Constructor completed successfully");
        }
        catch (Exception ex)
        {
            var errorMsg = $"Ошибка при открытии оформления заказа:\n{ex.Message}\n\nДетали:\n{ex.StackTrace}";
            Debug.WriteLine($"CheckoutView ERROR: {errorMsg}");
            MessageBox.Show(errorMsg, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void DeliveryAddressTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        // Разрешаем только буквы, цифры, тире, запятую и точку
        var regex = new Regex(@"^[а-яА-ЯёЁa-zA-Z0-9\-,. ]+$");
        e.Handled = !regex.IsMatch(e.Text);
    }

    private void DeliveryAddressTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
    {
        if (e.DataObject.GetDataPresent(typeof(string)))
        {
            string text = (string)e.DataObject.GetData(typeof(string));
            // Фильтруем вставленный текст, оставляя только буквы, цифры, тире, запятую, точку и пробелы
            var regex = new Regex(@"[^а-яА-ЯёЁa-zA-Z0-9\-,. ]");
            string filteredText = regex.Replace(text, "");
            
            if (sender is TextBox textBox)
            {
                int selectionStart = textBox.SelectionStart;
                string currentText = textBox.Text ?? "";
                string newText = currentText.Substring(0, textBox.SelectionStart) + 
                                filteredText + 
                                currentText.Substring(textBox.SelectionStart + textBox.SelectionLength);
                
                textBox.Text = newText;
                textBox.SelectionStart = Math.Min(selectionStart + filteredText.Length, newText.Length);
                textBox.SelectionLength = 0;
            }
            
            e.CancelCommand();
        }
        else
        {
            e.CancelCommand();
        }
    }
}

