using Group3.Semester3.DesktopClient.Model;
using Group3.Semester3.DesktopClient.Services;
using Group3.Semester3.DesktopClient.ViewHelpers;
using Group3.Semester3.WebApp.Models.Users;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
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
    public partial class Registration : UserControl
    {
        public static readonly DependencyProperty ParamsProperty = DependencyProperty.Register("Params", typeof(RegisterWindowParams), typeof(Registration), new FrameworkPropertyMetadata(new RegisterWindowParams()));

        public RegisterWindowParams Params
        {
            get { return GetValue(ParamsProperty) as RegisterWindowParams; }
            set { SetValue(ParamsProperty, value); }
        }

        private ApiService apiService {
            get => (GetValue(ParamsProperty) as RegisterWindowParams).ApiService;
        }

        public Registration()
        {
            InitializeComponent();
        }

        private async void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            RegexOptions options = RegexOptions.None;
            Regex regex = new Regex("[ ]{2,}", options);

            textBoxName.Text = regex.Replace(textBoxName.Text.Trim(), " ");
            textBoxEmail.Text = textBoxEmail.Text.Replace(" ", "");


            EmailRequiredPrompt.Visibility = textBoxEmail.Text.Length == 0 ? Visibility.Visible : Visibility.Hidden;
            NameRequiredPrompt.Visibility = textBoxName.Text.Length == 0 ? Visibility.Visible : Visibility.Hidden;
            PasswordRequiredPrompt.Visibility = passwordBoxPassword.Password.Length == 0 ? Visibility.Visible : Visibility.Hidden;
            PasswordRepeatRequiredPrompt.Visibility = passwordBoxRepeatPassword.Password.Length == 0 ? Visibility.Visible : Visibility.Hidden;

            if (textBoxEmail.Text.Length == 0 ||
                textBoxName.Text.Length == 0 ||
                passwordBoxPassword.Password.Length == 0 ||
                passwordBoxRepeatPassword.Password.Length == 0)
                return;

            if (passwordBoxPassword.Password != passwordBoxRepeatPassword.Password)
            {
                var msg = new ErrorNotificationMessage()
                {
                    Message = "The entered passwords don't match.\nPlease try again!"
                };

                await DialogHost.Show(msg, "RegisterDialog");
            }
            else
            {
                RegisterModel registerModel = new RegisterModel();
                registerModel.Email = textBoxEmail.Text;
                registerModel.Password = passwordBoxPassword.Password;
                registerModel.Name = textBoxName.Text;

                try
                {
                    UserModel userModel = apiService.Register(registerModel);

                    var msg = new InfoNotificationMessage()
                    {
                        Message = "You have successfully registered.\nPlease log in!",
                        Title = "Success"
                    };

                    await DialogHost.Show(msg, "RegisterDialog");
                    DialogHost.Close("LoginDialog");

                }
                catch (ApiService.ApiAuthorizationException ex)
                {
                    var msg = new ErrorNotificationMessage()
                    {
                        Message = ex.Message
                    };

                    await DialogHost.Show(msg, "RegisterDialog");
                }
                catch
                {
                    var msg = new ErrorNotificationMessage()
                    {
                        Message = "An unexpected error has occurred"
                    };

                    await DialogHost.Show(msg, "RegisterDialog");
                }
            }

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.Close("LoginDialog");
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            textBoxEmail.Clear();
            textBoxName.Clear();
            passwordBoxPassword.Clear();
            passwordBoxRepeatPassword.Clear();

            EmailRequiredPrompt.Visibility = Visibility.Hidden;
            NameRequiredPrompt.Visibility = Visibility.Hidden;
            PasswordRequiredPrompt.Visibility = Visibility.Hidden;
            PasswordRepeatRequiredPrompt.Visibility = Visibility.Hidden;
        }
    }
}
