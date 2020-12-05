using System;
using System.Windows;
using Group3.Semester3.DesktopClient.Services;
using Group3.Semester3.WebApp.Entities;

namespace Group3.Semester3.DesktopClient.Views.Partials
{
    public partial class FileRenameModal : Window
    {
        private FileEntity file;
        private ApiService _apiService;
        private FileEntryPartial _entryPartial;

        public FileRenameModal(ApiService service, FileEntity fileEntity, FileEntryPartial entryPartial) : base()
        {
            InitializeComponent();
            
            _apiService = service;
            _entryPartial = entryPartial;
            file = fileEntity;

            FileName.Text = file.Name;
        }

        private void RenameButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var name = FileName.Text;

                file = _apiService.RenameFile(file, name);
                
                _entryPartial.File = file;
                _entryPartial.FileNameTextBlock.Text = file.Name;
                this.Hide();
            }
            catch (Exception exception)
            {
                MessageBox.Show("Error: failed to rename a file");
            }
        }
    }
}