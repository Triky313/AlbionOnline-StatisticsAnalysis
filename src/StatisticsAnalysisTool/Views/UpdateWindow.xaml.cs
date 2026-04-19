using StatisticsAnalysisTool.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace StatisticsAnalysisTool.Views
{
    public partial class UpdateWindow
    {
        private readonly Func<Task<bool>> _downloadInstallAction;

        public UpdateWindowViewModel UpdateWindowViewModel
        {
            get;
        }

        public UpdateWindow(UpdateWindowViewModel viewModel, Func<Task<bool>> downloadInstallAction)
        {
            InitializeComponent();

            UpdateWindowViewModel = viewModel;
            _downloadInstallAction = downloadInstallAction;
            DataContext = UpdateWindowViewModel;
            Owner = Application.Current?.MainWindow;

            Height = UpdateWindowViewModel.HasPatchNotes ? 620 : 360;
            MinHeight = Height;
        }

        private async void DownloadInstall_Click(object sender, RoutedEventArgs e)
        {
            if (_downloadInstallAction == null)
            {
                DialogResult = true;
                Close();
                return;
            }

            var isSuccessful = await _downloadInstallAction();
            if (isSuccessful && IsVisible)
            {
                DialogResult = true;
                Close();
            }
        }

        private void RemindLater_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
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

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                e.Handled = true;
            }
        }
    }
}
