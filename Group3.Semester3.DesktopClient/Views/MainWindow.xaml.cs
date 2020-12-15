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

        private string CommandCreateFolder = "CreateFolder";
        private string CommandRemoveFile = "RemoveFile";
        private string CommandRenameFile = "RenameFile";

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

            var l = (rootFolder == null) ?
                apiService.FileList(groupId: Model.SelectedGroup.Id) :
                apiService.FileList(rootFolder.Id, Model.SelectedGroup.Id);

            if (l.Count > 0 && l[0].ParentId == Guid.Empty)
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
                    FileEntity = upFolder,
                    Dummy = true
                };
                Model.FileViewList.Add(e);
            }

            {
                string name;
                if (Model.SelectedGroup.Id != Guid.Empty) name = Model.SelectedGroup.Name;
                else name = "Home";

                foreach (var x in folderStack.Reverse())
                {
                    name += $" / {x.Name}";
                }
                labelPath.Content = name;
            }

            foreach (FileEntity f in l.Where(x => x.IsFolder).OrderBy(x => x.Name))
            {
                Model.FileViewList.Add(new FileEntityWrapper
                {
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

            if (upFolder != null) upFolder.Name = "[UP]";

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

        private async void MenuItemRemove_Click(object sender, RoutedEventArgs e)
        {
            var prompt = new YesNoPrompt()
            {
                Title = $"Remove {(Model.SelectedFile.FileEntity.IsFolder ? "folder" : "file")}",
                Message = "Are you sure you want to remove the " +
                    (Model.SelectedFile.FileEntity.IsFolder ? "folder " : "file ") +
                    Model.SelectedFile.FileEntity.Name + "?",
                ButtonText = "Remove",
                ButtonCommand = CommandRemoveFile
            };

            bool success = false;
            string message = "An unexpected error has occurred";

            DialogClosingEventHandler h = (e, args) =>
            {
                if ((string)args.Parameter != CommandRemoveFile)
                {
                    success = true;
                    return;
                }

                try
                {
                    apiService.DeleteFile(Model.SelectedFile.FileEntity);
                    success = true;
                }
                catch (ApiService.ApiAuthorizationException ex)
                {
                    message = ex.Message;
                }
                catch { }
            };

            await DialogHost.Show(prompt, "RootDialog", h);

            if (!success)
            {
                _ = DialogHost.Show(new ErrorNotificationMessage() { Message = message });
            }

            if (folderStack.Count > 0) UpdateFileList(folderStack.Peek());
            else UpdateFileList();
        }

        private async void MenuItemNewFolder_Click(object sender, RoutedEventArgs e)
        {
            TextPrompt prompt = new TextPrompt()
            {
                Title = "Create new folder",
                Message = "Enter the name of the folder",
                ButtonText = "Create folder",
                ButtonCommand = CommandCreateFolder
            };

            bool success = false;
            string message = "An unexpected error has occurred";

            DialogClosingEventHandler h = (e, args) =>
            {
                if ((string)args.Parameter != CommandCreateFolder)
                {
                    success = true;
                    return;
                }

                try
                {
                    apiService.CreateFolder(
                        prompt.Text,
                        folderStack.Count > 0 ? folderStack.Peek().Id : Guid.Empty,
                        Model.SelectedGroup.Id);
                    success = true;
                }
                catch (ApiService.ApiAuthorizationException ex)
                {
                    message = ex.Message;
                }
                catch { }
            };

            await DialogHost.Show(prompt, "RootDialog", h);

            if (!success)
            {
                _ = DialogHost.Show(new ErrorNotificationMessage() { Message = message });
            }

            if (folderStack.Count > 0) UpdateFileList(folderStack.Peek());
            else UpdateFileList();


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
            foreach (var item in Model.GroupList)
            {
                item.Selected = item.Group.Id == (e.NewValue as GroupWrapper)?.Group?.Id;
            }

            if (Model.GroupList.Count(x => x.Selected) == 0) Model.GroupList[0].Selected = true;

            Model.SelectedGroup = Model.GroupList.Where(x => x.Selected).First().Group;

            folderStack.Clear();
            UpdateFileList();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            TextPrompt msg = new TextPrompt();
            msg.Message = "Hello information";

            await DialogHost.Show(msg, "RootDialog");

            await DialogHost.Show(new InfoNotificationMessage() { Message = msg.Text }, "RootDialog");
        }

        private async void MenuItemRename_Click(object sender, RoutedEventArgs e)
        {
            var prompt = new TextPrompt()
            {
                Title = $"Rename {(Model.SelectedFile.FileEntity.IsFolder ? "folder" : "file")}",
                Message = "What should the new name of the " +
                    (Model.SelectedFile.FileEntity.IsFolder ? "folder " : "file ") +
                    Model.SelectedFile.FileEntity.Name + " be?",
                ButtonText = "Rename",
                ButtonCommand = CommandRenameFile
            };

            bool success = false;
            string message = "An unexpected error has occurred";

            DialogClosingEventHandler h = (e, args) =>
            {
                if ((string)args.Parameter != CommandRenameFile)
                {
                    success = true;
                    return;
                }

                try
                {
                    apiService.RenameFile(Model.SelectedFile.FileEntity, prompt.Text);
                    success = true;
                }
                catch (ApiService.ApiAuthorizationException ex)
                {
                    message = ex.Message;
                }
                catch { }
            };

            await DialogHost.Show(prompt, "RootDialog", h);

            if (!success)
            {
                _ = DialogHost.Show(new ErrorNotificationMessage() { Message = message });
            }

            if (folderStack.Count > 0) UpdateFileList(folderStack.Peek());
            else UpdateFileList();
        }

        private void UploadPanel_Drop(object sender, DragEventArgs e)
        {
            if (!((DialogHost)GetValue(ContentProperty)).IsOpen ||
            !(((DialogHost)GetValue(ContentProperty)).DialogContent is UploadPopup)) return;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                List<FileToUpload> l = new List<FileToUpload>();

                foreach (var filename in files)
                {
                    l.Add(new FileToUpload
                    {
                        Name = System.IO.Path.GetFileName(filename),
                        Path = filename
                    });
                }

                Guid parent = Guid.Empty;
                if (folderStack.Count > 0) parent = folderStack.Peek().Id;

                apiService.UploadFiles(l, parent);
            }

            if (((DialogHost)GetValue(ContentProperty)).IsOpen &&
            ((DialogHost)GetValue(ContentProperty)).DialogContent is UploadPopup)
            {
                DialogHost.Close("RootDialog");
            }

            if (folderStack.Count > 0) UpdateFileList(folderStack.Peek());
            else UpdateFileList();
        }

        private async void window_DragOver(object sender, DragEventArgs e)
        {
            if (((DialogHost)GetValue(ContentProperty)).IsOpen) return;

            await DialogHost.Show(new UploadPopup()
            {
                Title = "Drop your files here",
                Message = "Drag and drop files here you'd like to upload!"
            },
            "RootDialog");
        }

        private void window_DragLeave(object sender, MouseEventArgs e)
        {
            if (((DialogHost)GetValue(ContentProperty)).IsOpen &&
                ((DialogHost)GetValue(ContentProperty)).DialogContent is UploadPopup )
            {
                DialogHost.Close("RootDialog");
            }
        }
    }
}
