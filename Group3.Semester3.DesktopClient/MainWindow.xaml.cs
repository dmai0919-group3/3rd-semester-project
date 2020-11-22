using desktop_app;
using Group3.Semester3.DesktopClient.Helpers;
using Group3.Semester3.DesktopClient.Services;
using Group3.Semester3.WebApp.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private UserModel currentUser;
        private ApiService apiService = new ApiService();

        public MainWindow()
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
            Login login = new Login();
            login.Show();
            this.Close();
        }
    }
}
