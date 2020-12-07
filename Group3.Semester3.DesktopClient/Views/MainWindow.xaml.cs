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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ApiService apiService;
        private Switcher switcher;

        private MainWindowModel Model = new Model.MainWindowModel();

        private BitmapImage IconFolder;
        private BitmapImage IconFolderOpen;
        private BitmapImage IconFile;
        private BitmapImage IconText;
        private BitmapImage IconMusic;
        private BitmapImage IconImage;
        private BitmapImage IconExecutable;

        Stack<FileEntity> folderStack = new Stack<FileEntity>();

        public FileEntityWrapper SelectedFile { get; set; }

        public MainWindow(ApiService apiService, Switcher switcher)
        {
            DataContext = Model.FileViewList;

            this.switcher = switcher;
            this.apiService = apiService;

            InitializeComponent();

            // ono
            IconFolder = Resources["IconFolder"] as BitmapImage;
            IconExecutable = Resources["IconExecutable"] as BitmapImage;
            IconText = Resources["IconText"] as BitmapImage;
            IconMusic = Resources["IconMusic"] as BitmapImage;
            IconFile = Resources["IconFile"] as BitmapImage;
            IconImage = Resources["IconImage"] as BitmapImage;
            IconFolderOpen = Resources["IconFolderOpen"] as BitmapImage;

            labelUserName.Content = $"{apiService.User.Name} ({apiService.User.Email})".ToUpper();

            UpdateFilePanel(new FileEntity
            {
                Name = "Test Set.xml"
            });

            UpdateFileList();
            UpdateProgress();
        }

        private void UpdateFileList(FileEntity rootFolder = null)
        {
            Model.FileViewList.Clear();

            FileEntity upFolder = null;

            var l = (rootFolder == null) ? apiService.FileList() : apiService.FileList(rootFolder.Id);

            if(l.Count > 0 && l[0].ParentId == Guid.Empty)
            {
                folderStack.Clear();
                SelectedFile = null;
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
                foreach (var x in folderStack)
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
            if (SelectedFile != null && !SelectedFile.FileEntity.IsFolder) _ = FileSaveAsAsync(SelectedFile.FileEntity);
        }

        private void MenuItemRun_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedFile != null && !SelectedFile.FileEntity.IsFolder) _ = FileDownloadAndExecAsync(SelectedFile.FileEntity);
            if (SelectedFile?.FileEntity?.IsFolder == true) UpdateFileList(SelectedFile.FileEntity);
        }

        private void MenuItemRemove_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedFile != null && !SelectedFile.FileEntity.IsFolder) apiService.DeleteFile(SelectedFile.FileEntity);
            //TODO 
            if (SelectedFile?.FileEntity?.IsFolder == true) try { apiService.DeleteFile(SelectedFile.FileEntity); } catch { }

            if (folderStack.Count > 0) UpdateFileList(folderStack.Peek());
            else UpdateFileList();
        }

        private void MenuItemNewFolder_Click(object sender, RoutedEventArgs e)
        {
            // var result = DialogHost.Show(new String("asdf"));
        }
    }
}
