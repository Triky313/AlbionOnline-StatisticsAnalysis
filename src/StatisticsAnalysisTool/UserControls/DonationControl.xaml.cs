using StatisticsAnalysisTool.ViewModels;

namespace StatisticsAnalysisTool.UserControls
{
    /// <summary>
    /// Interaction logic for DonationControl.xaml
    /// </summary>
    public partial class DonationControl
    {
        private readonly DonationViewModel _donationViewModel;

        public DonationControl()
        {
            InitializeComponent();
            _donationViewModel = new DonationViewModel();
            DataContext = _donationViewModel;
        }
    }
}
