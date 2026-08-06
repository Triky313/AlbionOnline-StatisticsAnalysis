using FontAwesome5;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;

namespace StatisticsAnalysisTool.UserControls;

public partial class DashboardLootControl
{
    public static readonly DependencyProperty ShowDetailsProperty = DependencyProperty.Register(
        nameof(ShowDetails),
        typeof(bool),
        typeof(DashboardLootControl),
        new PropertyMetadata(true));

    public bool ShowDetails
    {
        get => (bool) GetValue(ShowDetailsProperty);
        set => SetValue(ShowDetailsProperty, value);
    }

    public DashboardLootControl()
    {
        InitializeComponent();
    }

    private void LootStatsToggle_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        ToggleSection(
            viewModel.DashboardBindings.LootStatsVisibility,
            value => viewModel.DashboardBindings.LootStatsVisibility = value,
            value => viewModel.DashboardBindings.LootStatsToggleIcon = value);
    }

    private void LootValueDistributionToggle_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        ToggleSection(
            viewModel.DashboardBindings.LootValueDistributionVisibility,
            value => viewModel.DashboardBindings.LootValueDistributionVisibility = value,
            value => viewModel.DashboardBindings.LootValueDistributionToggleIcon = value);
    }

    private void LootTierEnchantmentToggle_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        ToggleSection(
            viewModel.DashboardBindings.LootTierEnchantmentVisibility,
            value => viewModel.DashboardBindings.LootTierEnchantmentVisibility = value,
            value => viewModel.DashboardBindings.LootTierEnchantmentToggleIcon = value);
    }

    private void TopLootAreasToggle_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        ToggleSection(
            viewModel.DashboardBindings.TopLootAreasVisibility,
            value => viewModel.DashboardBindings.TopLootAreasVisibility = value,
            value => viewModel.DashboardBindings.TopLootAreasToggleIcon = value);
    }

    private static void ToggleSection(
        Visibility currentVisibility,
        Action<Visibility> setVisibility,
        Action<EFontAwesomeIcon> setIcon)
    {
        var isVisible = currentVisibility == Visibility.Visible;
        setVisibility(isVisible ? Visibility.Collapsed : Visibility.Visible);
        setIcon(isVisible ? EFontAwesomeIcon.Solid_Plus : EFontAwesomeIcon.Solid_Minus);
    }
}
