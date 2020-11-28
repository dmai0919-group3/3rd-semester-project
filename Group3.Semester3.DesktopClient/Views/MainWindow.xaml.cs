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
using System.Windows.Shapes;

namespace Group3.Semester3.DesktopClient.Views
{
    /// <summary>
    /// Interaction logic for PageSwitcher.xaml
    /// </summary>
    public partial class MainWindow : Window, INavigatable
    {
        private UserModel currentUser;
        private ApiService apiService = new ApiService();
        
        public MainWindow()
        {
            InitializeComponent();

            currentUser = apiService.CurrentUser();

            this.labelUserName.Content = currentUser.Name;

            if (Switcher.ActiveWindow != null)
            {
                var currentWindow = (Window) Switcher.ActiveWindow;
                currentWindow.Hide();
            }

            Switcher.ActiveWindow = this;
            Switcher.Switch(new UserProfile(currentUser));
        }

        public void Navigate(UserControl nextPage)
        {
            this.Grid.Children.Clear();
            this.Grid.Children.Add(nextPage);
        }

        public void Navigate(UserControl nextPage, object state)
        {
            this.Grid.Children.Clear();
            this.Grid.Children.Add(nextPage);

            ISwitchable s = nextPage as ISwitchable;

            if (s != null)
                s.UtilizeState(state);
            else
                throw new ArgumentException("NextPage is not ISwitchable! "
                  + nextPage.Name.ToString());
        }

    }
}
