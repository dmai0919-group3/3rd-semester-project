﻿using Group3.Semester3.DesktopClient.Services;
using Group3.Semester3.DesktopClient.ViewHelpers;
using Group3.Semester3.DesktopClient.Views.ViewPanels;
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
        private ApiService apiService;
        private Switcher switcher;

        public MainWindow(ApiService apiService, Switcher switcher)
        {
            this.switcher = switcher;
            this.apiService = apiService;

            InitializeComponent();

            this.labelUserName.Content = $"{apiService.User.Name} ({apiService.User.Email})".ToUpper();

            UpdateFilePanel(new WebApp.Entities.FileEntity
            {
                Name = "Test Set.xml"
            });

            //if (switcher.ActiveWindow != null)
            //{
            //    var currentWindow = (Window) switcher.ActiveWindow;
            //    currentWindow.Hide();
            //}

            //switcher.ActiveWindow = this;
            //switcher.Switch(new UserProfile(apiService, switcher));
        }

        public void Navigate(UserControl nextPage)
        {
            //this.Grid.Children.Clear();
            //this.Grid.Children.Add(nextPage);
        }

        public void Navigate(UserControl nextPage, object state)
        {
            //this.Grid.Children.Clear();
            //this.Grid.Children.Add(nextPage);

            //ISwitchable s = nextPage as ISwitchable;

            //if (s != null)
            //    s.UtilizeState(state);
            //else
            //    throw new ArgumentException("NextPage is not ISwitchable! "
            //      + nextPage.Name.ToString());
        }

        private void btnShow_Click(object sender, RoutedEventArgs e)
        {
            //switcher.Switch(new MyFiles(apiService, switcher));
        }

        private void btnUpload_Click(object sender, RoutedEventArgs e)
        {
            //switcher.Switch(new UploadFile(apiService, switcher));
        }

    }
}
