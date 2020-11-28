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

namespace Group3.Semester3.DesktopClient
{
    /// <summary>
    /// Interaction logic for PageSwitcher.xaml
    /// </summary>
    public partial class PageSwitcher : Window
    {
        public PageSwitcher()
        {
            InitializeComponent();
            Switcher.pageSwitcher = this;
            Switcher.Switch(new Uc_Login());
        }

        public void Navigate(UserControl nextPage)
        {
            this.Content = nextPage;
        }

        public void Navigate(UserControl nextPage, object state)
        {
            this.Content = nextPage;
            ISwitchable s = nextPage as ISwitchable;

            if (s != null)
                s.UtilizeState(state);
            else
                throw new ArgumentException("NextPage is not ISwitchable! "
                  + nextPage.Name.ToString());
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            Switcher.Switch(new Uc_Registration());
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            Switcher.Switch(new Uc_Login());
        }

    }
}
