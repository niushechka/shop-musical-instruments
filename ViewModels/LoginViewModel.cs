using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using course_project_shop_musical_instruments.Services;
using System;
using System.Diagnostics;
using System.Windows;

namespace course_project_shop_musical_instruments.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly AuthService _authService;
    private readonly NavigationService _navigationService;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private string firstName = string.Empty;

    [ObservableProperty]
    private string lastName = string.Empty;

    [ObservableProperty]
    private string phone = string.Empty;

    [ObservableProperty]
    private string address = string.Empty;

    [ObservableProperty]
    private bool isLoginMode = true;

    public LoginViewModel(AuthService authService, NavigationService navigationService)
    {
        _authService = authService;
        _navigationService = navigationService;
    }

    [RelayCommand]
    private async Task Login()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("Login: Starting login process");
            
            if (IsLoginMode)
            {
                if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
                {
                    MessageBox.Show("Пожалуйста, введите email и пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"Login: Attempting login for {Email}");
                var success = await _authService.LoginAsync(Email, Password);
                
                if (success)
                {
                    System.Diagnostics.Debug.WriteLine($"Login: Success, IsAdmin={_authService.IsAdmin}");
                    if (_authService.IsAdmin)
                    {
                        System.Diagnostics.Debug.WriteLine("Login: Navigating to Admin");
                        _navigationService.NavigateToAdmin();
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Login: Navigating to Catalog");
                        _navigationService.NavigateToCatalog();
                    }
                    System.Diagnostics.Debug.WriteLine("Login: Navigation completed");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Login: Failed - invalid credentials");
                    MessageBox.Show("Неверный email или пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                await Register();
            }
        }
        catch (Exception ex)
        {
            var errorMsg = $"Ошибка при входе:\n{ex.Message}\n\n{ex.StackTrace}";
            System.Diagnostics.Debug.WriteLine($"Login ERROR: {errorMsg}");
            MessageBox.Show(errorMsg, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task Register()
    {
        try
        {
            // Проверяем все поля как обязательные
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName) ||
                string.IsNullOrWhiteSpace(Phone) || string.IsNullOrWhiteSpace(Address))
            {
                MessageBox.Show("Пожалуйста, заполните все поля", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var success = await _authService.RegisterAsync(Email, Password, FirstName, LastName, Phone, Address);
            if (success)
            {
                MessageBox.Show("Регистрация успешна!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                _navigationService.NavigateToCatalog();
            }
            else
            {
                MessageBox.Show("Пользователь с таким email уже существует", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при регистрации:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void ToggleMode()
    {
        IsLoginMode = !IsLoginMode;
    }
}

