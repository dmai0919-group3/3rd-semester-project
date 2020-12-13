using Group3.Semester3.DesktopClient.Model;
using Group3.Semester3.DesktopClient.Services;
using Group3.Semester3.DesktopClient.ViewHelpers;
using Group3.Semester3.WebApp.Models.Users;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Group3.Semester3.DesktopClient.Views.Auth
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        private ApiService apiService;
        private LoginWindowModel Model;
        public Login(ApiService apiService)
        {
            this.apiService = apiService;
            InitializeComponent();

            Model = (LoginWindowModel)DataContext;
        }

        /// <summary>
        /// This method is called when the Login button is clicked.
        /// It calls the ApiService.Authorize() method and logs the user in.
        /// When the user is logged in, it initializes a new MainWindow and Shows it.
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            textBoxEmail.Text = textBoxEmail.Text.Trim();

            Model.EmailRequiredPromptShown = textBoxEmail.Text.Length == 0;
            Model.PasswordRequiredPromptShown = passwordBoxPassword.Password.Length == 0;

            if (Model.PasswordRequiredPromptShown || Model.EmailRequiredPromptShown)
                return;

            try
            {
                apiService.Authorize(
                    textBoxEmail.Text,
                    passwordBoxPassword.Password);

                new MainWindow(apiService).Show();

                Close();
            }
            catch (ApiService.ApiAuthorizationException ex)
            {
                var msg = new ErrorNotificationMessage()
                {
                    Message = ex.Message
                };
                
                await DialogHost.Show(msg, "LoginDialog");
            }
            catch
            {
                var msg = new ErrorNotificationMessage()
                {
                    Message = "An unexpected error has occurred"
                };

                await DialogHost.Show(msg, "LoginDialog");
            }
        }

        private async void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new RegisterWindowParams() { ApiService = apiService };
            await DialogHost.Show(dialog, "LoginDialog");
        }

    }
}
