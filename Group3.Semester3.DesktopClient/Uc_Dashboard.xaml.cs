using Group3.Semester3.DesktopClient.Services;
using Group3.Semester3.WebApp.Entities;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Group3.Semester3.DesktopClient
{
    /// <summary>
    /// Interaction logic for Uc_Dashboard.xaml
    /// </summary>
    public partial class Uc_Dashboard : UserControl, ISwitchable
    {

        private UserModel currentUser;
        private ApiService apiService = new ApiService();
        public Uc_Dashboard()
        {
            InitializeComponent();

            currentUser = apiService.CurrentUser();
            labelUserName.Content += currentUser.Name.ToUpper();

            List<FileEntity> files = apiService.FileList(currentUser);

            foreach (var f in files)
            {
                var item = new TreeViewItem();
                var grid = new Grid();

                ColumnDefinition colDef1 = new ColumnDefinition();
                ColumnDefinition colDef2 = new ColumnDefinition();
                colDef1.Width = GridLength.Auto;
                colDef2.Width = GridLength.Auto;

                grid.ColumnDefinitions.Add(colDef1);
                grid.ColumnDefinitions.Add(colDef2);

                grid.VerticalAlignment = VerticalAlignment.Center;
                grid.HorizontalAlignment = HorizontalAlignment.Stretch;

                TextBlock txt1 = new TextBlock();
                txt1.Text = f.Name;
                {
                    Thickness margin = txt1.Margin;
                    margin.Right = 12;
                    txt1.Margin = margin;
                }
                txt1.FontWeight = FontWeights.Bold;
                Grid.SetColumn(txt1, 0);

                // Add the second text cell to the Grid
                TextBlock txt2 = new TextBlock();
                txt2.Text = f.Id.ToString();
                txt2.Foreground = Brushes.DarkGray;
                txt2.HorizontalAlignment = HorizontalAlignment.Center;
                Grid.SetColumn(txt2, 1);

                grid.Children.Add(txt1);
                grid.Children.Add(txt2);

                treeBogoRoot.Items.Add(grid);
            }
            //< TreeViewItem Header = "Bullshit" IsExpanded = "True" />
        }

        public void UtilizeState(object state)
        {
            throw new NotImplementedException();
        }
    }
}
