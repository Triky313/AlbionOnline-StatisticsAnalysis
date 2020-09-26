using System.Diagnostics;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Common
{
    public class Alert
    {
        public Alert(string uniqueName)
        {
            UniqueName = uniqueName;
        }

        public string UniqueName { get; set; }

        private bool _isEventActive;

        public void StartEvent()
        {
            if (_isEventActive)
            {
                return;
            }

            AlertEvent(UniqueName);
        }

        public void StopEvent()
        {
            _isEventActive = false;
        }

        private async void AlertEvent(string uniqueName)
        {
            while (_isEventActive)
            {
                Debug.Print($"{uniqueName} check...");
                await Task.Delay(5000);
            }
        }
    }
}