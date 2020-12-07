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
    /// Interaction logic for Register.xaml
    /// </summary>
    public partial class Registration : UserControl, ISwitchable
    {
        private ApiService apiService;
        private Switcher switcher;

        public Registration(ApiService apiService, Switcher switcher)
        {
            this.switcher = switcher;
            this.apiService = apiService;

            InitializeComponent();
        }

        /// <summary>
        /// This method is called when the Register button is clicked.
        /// It first checks if the entered password1 and password2 are the same.
        /// If they are, it calls the ApiService.Register() method and registers a new user.
        /// If there are any exceptions thrown, it catches them and shows a MessageBox with the message of the Exception catched
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            if (password1.Password != password2.Password)
            {
                MessageBox.Show("The entered passwords don't match.\nPlease try again!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
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
                catch (Exception ex) // This shouldn't be here. We shouldn't print out a generic exception message.
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                MessageBox.Show("You have successfully registered.\nPlease log in!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                //switcher.Switch(new Login(apiService, switcher));
            }

        }

        /// <summary>
        /// This method is called when the Cancel button is clicked. It calls the Switcher.Switch() method and changes the view to Login.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            //switcher.Switch(new Login(apiService, switcher));
        }

        public void UtilizeState(object state)
        {
            throw new NotImplementedException();
        }
    }
}
