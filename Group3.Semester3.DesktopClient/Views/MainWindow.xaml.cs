using Group3.Semester3.DesktopClient.Services;
using Group3.Semester3.DesktopClient.ViewHelpers;
using Group3.Semester3.DesktopClient.Views.ViewPanels;
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
using Group3.Semester3.DesktopClient.Model;
using static Group3.Semester3.DesktopClient.Model.MainWindowModel;

namespace Group3.Semester3.DesktopClient.Views
{
    /// <summary>
    /// Interaction logic for PageSwitcher.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ApiService apiService;
        private Switcher switcher;

        private MainWindowModel Model = new Model.MainWindowModel();

        public FileEntityWrapper SelectedFile { get; set; }

        public MainWindow(ApiService apiService, Switcher switcher)
        {
            DataContext = Model.FileViewList;

            this.switcher = switcher;
            this.apiService = apiService;

            InitializeComponent();

            labelUserName.Content = $"{apiService.User.Name} ({apiService.User.Email})".ToUpper();

            foreach (FileEntity f in apiService.FileList())
            {
                Model.FileViewList.Add(new FileEntityWrapper { FileEntity = f });
            }

            UpdateFilePanel(new FileEntity
            {
                Name = "Test Set.xml"
            });

            UpdateProgress();
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (FileEntityWrapper item in e.AddedItems)
            {
                UpdateFilePanel(item.FileEntity);
                item.Selected = true;
            }

            foreach (FileEntityWrapper item in e.RemovedItems)
            {
                item.Selected = false;
            }
        }

        private void HandleDoubleClick(object sender, MouseButtonEventArgs e)
        {
            FileEntityWrapper selected = ((ListViewItem)sender).Content as FileEntityWrapper;
            if (!selected.FileEntity.IsFolder) FileDownloadAndExecAsync(selected.FileEntity);
        }

        private void MenuItemSave_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedFile != null && !SelectedFile.FileEntity.IsFolder) FileSaveAsAsync(SelectedFile.FileEntity);
        }

        private void MenuItemRun_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedFile != null && !SelectedFile.FileEntity.IsFolder) FileDownloadAndExecAsync(SelectedFile.FileEntity);
        }
    }
}
