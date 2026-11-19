using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.RegularExpressions;
using course_project_shop_musical_instruments.ViewModels;
using course_project_shop_musical_instruments.Services;

namespace course_project_shop_musical_instruments.Views;

public partial class AccountView : Window
{
    public AccountView()
    {
        InitializeComponent();
        
        var services = ServiceLocator.Instance;
        DataContext = new AccountViewModel(services.AuthService, services.OrderService, services.UserRepository, services.NavigationService);
        
        // Привязка паролей к ViewModel
        PasswordBox.PasswordChanged += (s, e) =>
        {
            if (DataContext is AccountViewModel vm)
            {
                vm.Password = PasswordBox.Password;
            }
        };
        
        ConfirmPasswordBox.PasswordChanged += (s, e) =>
        {
            if (DataContext is AccountViewModel vm)
            {
                vm.ConfirmPassword = ConfirmPasswordBox.Password;
            }
        };

        // Обновляем заказы при активации окна
        this.Activated += async (s, e) =>
        {
            if (DataContext is AccountViewModel vm)
            {
                await vm.RefreshOrdersAsync();
            }
        };

        // Обновляем заказы при получении фокуса окна
        this.GotFocus += async (s, e) =>
        {
            if (DataContext is AccountViewModel vm)
            {
                await vm.RefreshOrdersAsync();
            }
        };
    }

    private void PhoneEditTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        var regex = new Regex(@"^[\d\+]+$");
        e.Handled = !regex.IsMatch(e.Text);
    }

    private void PhoneEditTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
    {
        if (e.DataObject.GetDataPresent(typeof(string)))
        {
            string text = (string)e.DataObject.GetData(typeof(string));
            var regex = new Regex(@"[^\d\+]");
            string filteredText = regex.Replace(text, "");
            
            if (filteredText.Length > 13)
            {
                filteredText = filteredText.Substring(0, 13);
            }
            
            if (sender is TextBox textBox)
            {
                int selectionStart = textBox.SelectionStart;
                string currentText = textBox.Text ?? "";
                string newText = currentText.Substring(0, textBox.SelectionStart) + 
                                filteredText + 
                                currentText.Substring(textBox.SelectionStart + textBox.SelectionLength);
                
                if (newText.Length > 13)
                {
                    newText = newText.Substring(0, 13);
                }
                
                textBox.Text = newText;
                textBox.SelectionStart = Math.Min(selectionStart + filteredText.Length, 13);
                textBox.SelectionLength = 0;
            }
            
            e.CancelCommand();
        }
        else
        {
            e.CancelCommand();
        }
    }

    private void FirstNameEditTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        // Разрешаем только буквы (русские и английские, большие и маленькие)
        var regex = new Regex(@"^[а-яА-ЯёЁa-zA-Z]+$");
        e.Handled = !regex.IsMatch(e.Text);
    }

    private void FirstNameEditTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
    {
        if (e.DataObject.GetDataPresent(typeof(string)))
        {
            string text = (string)e.DataObject.GetData(typeof(string));
            // Фильтруем вставленный текст, оставляя только буквы
            var regex = new Regex(@"[^а-яА-ЯёЁa-zA-Z]");
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

    private void LastNameEditTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        // Разрешаем только буквы (русские и английские, большие и маленькие)
        var regex = new Regex(@"^[а-яА-ЯёЁa-zA-Z]+$");
        e.Handled = !regex.IsMatch(e.Text);
    }

    private void LastNameEditTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
    {
        if (e.DataObject.GetDataPresent(typeof(string)))
        {
            string text = (string)e.DataObject.GetData(typeof(string));
            // Фильтруем вставленный текст, оставляя только буквы
            var regex = new Regex(@"[^а-яА-ЯёЁa-zA-Z]");
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

    private void AddressEditTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        // Разрешаем только буквы, цифры, тире, запятую и точку
        var regex = new Regex(@"^[а-яА-ЯёЁa-zA-Z0-9\-,. ]+$");
        e.Handled = !regex.IsMatch(e.Text);
    }

    private void AddressEditTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
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

