using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StatisticsAnalysisTool.Common.Controls;

public class DigitsTextBox : TextBox
{
    public DigitsTextBox()
    {
        PreviewTextInput += DefaultPreviewTextInput;
        DataObject.AddPastingHandler(this, OnPaste);
    }

    private static void DefaultPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = string.IsNullOrEmpty(e.Text) || !e.Text.All(char.IsDigit);
    }

    private static void OnPaste(object sender, DataObjectPastingEventArgs e)
    {
        if (e.DataObject.GetDataPresent(DataFormats.Text)
            && e.DataObject.GetData(DataFormats.Text) is string text
            && !string.IsNullOrEmpty(text)
            && text.All(char.IsDigit))
        {
            return;
        }

        e.CancelCommand();
    }
}
