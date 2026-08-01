using StatisticsAnalysisTool.Localization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StatisticsAnalysisTool.Views;

public partial class LootComparatorSaveNameWindow
{
    public LootComparatorSaveNameWindow()
    {
        InitializeComponent();

        Title = LocalizationController.Translation("SAVE_LOOT_COMPARATOR_STATE");
        TitleLabel.Content = Title;
        PromptTextBlock.Text = LocalizationController.Translation("LOOT_COMPARATOR_SAVE_NAME_PROMPT");
        SaveButton.Content = LocalizationController.Translation("SAVE");
        CancelButton.Content = LocalizationController.Translation("CANCEL");
        SaveButton.IsEnabled = false;
        Owner = Application.Current?.MainWindow;

        Loaded += (_, _) => SaveNameTextBox.Focus();
    }

    public string SaveName { get; private set; } = string.Empty;

    private void SaveNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        SaveButton.IsEnabled = !string.IsNullOrWhiteSpace(SaveNameTextBox.Text);
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var name = SaveNameTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        SaveName = name;
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void Hotbar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            DragMove();
        }
    }
}
