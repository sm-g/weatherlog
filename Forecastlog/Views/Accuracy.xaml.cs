using System.Windows.Controls;

namespace Weatherlog.Views
{
    /// <summary>
    /// Interaction logic for Accuracy.xaml
    /// </summary>
    public partial class Accuracy : UserControl
    {
        public Accuracy()
        {
            InitializeComponent();
        }

        private void accuracyPlots_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            weatherPlots.ScrollToVerticalOffset((sender as ScrollViewer).VerticalOffset);
        }

        private void weatherPlots_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            accuracyPlots.ScrollToVerticalOffset((sender as ScrollViewer).VerticalOffset);
        }
    }
}