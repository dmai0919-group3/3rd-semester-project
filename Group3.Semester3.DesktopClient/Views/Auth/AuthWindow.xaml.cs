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
        public AuthWindow()
        {
            InitializeComponent();

            if (Switcher.ActiveWindow != null)
            {
                var currentWindow = (Window) Switcher.ActiveWindow;
                currentWindow.Hide();
            }

            Switcher.ActiveWindow = this;
            Switcher.Switch(new Login());
        }

        public void Navigate(UserControl nextPage)
        {
            this.Grid.Children.Clear();
            this.Grid.Children.Add(nextPage);
        }

        public void Navigate(UserControl nextPage, object state)
        {
            throw new NotImplementedException();
        }
    }
}
