using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.RegularExpressions;
using course_project_shop_musical_instruments.ViewModels;
using course_project_shop_musical_instruments.Services;

namespace course_project_shop_musical_instruments.Views;

public partial class AdminProfileView : Window
{
    public AdminProfileView()
    {
        InitializeComponent();
        
        var services = ServiceLocator.Instance;
        DataContext = new AdminProfileViewModel(services.AuthService, services.UserRepository, services.NavigationService);
        
        // Привязка паролей к ViewModel
        PasswordBox.PasswordChanged += (s, e) =>
        {
            if (DataContext is AdminProfileViewModel vm)
            {
                vm.Password = PasswordBox.Password;
            }
        };
        
        ConfirmPasswordBox.PasswordChanged += (s, e) =>
        {
            if (DataContext is AdminProfileViewModel vm)
            {
                vm.ConfirmPassword = ConfirmPasswordBox.Password;
            }
        };
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

}

