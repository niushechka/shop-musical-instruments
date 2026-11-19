using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace course_project_shop_musical_instruments.Views;

public partial class AdminProductsView : UserControl
{
    public AdminProductsView()
    {
        InitializeComponent();
    }

    private void PriceTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            string currentText = textBox.Text ?? string.Empty;
            int selectionStart = textBox.SelectionStart;
            int selectionLength = textBox.SelectionLength;
            string newText = currentText.Substring(0, selectionStart) + e.Text + currentText.Substring(selectionStart + selectionLength);

            // Запрещаем ввод минуса и других недопустимых символов
            // Разрешаем только цифры и одну точку для десятичных чисел
            Regex regex = new Regex(@"^[0-9]*\.?[0-9]*$");
            if (!regex.IsMatch(newText))
            {
                e.Handled = true;
                return;
            }

            // Проверяем количество точек (не более одной)
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
            // Запрещаем вставку текста, содержащего минус или другие недопустимые символы
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

    private void StockQuantityTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            string currentText = textBox.Text ?? string.Empty;
            int selectionStart = textBox.SelectionStart;
            int selectionLength = textBox.SelectionLength;
            string newText = currentText.Substring(0, selectionStart) + e.Text + currentText.Substring(selectionStart + selectionLength);

            // Запрещаем ввод минуса и других недопустимых символов
            // Разрешаем только целые положительные числа (только цифры)
            Regex regex = new Regex(@"^[0-9]*$");
            if (!regex.IsMatch(newText))
            {
                e.Handled = true;
            }
        }
    }

    private void StockQuantityTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
    {
        if (e.DataObject.GetDataPresent(typeof(string)))
        {
            string pastedText = (string)e.DataObject.GetData(typeof(string));
            // Запрещаем вставку текста, содержащего минус, точку или другие недопустимые символы
            // Разрешаем только целые положительные числа
            Regex regex = new Regex(@"^[0-9]*$");
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
