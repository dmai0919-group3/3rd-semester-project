using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace Group3.Semester3.DesktopClient
{
    public static class Switcher
    {
        public static PageSwitcher pageSwitcher;

        public static void Switch(UserControl newPage)
        {
            pageSwitcher.Navigate(newPage);
        }

        public static void Switch(UserControl newPage, object state)
        {
            pageSwitcher.Navigate(newPage, state);
        }
    }
}
