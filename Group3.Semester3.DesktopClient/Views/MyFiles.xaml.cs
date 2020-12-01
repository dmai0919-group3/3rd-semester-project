using Group3.Semester3.DesktopClient.Services;
using Group3.Semester3.DesktopClient.ViewHelpers;
using Group3.Semester3.DesktopClient.Views.Partials;
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
using Group3.Semester3.WebApp.Helpers.Exceptions;

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

            ShowDirectoryFiles(Guid.Empty);
        }
        private void btnUpload_Click(object sender, RoutedEventArgs e)
        {
            switcher.Switch(new UploadFile(apiService, switcher));
        }

        private void MenuDelete_Click(object sender, RoutedEventArgs e)
        {
            //TODO
        }
        
        private void ShowDirectoryFiles(Guid parentId)
        {
            List<FileEntity> files = apiService.FileList(parentId);

            if (parentId == Guid.Empty)
            {
                FolderName.Text = "Home";
            }
            
            TreeBogoRoot.Items.Clear();
            
            foreach (var file in files)
            {
                var item = new FileEntryPartial(file);

                if (file.IsFolder)
                {
                    item.MouseLeftButtonUp += new MouseButtonEventHandler((Folder_OnMouseClick));
                }

                TreeBogoRoot.Items.Add(item);
            }
        }
        
        public void Folder_OnMouseClick(object sender, MouseButtonEventArgs e)
        {
            var partial = (FileEntryPartial) sender;

            var file = partial.file;

            FolderName.Text = file.Name;
            
            ShowDirectoryFiles(file.Id);
        }
    }
}
