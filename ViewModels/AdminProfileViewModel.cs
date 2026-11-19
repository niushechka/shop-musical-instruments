using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using course_project_shop_musical_instruments.Models;
using course_project_shop_musical_instruments.Services;
using course_project_shop_musical_instruments.Data;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace course_project_shop_musical_instruments.ViewModels;

public partial class AdminProfileViewModel : BaseViewModel
{
    private readonly AuthService _authService;
    private readonly UserRepository _userRepository;
    private readonly NavigationService _navigationService;

    [ObservableProperty]
    private bool isEditMode;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string firstName = string.Empty;

    [ObservableProperty]
    private string lastName = string.Empty;

    [ObservableProperty]
    private string phone = string.Empty;

    [ObservableProperty]
    private string address = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private string confirmPassword = string.Empty;

    public User? CurrentUser => _authService.CurrentUser;

    public AdminProfileViewModel(AuthService authService, UserRepository userRepository, NavigationService navigationService)
    {
        _authService = authService;
        _userRepository = userRepository;
        _navigationService = navigationService;
        
        LoadUserData();
    }

    private void LoadUserData()
    {
        if (CurrentUser != null)
        {
            Email = CurrentUser.Email;
            FirstName = CurrentUser.FirstName;
        }
    }

    [RelayCommand]
    private void EditProfile()
    {
        IsEditMode = true;
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditMode = false;
        LoadUserData();
        Password = string.Empty;
        ConfirmPassword = string.Empty;
    }

    [RelayCommand]
    private async Task SaveProfile()
    {
        if (CurrentUser == null)
        {
            MessageBox.Show("Пользователь не авторизован", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        // Валидация обязательных полей
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(FirstName))
        {
            MessageBox.Show("Пожалуйста, заполните все обязательные поля (Email, Имя)", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Проверка email на уникальность (если изменился)
        if (Email != CurrentUser.Email)
        {
            var existingUser = await _userRepository.GetByEmailAsync(Email);
            if (existingUser != null)
            {
                MessageBox.Show("Пользователь с таким email уже существует", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        // Проверка пароля, если он указан
        if (!string.IsNullOrWhiteSpace(Password))
        {
            if (Password != ConfirmPassword)
            {
                MessageBox.Show("Пароли не совпадают", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Password.Length < 3)
            {
                MessageBox.Show("Пароль должен содержать минимум 3 символа", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        try
        {
            // Обновляем данные пользователя
            CurrentUser.Email = Email;
            CurrentUser.FirstName = FirstName;

            // Обновляем пароль, если он указан
            if (!string.IsNullOrWhiteSpace(Password))
            {
                CurrentUser.PasswordHash = Password; // В продакшене нужно хешировать пароль
            }

            var success = await _userRepository.UpdateAsync(CurrentUser);
            
            if (success)
            {
                // Обновляем текущего пользователя в AuthService
                await _authService.UpdateCurrentUserAsync();
                
                MessageBox.Show("Данные успешно обновлены", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                IsEditMode = false;
                Password = string.Empty;
                ConfirmPassword = string.Empty;
                
                // Обновляем данные на экране
                LoadUserData();
            }
            else
            {
                MessageBox.Show("Не удалось обновить данные", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при обновлении данных:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void NavigateToAdmin()
    {
        _navigationService.NavigateToAdmin();
    }

    [RelayCommand]
    private void Logout()
    {
        try
        {
            _authService.Logout();
            _navigationService.NavigateToLogin();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при выходе:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

