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
using MaterialDesignThemes.Wpf;
using System.Threading.Tasks;
using System.Linq;

namespace Group3.Semester3.DesktopClient.Views
{
    public class MainWindowCommands
    {
        public static RoutedUICommand RefreshCommand
                            = new RoutedUICommand("Refresh",
                                                  "RefreshCommand",
                                                  typeof(MainWindowCommands));
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ApiService apiService;

        private MainWindowModel Model;

        private BitmapImage IconFolder;
        private BitmapImage IconFolderOpen;
        private BitmapImage IconFile;
        private BitmapImage IconText;
        private BitmapImage IconMusic;
        private BitmapImage IconImage;
        private BitmapImage IconExecutable;

        Stack<FileEntity> folderStack = new Stack<FileEntity>();


        public MainWindow(ApiService apiService)
        {

            this.apiService = apiService;

            InitializeComponent();

            Model = DataContext as MainWindowModel;

            IconFolder = Resources["IconFolder"] as BitmapImage;
            IconExecutable = Resources["IconExecutable"] as BitmapImage;
            IconText = Resources["IconText"] as BitmapImage;
            IconMusic = Resources["IconMusic"] as BitmapImage;
            IconFile = Resources["IconFile"] as BitmapImage;
            IconImage = Resources["IconImage"] as BitmapImage;
            IconFolderOpen = Resources["IconFolderOpen"] as BitmapImage;

            labelUserName.Content = $"{apiService.User.Name} ({apiService.User.Email})".ToUpper();

            UpdateGroupList();
            UpdateFileList();
            UpdateProgress();
        }

        private void UpdateGroupList()
        {
            var selectedGroupEntry = Model.GroupList.Where(x => x.Selected).FirstOrDefault();

            Model.GroupList.Clear();

            Model.GroupList.Add(new GroupWrapper
            {
                Group = new Group
                {
                    Id = Guid.Empty,
                    Name = "My Files"
                },
                Selected = true// selectedGroupEntry?.Group?.Id == Guid.Empty
            });

            foreach (var group in apiService.GetGroups())
            {
                Model.GroupList.Add(new GroupWrapper
                {
                    Group = group,
                    Selected = selectedGroupEntry?.Group?.Id == group.Id
                });
            }

            treeViewGroups.Items.Refresh();
        }

        private void UpdateFileList(FileEntity rootFolder = null)
        {
            Model.FileViewList.Clear();

            FileEntity upFolder = null;

            var l = (rootFolder == null) ? apiService.FileList() : apiService.FileList(rootFolder.Id);

            if(l.Count > 0 && l[0].ParentId == Guid.Empty)
            {
                folderStack.Clear();
                Model.SelectedFile = null;
                UpdateFilePanel();
            }

            if (folderStack.Count > 0 && folderStack.Peek().ParentId == rootFolder?.Id)
            {
                folderStack.Pop();
            }
            else if (rootFolder != null && rootFolder?.Id != Guid.Empty) folderStack.Push(rootFolder);

            if (rootFolder != null && rootFolder?.Id != Guid.Empty)
            {
                upFolder = folderStack.Count > 0 ? new FileEntity { Id = folderStack.Peek().ParentId, IsFolder = true } : new FileEntity { Id = Guid.Empty, IsFolder = true };
                var e = new FileEntityWrapper
                {
                    Icon = IconFolderOpen,
                    FileEntity = upFolder
                };
                Model.FileViewList.Add(e);
            }

            {
                string name = "Home";
                foreach (var x in folderStack.Reverse())
                {
                    name += $" / {x.Name}";
                }
                labelPath.Content = name;
            }

            foreach (FileEntity f in l.Where(x => x.IsFolder).OrderBy(x => x.Name))
            {
                Model.FileViewList.Add(new FileEntityWrapper { 
                    FileEntity = f,
                    Icon = IconFolder
                });
            }

            foreach (FileEntity f in l.Where(x => !x.IsFolder).OrderBy(x => x.Name))
            {
                var e = new FileEntityWrapper { FileEntity = f };

                if (f.Name.EndsWith(".mp3") ||
                    f.Name.EndsWith(".wav") ||
                    f.Name.EndsWith(".ogg") ||
                    f.Name.EndsWith(".flac")) e.Icon = IconMusic;
                else if (
                    f.Name.EndsWith(".txt") ||
                    f.Name.EndsWith(".doc") ||
                    f.Name.EndsWith(".docx") ||
                    f.Name.EndsWith(".odt")) e.Icon = IconText;
                else if (
                    f.Name.EndsWith(".exe") ||
                    f.Name.EndsWith(".cmd") ||
                    f.Name.EndsWith(".bat") ||
                    f.Name.EndsWith(".com") ||
                    f.Name.EndsWith(".vbs")) e.Icon = IconExecutable;
                else if (
                    f.Name.EndsWith(".bmp") ||
                    f.Name.EndsWith(".jpg") ||
                    f.Name.EndsWith(".jpeg") ||
                    f.Name.EndsWith(".psd") ||
                    f.Name.EndsWith(".tiff") ||
                    f.Name.EndsWith(".gif") ||
                    f.Name.EndsWith(".gif") ||
                    f.Name.EndsWith(".png")) e.Icon = IconImage;
                else
                    e.Icon = IconFile;

                Model.FileViewList.Add(e);
            }

            if(upFolder != null) upFolder.Name = "[UP]";

            listFileListView.Items.Refresh();
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
            if (!selected.FileEntity.IsFolder)
            {
                _ = FileDownloadAndExecAsync(selected.FileEntity);
            }
            else
            {
                UpdateFilePanel(selected.FileEntity);
                UpdateFileList(selected.FileEntity);
            }
        }

        private void MenuItemSave_Click(object sender, RoutedEventArgs e)
        {
            if (Model.SelectedFile?.FileEntity?.IsFolder != true) _ = FileSaveAsAsync(Model.SelectedFile.FileEntity);
        }

        private void MenuItemRun_Click(object sender, RoutedEventArgs e)
        {
            if (Model.SelectedFile?.FileEntity?.IsFolder != true) _ = FileDownloadAndExecAsync(Model.SelectedFile.FileEntity);
            if (Model.SelectedFile?.FileEntity?.IsFolder == true) UpdateFileList(Model.SelectedFile.FileEntity);
        }

        private void MenuItemRemove_Click(object sender, RoutedEventArgs e)
        {
            if (Model.SelectedFile?.FileEntity?.IsFolder != true) apiService.DeleteFile(Model.SelectedFile.FileEntity);
            //TODO 
            if (Model.SelectedFile?.FileEntity?.IsFolder == true) try { apiService.DeleteFile(Model.SelectedFile.FileEntity); } catch { }

            if (folderStack.Count > 0) UpdateFileList(folderStack.Peek());
            else UpdateFileList();
        }

        private void MenuItemNewFolder_Click(object sender, RoutedEventArgs e)
        {
            // TODO Show popup
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (folderStack.Count > 0) UpdateFileList(folderStack.Peek());
                else UpdateFileList();
            }
            catch { }
        }

        private void TreeViewGroups_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            foreach(var item in Model.GroupList)
            {
                item.Selected = item.Group.Id == (e.NewValue as GroupWrapper)?.Group?.Id;
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            NotificationMessage msg = new NotificationMessage();
            msg.Message = "Hello information";

            await DialogHost.Show(msg, "RootDialog");
        }
    }
}
