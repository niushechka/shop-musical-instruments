using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using course_project_shop_musical_instruments.ViewModels;
using course_project_shop_musical_instruments.Views;

namespace course_project_shop_musical_instruments.Services;

public class NavigationService
{
    private Window? _currentWindow;

    public void NavigateToLogin(bool registerMode = false)
    {
        try
        {
            Debug.WriteLine($"NavigateToLogin: Starting navigation (registerMode: {registerMode})");
            
            // Проверяем, не открыто ли уже окно авторизации
            foreach (Window window in Application.Current.Windows)
            {
                if (window is LoginView existingLoginView)
                {
                    Debug.WriteLine("NavigateToLogin: LoginView already open, activating it");
                    existingLoginView.Activate();
                    existingLoginView.Focus();
                    
                    // Устанавливаем режим регистрации, если нужно
                    if (registerMode && existingLoginView.DataContext is LoginViewModel existingViewModel)
                    {
                        existingViewModel.IsLoginMode = false;
                    }
                    return;
                }
            }
            
            Debug.WriteLine("NavigateToLogin: Creating LoginView");
            var loginWindow = new LoginView();
            Debug.WriteLine("NavigateToLogin: LoginView created");
            
            // Устанавливаем режим регистрации, если нужно
            if (registerMode && loginWindow.DataContext is LoginViewModel newViewModel)
            {
                newViewModel.IsLoginMode = false;
                Debug.WriteLine("NavigateToLogin: Register mode set to true");
            }
            
            // Открываем LoginView как модальное окно, чтобы CatalogView оставался в фоне
            loginWindow.Owner = _currentWindow;
            loginWindow.ShowDialog();
            Debug.WriteLine("NavigateToLogin: LoginView shown as dialog");
            
            // После закрытия LoginView обновляем статус авторизации в CatalogView
            if (_currentWindow != null && _currentWindow.DataContext is CatalogViewModel catalogViewModel)
            {
                catalogViewModel.UpdateAuthenticationStatus();
                Debug.WriteLine("NavigateToLogin: Authentication status updated in CatalogView");
            }
        }
        catch (Exception ex)
        {
            var errorMsg = $"Ошибка при открытии окна входа:\n{ex.Message}\n\n{ex.StackTrace}";
            Debug.WriteLine($"NavigateToLogin ERROR: {errorMsg}");
            MessageBox.Show(errorMsg, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void NavigateToCatalog()
    {
        try
        {
            Debug.WriteLine("NavigateToCatalog: Starting navigation");
            
            // Закрываем все окна LoginView
            foreach (Window window in Application.Current.Windows)
            {
                if (window is LoginView)
                {
                    Debug.WriteLine("NavigateToCatalog: Found LoginView, closing it");
                    window.Closing -= OnWindowClosing;
                    window.Close();
                }
            }
            
            // Закрываем все окна ProductDetailsView
            foreach (Window window in Application.Current.Windows)
            {
                if (window is ProductDetailsView)
                {
                    Debug.WriteLine("NavigateToCatalog: Found ProductDetailsView, closing it");
                    window.Closing -= OnWindowClosing;
                    window.Close();
                }
            }
            
            // Также закрываем текущее окно, если оно не LoginView
            if (_currentWindow != null && _currentWindow.GetType().Name != "LoginView")
            {
                Debug.WriteLine($"NavigateToCatalog: Closing current window: {_currentWindow.GetType().Name}");
                _currentWindow.Closing -= OnWindowClosing;
                _currentWindow.Close();
            }
            
            Debug.WriteLine("NavigateToCatalog: Creating CatalogView");
            var catalogWindow = new CatalogView();
            Debug.WriteLine("NavigateToCatalog: CatalogView created");
            
            // Устанавливаем новое окно как текущее
            _currentWindow = catalogWindow;
            catalogWindow.Closing += OnWindowClosing;
            
            catalogWindow.Show();
            Debug.WriteLine("NavigateToCatalog: CatalogView shown");
            
            catalogWindow.Activate();
            catalogWindow.Focus();
            Debug.WriteLine("NavigateToCatalog: CatalogView activated");
            Debug.WriteLine("NavigateToCatalog: Navigation completed");
        }
        catch (Exception ex)
        {
            var errorMsg = $"Ошибка при открытии каталога:\n{ex.Message}\n\n{ex.StackTrace}";
            Debug.WriteLine($"NavigateToCatalog ERROR: {errorMsg}");
            
            MessageBox.Show(
                errorMsg,
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    public void NavigateToCart()
    {
        try
        {
            Debug.WriteLine("NavigateToCart: Creating CartView");
            var cartWindow = new CartView();
            Debug.WriteLine("NavigateToCart: CartView created");
            
            // Закрываем текущее окно, отключив обработчик Closing
            if (_currentWindow != null && _currentWindow != cartWindow)
            {
                Debug.WriteLine($"NavigateToCart: Closing current window: {_currentWindow.GetType().Name}");
                _currentWindow.Closing -= OnWindowClosing;
                _currentWindow.Close();
            }
            
            // Устанавливаем новое окно как текущее
            _currentWindow = cartWindow;
            cartWindow.Closing += OnWindowClosing;
            
            cartWindow.Show();
            cartWindow.Activate();
            cartWindow.Focus();
            Debug.WriteLine("NavigateToCart: Navigation completed");
        }
        catch (Exception ex)
        {
            var errorMsg = $"Ошибка при открытии корзины:\n{ex.Message}\n\n{ex.StackTrace}";
            Debug.WriteLine($"NavigateToCart ERROR: {errorMsg}");
            MessageBox.Show(errorMsg, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void NavigateToCheckout()
    {
        try
        {
            Debug.WriteLine("NavigateToCheckout: Creating CheckoutView");
            var checkoutWindow = new CheckoutView();
            Debug.WriteLine("NavigateToCheckout: CheckoutView created");
            
            // Закрываем текущее окно, отключив обработчик Closing
            if (_currentWindow != null && _currentWindow != checkoutWindow)
            {
                Debug.WriteLine($"NavigateToCheckout: Closing current window: {_currentWindow.GetType().Name}");
                _currentWindow.Closing -= OnWindowClosing;
                _currentWindow.Close();
            }
            
            // Устанавливаем новое окно как текущее
            _currentWindow = checkoutWindow;
            checkoutWindow.Closing += OnWindowClosing;
            
            checkoutWindow.Show();
            checkoutWindow.Activate();
            checkoutWindow.Focus();
            Debug.WriteLine("NavigateToCheckout: Navigation completed");
        }
        catch (Exception ex)
        {
            var errorMsg = $"Ошибка при открытии оформления заказа:\n{ex.Message}\n\n{ex.StackTrace}";
            Debug.WriteLine($"NavigateToCheckout ERROR: {errorMsg}");
            MessageBox.Show(errorMsg, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void NavigateToAccount()
    {
        try
        {
            Debug.WriteLine("NavigateToAccount: Creating AccountView");
            var accountWindow = new AccountView();
            Debug.WriteLine("NavigateToAccount: AccountView created");
            
            // Закрываем текущее окно, отключив обработчик Closing
            if (_currentWindow != null && _currentWindow != accountWindow)
            {
                Debug.WriteLine($"NavigateToAccount: Closing current window: {_currentWindow.GetType().Name}");
                _currentWindow.Closing -= OnWindowClosing;
                _currentWindow.Close();
            }
            
            // Устанавливаем новое окно как текущее
            _currentWindow = accountWindow;
            accountWindow.Closing += OnWindowClosing;
            
            accountWindow.Show();
            accountWindow.Activate();
            accountWindow.Focus();
            Debug.WriteLine("NavigateToAccount: Navigation completed");
        }
        catch (Exception ex)
        {
            var errorMsg = $"Ошибка при открытии профиля:\n{ex.Message}\n\n{ex.StackTrace}";
            Debug.WriteLine($"NavigateToAccount ERROR: {errorMsg}");
            MessageBox.Show(errorMsg, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void NavigateToAdmin()
    {
        try
        {
            Debug.WriteLine("NavigateToAdmin: Starting navigation");
            
            // Закрываем все окна LoginView
            foreach (Window window in Application.Current.Windows)
            {
                if (window is LoginView)
                {
                    Debug.WriteLine("NavigateToAdmin: Found LoginView, closing it");
                    window.Closing -= OnWindowClosing;
                    window.Close();
                }
            }
            
            // Также закрываем текущее окно, если оно не LoginView
            if (_currentWindow != null && _currentWindow.GetType().Name != "LoginView")
            {
                Debug.WriteLine($"NavigateToAdmin: Closing current window: {_currentWindow.GetType().Name}");
                _currentWindow.Closing -= OnWindowClosing;
                _currentWindow.Close();
            }
            
            Debug.WriteLine("NavigateToAdmin: Creating AdminMainView");
            var adminWindow = new AdminMainView();
            Debug.WriteLine("NavigateToAdmin: AdminMainView created");
            
            // Устанавливаем новое окно как текущее
            _currentWindow = adminWindow;
            adminWindow.Closing += OnWindowClosing;
            
            adminWindow.Show();
            Debug.WriteLine("NavigateToAdmin: AdminMainView shown");
            
            adminWindow.Activate();
            adminWindow.Focus();
            Debug.WriteLine("NavigateToAdmin: AdminMainView activated");
            Debug.WriteLine("NavigateToAdmin: Navigation completed");
        }
        catch (Exception ex)
        {
            var errorMsg = $"Ошибка при открытии админ-панели:\n{ex.Message}\n\n{ex.StackTrace}";
            Debug.WriteLine($"NavigateToAdmin ERROR: {errorMsg}");
            
            MessageBox.Show(
                errorMsg,
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    public void NavigateToAdminProfile()
    {
        try
        {
            Debug.WriteLine("NavigateToAdminProfile: Creating AdminProfileView");
            var adminProfileWindow = new AdminProfileView();
            Debug.WriteLine("NavigateToAdminProfile: AdminProfileView created");
            
            // Закрываем текущее окно, отключив обработчик Closing
            if (_currentWindow != null && _currentWindow != adminProfileWindow)
            {
                Debug.WriteLine($"NavigateToAdminProfile: Closing current window: {_currentWindow.GetType().Name}");
                _currentWindow.Closing -= OnWindowClosing;
                _currentWindow.Close();
            }
            
            // Устанавливаем новое окно как текущее
            _currentWindow = adminProfileWindow;
            adminProfileWindow.Closing += OnWindowClosing;
            
            adminProfileWindow.Show();
            adminProfileWindow.Activate();
            adminProfileWindow.Focus();
            Debug.WriteLine("NavigateToAdminProfile: Navigation completed");
        }
        catch (Exception ex)
        {
            var errorMsg = $"Ошибка при открытии профиля администратора:\n{ex.Message}\n\n{ex.StackTrace}";
            Debug.WriteLine($"NavigateToAdminProfile ERROR: {errorMsg}");
            MessageBox.Show(errorMsg, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void SetCurrentWindow(Window window)
    {
        _currentWindow = window;
    }
    
    private void OnWindowClosing(object? sender, CancelEventArgs e)
    {
        if (sender is Window window && Application.Current != null)
        {
            Debug.WriteLine($"OnWindowClosing: Window {window.GetType().Name} is closing");
            Application.Current.Shutdown();
        }
    }
}

