using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Group3.Semester3.DesktopClient.ViewHelpers
{
    public class Switcher
    {
        public INavigatable ActiveWindow;

        public void Switch(UserControl newPage)
        {
            ActiveWindow.Navigate(newPage);
        }

        public void Switch(UserControl newPage, object state)
        {
            ActiveWindow.Navigate(newPage, state);
        }
    }
}
