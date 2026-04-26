using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Nalbur.Wpf.Views;

public partial class SalesView : UserControl
{
    private static readonly Regex NumberRegex = new("^[0-9]+$");

    public SalesView()
    {
        InitializeComponent();
    }

    private void QuantityTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !NumberRegex.IsMatch(e.Text);
    }

    private void QuantityTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
    {
        if (e.DataObject.GetDataPresent(typeof(string)))
        {
            var text = e.DataObject.GetData(typeof(string)) as string;

            if (string.IsNullOrWhiteSpace(text) || !NumberRegex.IsMatch(text))
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