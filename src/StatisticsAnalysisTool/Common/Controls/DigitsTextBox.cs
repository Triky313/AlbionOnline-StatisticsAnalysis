using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace StatisticsAnalysisTool.Common.Controls;

public class DigitsTextBox : TextBox
{
    public DigitsTextBox()
    {
        PreviewTextInput += DefaultPreviewTextInput;
    }

    private static void DefaultPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !char.IsDigit(e.Text.Last()) && e.Text.Last() != '.';
    }
}