using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using BinaryCalculator;

namespace WPF_BinaryCalculator;

public partial class MainWindow : Window
{
    char selectedOperation = ' ';
    BitCollection firstNum = null;
    BitCollection secondNum = null;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Button_ExecuteOperation(object sender, RoutedEventArgs e)
    {
        secondNum = ConvertTextToBinary(Text_Result.Text);
        if (firstNum is null || secondNum is null)
            return;

        var result = Calculator.ExecuteOperation(firstNum, secondNum, selectedOperation);
        Text_Result.Text = result is null ? "0" : result.BitsToInt().ToString();
        Text_BinaryNum.Text = result is null ? "0" : result.ToString();
        Text_BitsCount.Text = result is null ? "1 bit" : $"{result.Length.ToString()} bits";
        firstNum = null;
        secondNum = null;
    }

    private void Button_EnterDigit(object sender, RoutedEventArgs e)
    {
        var b = sender as Button;
        if (b is null || b.Content is null)
            return;

        string number = (string)b.Content;

        if (CompareTextWith("0"))
            Text_Result.Text = number;
        else
            Text_Result.Text += number;

        try
        {
            if (long.Parse(Text_Result.Text) > 2_147_483_647)
                Button_DeleteLastDigit(sender, e);
        }
        catch (Exception ex) {
            GenerateMessage(ex);
            Text_Result.Text = "0";
        }

        UpdateBinaryText(Text_Result.Text);
    }

    private void Button_SelectOperation(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        if (button is null || button.Content is null)
            return;

        string content = (string)button.Content;
        selectedOperation = content.Length > 0 ? content[0]: ' ';
        if (firstNum is not null)
            return;
        firstNum = ConvertTextToBinary(Text_Result.Text);
        Button_ClearText(sender, e);
    }

    private void Button_ClearText(object sender, RoutedEventArgs e)
    {
        Text_Result.Text = "0";
        UpdateBinaryText(Text_Result.Text);
    }

    private void Button_ChangeSign(object sender, RoutedEventArgs e)
    {
        var text = Text_Result.Text;
        if (text.Length == 0 || CompareTextWith("0"))
            return;

        if (CompareTextWith(textPosition: 0, '-'))
            Text_Result.Text = text.Remove(0, 1);
        else
            Text_Result.Text = '-' + text;

        UpdateBinaryText(Text_Result.Text);
    }

    private void Button_DeleteLastDigit(object sender, RoutedEventArgs e)
    {
        var len = Text_Result.Text.Length;
        if (len == 1 || CompareTextWith(textPosition: 0, '-'))
        {
            Text_Result.Text = "0";
            UpdateBinaryText(Text_Result.Text);
            return;
        }
            
        Text_Result.Text = Text_Result.Text.Remove(len - 1);
        UpdateBinaryText(Text_Result.Text);
    }

    void UpdateBinaryText(string text)
    {
        var binary = ConvertTextToBinary(text);
        Text_BinaryNum.Text = binary.ToString();
        Text_BitsCount.Text = $"{binary.Length.ToString()} bits";
    }

    bool CompareTextWith(string comparison)
    {
        return Text_Result.Text == comparison;
    }

    bool CompareTextWith(int textPosition, char comparison)
    {
        var len = Text_Result.Text.Length;
        if (textPosition < 0 || textPosition > len - 1)
            return false;

        return Text_Result.Text[textPosition] == comparison;
    }

    BitCollection ConvertTextToBinary(string text)
    {
        int num = 0;
        try
        {
            num = int.Parse(text);
        }
        catch(Exception e) {
            GenerateMessage(e);
            Text_BinaryNum.Text = "0";
            Text_BitsCount.Text = "1 bit";
            return null;
        }

        return Calculator.IntToBit(num);
    }

    void GenerateMessage(Exception e)
    {
        MessageBox.Show($"Exception Generated:\n{e.GetType()}:\n\t-{e.Message}");
    }
}