using Group3.Semester3.DesktopClient.Services;
using Group3.Semester3.DesktopClient.ViewHelpers;
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
using System.Windows.Shapes;

namespace Group3.Semester3.DesktopClient.Views.Auth
{
    /// <summary>
    /// Interaction logic for AuthWindow.xaml
    /// </summary>
    public partial class AuthWindow : Window, INavigatable
    {
        private ApiService apiService;
        private Switcher switcher;
        /// <summary>
        /// Constructor for the AuthWindow class
        /// </summary>
        /// <param name="apiService">The ApiService object used by the whole application</param>
        /// <param name="switcher">The Switcher object used by the whole application</param>
        public AuthWindow(ApiService apiService, Switcher switcher)
        {
            this.switcher = switcher;
            this.apiService = apiService;

            InitializeComponent();

            if (switcher.ActiveWindow != null)
            {
                var currentWindow = (Window)switcher.ActiveWindow;
                currentWindow.Hide();
            }

            switcher.ActiveWindow = this;
            switcher.Switch(new Login(apiService, switcher));
        }

        /// <summary>
        /// Logic for changing between the Login and Registration views
        /// </summary>
        /// <param name="nextPage">The new View we want to change to</param>
        public void Navigate(UserControl nextPage)
        {
            Grid.Children.Clear();
            Grid.Children.Add(nextPage);
        }

        /// <summary>
        /// Not Implemented
        /// //TODO
        /// </summary>
        /// <param name="nextPage"></param>
        /// <param name="state"></param>
        /// <exception cref="NotImplementedException">This method is not implemented</exception>
        public void Navigate(UserControl nextPage, object state)
        {
            throw new NotImplementedException();
        }
    }
}
