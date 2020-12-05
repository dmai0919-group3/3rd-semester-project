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
        /// 
        /// </summary>
        /// <param name="nextPage"></param>
        public void Navigate(UserControl nextPage)
        {
            Grid.Children.Clear();
            Grid.Children.Add(nextPage);
        }

        /// <summary>
        /// //TODO
        /// </summary>
        /// <param name="nextPage"></param>
        /// <param name="state"></param>
        public void Navigate(UserControl nextPage, object state)
        {
            throw new NotImplementedException();
        }
    }
}
