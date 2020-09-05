using System;
using System.Windows;
using System.Windows.Controls;
using Weatherlog.ViewModels;

namespace Weatherlog.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            CreateTrayIcon();
            DataContext = new MainViewModel();
        }

        private void CreateTrayIcon()
        {
            System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon();
#if DEBUG
            ni.Icon = Properties.Resources.debugicon;
#else
            ni.Icon = Properties.Resources.icon;
#endif

            ni.Visible = true;
            ni.DoubleClick +=
                (s, e) =>
                {
                    this.Show();
                    this.WindowState = WindowState.Normal;
                };
            this.Closed +=
                (s, e) =>
                {
                    if (ni != null)
                    {
                        ni.Visible = false;
                        ni.Dispose();
                        ni = null;
                    }
                };
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == System.Windows.WindowState.Minimized)
                this.Hide();

            base.OnStateChanged(e);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            log.ScrollToEnd();
        }
    }
}