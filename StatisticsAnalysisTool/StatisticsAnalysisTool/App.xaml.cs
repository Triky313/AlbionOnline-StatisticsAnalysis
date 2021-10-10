using System.Runtime.InteropServices;
using System.Windows.Threading;

namespace StatisticsAnalysisTool
{
    /// <summary>
    ///     Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App
    {
        // Fixes a issue in the WPF clipboard handler.
        // It is necessary to handle the unhandled exception in the Application.DispatcherUnhandledException event.
        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Exception is COMException { ErrorCode: -2147221040 })
            {
                e.Handled = true;
            }
        }
    }
}