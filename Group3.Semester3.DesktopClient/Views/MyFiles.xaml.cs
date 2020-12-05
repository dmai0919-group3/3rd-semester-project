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
        
        public FileEntity CurrentFolder;

        public MyFiles(ApiService apiService, Switcher switcher)
        {
            this.switcher = switcher;
            this.apiService = apiService;
            this.CurrentFolder = null;
            
            InitializeComponent();

            ShowDirectoryFiles(Guid.Empty);
        }
        /// <summary>
        /// This method is called when the Upload button is clicked.
        /// It calls the Switcher.Switch() method with a new UploadFile class as parameter
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void btnUpload_Click(object sender, RoutedEventArgs e)
        {
            switcher.Switch(new UploadFile(apiService, switcher, GetCurrentFolderGuid()));
        }

        /// <summary>
        /// Loads the files contained in a given folder. 
        /// </summary>
        /// <param name="parentId">the Guid of the parent folder (or null if the parent is the root directory)</param>
        public void ShowDirectoryFiles(Guid parentId)
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

        /// <summary>
        /// This method is called when the Create Folder button is clicked. It creates a new CreateFolderModal and shows it.
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void CreateFolder_Click(object sender, RoutedEventArgs e)
        {
            var modal = new CreateFolderModal(apiService, this, GetCurrentFolderGuid());
            modal.Show();
        }
        
        /// <summary>
        /// This method is called when the user clicks on the name of a Folder. It loads the contents of that given folder.
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        public void Folder_OnMouseClick(object sender, MouseButtonEventArgs e)
        {
            var partial = (FileEntryPartial) sender;

            var file = partial.File;

            FolderName.Text = file.Name;

            this.CurrentFolder = file;
            
            ShowDirectoryFiles(file.Id);
        }

        /// <summary>
        /// This method is called when the user clicks the Rename option in the ContextMenu of a file.
        /// It creates a new FileRenameModal and shows it.
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        public void File_RenameAction(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem) sender;

            var contextMenu = (ContextMenu) menuItem.Parent;

            var fileEntryPartial = (FileEntryPartial) contextMenu.PlacementTarget;

            var file = fileEntryPartial.File;
            
            var modal = new FileRenameModal(apiService, file, fileEntryPartial);
            modal.Show();
        }

        /// <summary>
        /// This method is called when the user clicks the Delete option in the ContextMenu of a file.
        /// It shows a confirmation MessageBox that asks the user if they really want to delete a file.
        /// If they do, it calls the ApiService.DeleteFile() method and tries to delete the file. It also removes the item from the TreeBogoRoot file tree.
        /// If there are any exceptions, it catches it and shows the MessageBox with the message of the exception.
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
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

        /// <summary>
        /// It finds the Guid of the currently open folder
        /// </summary>
        /// <returns>The Guid of the currently open folder</returns>
        private Guid GetCurrentFolderGuid()
        {
            var id = Guid.Empty;

            if (CurrentFolder != null)
            {
                id = CurrentFolder.Id;
            }

            return id;
        }

        /// <summary>
        /// This method is called when the Back button is clicked. It goes up a folder in the tree.
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if(CurrentFolder.ParentId != null) {
            ShowDirectoryFiles(CurrentFolder.ParentId);
            }
            else { BackButton.IsEnabled = false; }
            // TODO: hide when at root folder with BackButton.IsEnabled = false;
            // TODO: works 2nd>1st folder, also 3rd>2nd, but when 3rd>2nd then 2nd>1st doesnt work, inspect and fix
        }
    }
}
