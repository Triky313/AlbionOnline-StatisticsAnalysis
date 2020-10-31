using log4net;
using System.Windows;

namespace StatisticsAnalysisTool
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(App));
        protected override void OnStartup(StartupEventArgs e)
        {
            log4net.Config.XmlConfigurator.Configure();
            //Log.Info("        =============  Started Logging  =============        ");
            base.OnStartup(e);
        }
    }
}
