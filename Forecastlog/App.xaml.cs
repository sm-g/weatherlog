using System.Windows;


namespace Weatherlog
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        BackgroundDownloader bd = new BackgroundDownloader();
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            bd.Run();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            bd.Stop();
        }
    }
}
