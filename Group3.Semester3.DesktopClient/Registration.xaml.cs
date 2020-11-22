using Group3.Semester3.DesktopClient.Services;
using Group3.Semester3.WebApp.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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

namespace desktop_app
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Registration : Window
    {
        public Registration()
        {
            InitializeComponent();
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            if (password1.Password != password2.Password)
            {
                MessageBox.Show("The entered passwords don't match.\nPlease try again!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                ApiService apiService = new ApiService();

                RegisterModel registerModel = new RegisterModel();
                registerModel.Email = email.Text;
                registerModel.Password = password1.Password;
                registerModel.Name = name.Text;

                try
                {
                    UserModel userModel = apiService.Register(registerModel);
                }
                catch (Newtonsoft.Json.JsonReaderException)
                {
                    MessageBox.Show("Error communicating with the server", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                MessageBox.Show("You have successfully registered.\nPlease log in!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                Login login = new Login();
                login.Show();
                this.Close();
            }
            
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Login login = new Login();
            login.Show();
            this.Close();
        }
    }
}
