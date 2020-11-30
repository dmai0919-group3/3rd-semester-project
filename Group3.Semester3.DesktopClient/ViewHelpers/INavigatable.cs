using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace Group3.Semester3.DesktopClient.ViewHelpers
{
    public interface INavigatable
    {
        public void Navigate(UserControl nextPage);

        public void Navigate(UserControl nextPage, object state);
    }
}
