using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.RegularExpressions;
using course_project_shop_musical_instruments.ViewModels;
using course_project_shop_musical_instruments.Services;

namespace course_project_shop_musical_instruments.Views;

public partial class LoginView : Window
{
    public LoginView()
    {
        InitializeComponent();
        
        var services = ServiceLocator.Instance;
        // НЕ устанавливаем текущее окно здесь - это будет сделано в NavigationService
        // services.NavigationService.SetCurrentWindow(this);
        
        DataContext = new LoginViewModel(services.AuthService, services.NavigationService);
        
        PasswordBox.PasswordChanged += (s, e) =>
        {
            if (DataContext is LoginViewModel vm)
            {
                vm.Password = PasswordBox.Password;
            }
        };
    }

    private void PhoneTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        // Разрешаем только цифры и символ +
        var regex = new Regex(@"^[\d\+]+$");
        e.Handled = !regex.IsMatch(e.Text);
    }

    private void PhoneTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
    {
        if (e.DataObject.GetDataPresent(typeof(string)))
        {
            string text = (string)e.DataObject.GetData(typeof(string));
            // Фильтруем вставленный текст, оставляя только цифры и +
            var regex = new Regex(@"[^\d\+]");
            string filteredText = regex.Replace(text, "");
            
            // Ограничиваем длину до 13 символов
            if (filteredText.Length > 13)
            {
                filteredText = filteredText.Substring(0, 13);
            }
            
            // Устанавливаем отфильтрованный текст
            if (sender is TextBox textBox)
            {
                int selectionStart = textBox.SelectionStart;
                string currentText = textBox.Text ?? "";
                string newText = currentText.Substring(0, textBox.SelectionStart) + 
                                filteredText + 
                                currentText.Substring(textBox.SelectionStart + textBox.SelectionLength);
                
                // Ограничиваем общую длину
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

    private void FirstNameTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        // Разрешаем только буквы (русские и английские, большие и маленькие)
        var regex = new Regex(@"^[а-яА-ЯёЁa-zA-Z]+$");
        e.Handled = !regex.IsMatch(e.Text);
    }

    private void FirstNameTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
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

    private void LastNameTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        // Разрешаем только буквы (русские и английские, большие и маленькие)
        var regex = new Regex(@"^[а-яА-ЯёЁa-zA-Z]+$");
        e.Handled = !regex.IsMatch(e.Text);
    }

    private void LastNameTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
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

    private void AddressTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        // Разрешаем только буквы, цифры, тире, запятую и точку
        var regex = new Regex(@"^[а-яА-ЯёЁa-zA-Z0-9\-,. ]+$");
        e.Handled = !regex.IsMatch(e.Text);
    }

    private void AddressTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
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

