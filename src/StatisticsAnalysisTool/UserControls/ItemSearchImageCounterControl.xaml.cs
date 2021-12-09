using System.Windows;

namespace StatisticsAnalysisTool.UserControls
{
    /// <summary>
    /// Interaction logic for ErrorBarControl.xaml
    /// </summary>
    public partial class ItemSearchImageCounterControl
    {
        public ItemSearchImageCounterControl()
        {
            InitializeComponent();
        }

        public string LocalImageCounter
        {
            get => (string)GetValue(LocalImageCounterProperty);
            set => SetValue(LocalImageCounterProperty, value);
        }

        public static readonly DependencyProperty LocalImageCounterProperty = DependencyProperty.Register("LocalImageCounter", typeof(string), typeof(ItemSearchImageCounterControl));

        public string ItemCounterString
        {
            get => (string)GetValue(ItemCounterStringProperty);
            set => SetValue(ItemCounterStringProperty, value);
        }

        public static readonly DependencyProperty ItemCounterStringProperty = DependencyProperty.Register("ItemCounterString", typeof(string), typeof(ItemSearchImageCounterControl));
    }
}
