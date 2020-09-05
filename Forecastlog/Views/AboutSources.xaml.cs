using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Weatherlog.Views
{
    /// <summary>
    /// Interaction logic for AboutSources.xaml
    /// </summary>
    public partial class AboutSources : UserControl
    {
        public AboutSources()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}