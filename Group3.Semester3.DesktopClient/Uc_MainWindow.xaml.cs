using Group3.Semester3.DesktopClient.Helpers;
using Group3.Semester3.DesktopClient.Services;
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

namespace Group3.Semester3.DesktopClient
{
    /// <summary>
    /// Interaction logic for Uc_MainWindow.xaml
    /// </summary>
    public partial class Uc_MainWindow : UserControl, ISwitchable
    {
        private UserModel currentUser;
        private ApiService apiService = new ApiService();

        public Uc_MainWindow()
        {
            InitializeComponent();
            currentUser = apiService.CurrentUser();
            userName.Content += currentUser.Name + "!";
            userEmail.Content += currentUser.Email;
            userId.Content += currentUser.Id.ToString();
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            BearerToken.Token = "";
            currentUser = null;
            Switcher.Switch(new Uc_Login());
        }

        private void btnDashboard_Click(object sender, RoutedEventArgs e)
        {
            Switcher.Switch(new Uc_Dashboard());
        }
        public void UtilizeState(object state)
        {
            throw new NotImplementedException();
        }
    }
}
