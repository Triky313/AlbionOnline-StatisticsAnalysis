using System.Threading.Tasks;
using StatisticsAnalysisTool.ViewModels;

namespace StatisticsAnalysisTool.UserControls
{
    /// <summary>
    /// Interaction logic for DonationControl.xaml
    /// </summary>
    public partial class DonationControl
    {
        public DonationControl()
        {
            InitializeComponent();
            DataContext = new DonationViewModel();
        }
    }
}
