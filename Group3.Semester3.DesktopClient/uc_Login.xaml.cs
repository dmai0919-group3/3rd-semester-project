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
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class Uc_Login : UserControl, ISwitchable
    {
        public Uc_Login()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            ApiService apiService = new ApiService();

            String email = emailTextbox.Text;
            String password = passwordTextbox.Password;
            LoginResultModel loginResultModel = apiService.Login(email, password);

            Switcher.Switch(new Uc_UploadFile());

        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            Switcher.Switch(new Uc_Registration());
        }

        public void UtilizeState(object state)
        {
            throw new NotImplementedException();
        }
    }
}
