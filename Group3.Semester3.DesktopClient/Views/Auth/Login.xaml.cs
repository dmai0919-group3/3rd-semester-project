using Group3.Semester3.DesktopClient.Services;
using Group3.Semester3.DesktopClient.ViewHelpers;
using Group3.Semester3.WebApp.Models.Users;
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
    public partial class Login : UserControl, ISwitchable
    {
        private ApiService apiService;
        private Switcher switcher;
        public Login(ApiService apiService, Switcher switcher)
        {
            this.switcher = switcher;
            this.apiService = apiService;
            InitializeComponent();
        }

        /// <summary>
        /// This method is called when the Login button is clicked.
        /// It calls the ApiService.Authorize() method and logs the user in.
        /// When the user is logged in, it initializes a new MainWindow and Shows it.
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string email = emailTextbox.Text;
            string password = passwordTextbox.Password;
            apiService.Authorize(email, password);
            
            new MainWindow(apiService, switcher).Show();
        }

        /// <summary>
        /// This method is called when the Register button is clicked.
        /// It calls the Switcher.Switch() and changes the current view to the Registration.
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            switcher.Switch(new Registration(apiService, switcher));
        }

        public void UtilizeState(object state)
        {
            throw new NotImplementedException();
        }
    }
}
