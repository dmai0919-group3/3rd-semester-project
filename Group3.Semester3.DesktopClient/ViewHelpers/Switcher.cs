using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Group3.Semester3.DesktopClient.ViewHelpers
{
    public static class Switcher
    {
        public static INavigatable ActiveWindow;

        public static void Switch(UserControl newPage)
        {
            ActiveWindow.Navigate(newPage);
        }

        public static void Switch(UserControl newPage, object state)
        {
            ActiveWindow.Navigate(newPage, state);
        }
    }
}
