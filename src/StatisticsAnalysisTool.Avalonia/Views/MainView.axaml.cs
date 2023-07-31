using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using System;

namespace StatisticsAnalysisTool.Avalonia.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime { MainWindow: not null } desktopLifetime)
        {
            //if (e.Pointer.CapturedChangedButton != MouseButton.Left || e.ButtonState != MouseButtonState.Pressed)
            //{
            //    return;
            //}

            try
            {
                // TODO: Window Drag
                //DragMove();
            }
            catch (Exception exception)
            {
                //Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, exception);
            }
        }
    }
}
