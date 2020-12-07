using Group3.Semester3.DesktopClient.Services;
using Group3.Semester3.DesktopClient.ViewHelpers;
using Group3.Semester3.DesktopClient.Views;
using Group3.Semester3.DesktopClient.Views.Auth;
using System.Windows;

namespace Group3.Semester3.DesktopClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ApiService _apiService;
        private Switcher _switcher;

        /// <summary>
        /// This method is called when the application is started. It initializes the ApiService and Switcher objects used across the whole application.
        /// After that, it shows a new AuthWindow.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnStartup(object sender, StartupEventArgs e)
        {
            _apiService = new ApiService();
            _switcher = new Switcher();

            new Login(_apiService, _switcher).Show();
        }
    }
}
