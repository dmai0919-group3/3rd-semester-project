using Group3.Semester3.DesktopClient.Services;
using Group3.Semester3.DesktopClient.ViewHelpers;
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
using System.Windows.Shapes;

namespace Group3.Semester3.DesktopClient.Views
{
    /// <summary>
    /// Interaction logic for MyFiles.xaml
    /// </summary>
    public partial class MyFiles : UserControl
    {
        private ApiService apiService;
        private Switcher switcher;

        public MyFiles(ApiService apiService, Switcher switcher)
        {
            this.switcher = switcher;
            this.apiService = apiService;
            InitializeComponent();

            labelUserName.Content += apiService.User.Name.ToUpper();

            List<FileEntity> files = apiService.FileList();

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

                /*if (f.ParentId == null)
                {
                    if (!f.IsFolder)
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
                    } */
                /*else
                {
                    TreeViewItem treeFolder = new TreeViewItem();
                    treeFolder.IsExpanded = false;
                    treeFolder.Name = f.Id.ToString();

                    StackPanel panelFolder = new StackPanel();
                    panelFolder.Orientation = Orientation.Horizontal;

                    TextBlock txtFolder = new TextBlock();
                    txtFolder.Text = f.Name;
                    txtFolder.FontSize = 14;

                    panelFolder.Children.Add(txtFolder);
                    treeFolder.Items.Add(panelFolder);
                    treeBogoRoot.Items.Add(treeFolder);
                }*/
            
            
            //< TreeViewItem Header = "Bullshit" IsExpanded = "True" />
            }
        }
        private void btnUpload_Click(object sender, RoutedEventArgs e)
        {
            switcher.Switch(new UploadFile(apiService, switcher));
        }
    }
}
