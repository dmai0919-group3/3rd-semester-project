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
                
                var contextMenu = new ContextMenu();
                
                var renameMenuItem = new MenuItem();
                renameMenuItem.Header = "Rename";
                renameMenuItem.Click += new RoutedEventHandler(File_RenameAction);
                
                var deleteMenuItem = new MenuItem();
                deleteMenuItem.Header = "Delete";
                deleteMenuItem.Click += new RoutedEventHandler(File_DeleteAction);

                contextMenu.Items.Add(renameMenuItem);
                contextMenu.Items.Add(new Separator());
                contextMenu.Items.Add(deleteMenuItem);

                item.ContextMenu = contextMenu;
                

                TreeBogoRoot.Items.Add(item);
            }
        }

        public void Folder_OnMouseClick(object sender, MouseButtonEventArgs e)
        {
            var partial = (FileEntryPartial) sender;

            var file = partial.File;

            FolderName.Text = file.Name;
            
            ShowDirectoryFiles(file.Id);
        }

        public void File_RenameAction(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem) sender;

            var contextMenu = (ContextMenu) menuItem.Parent;

            var fileEntryPartial = (FileEntryPartial) contextMenu.PlacementTarget;

            var file = fileEntryPartial.File;
            
            var modal = new FileRenameModal(apiService, file, fileEntryPartial);
            modal.Show();
        }

        public void File_DeleteAction(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem) sender;

            var contextMenu = (ContextMenu) menuItem.Parent;

            var fileEntryPartial = (FileEntryPartial) contextMenu.PlacementTarget;

            var file = fileEntryPartial.File;
            
            string messageBoxText = "Are you sure you want to delete file " + file.Name + "?";
            string caption = "Delete a file";
            MessageBoxButton button = MessageBoxButton.YesNo;
            MessageBoxImage icon = MessageBoxImage.Warning;
            
            MessageBoxResult result = MessageBox.Show(messageBoxText, caption, button, icon);
            
            switch (result)
            {
                case MessageBoxResult.Yes:
                    try
                    {
                        apiService.DeleteFile(file);
                        
                        TreeBogoRoot.Items.Remove(fileEntryPartial);
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show("Error: file could not be deleted");
                    }
                    break;
            }
        }
    }
}
