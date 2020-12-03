using System;
using System.Windows;
using System.Windows.Controls;
using Group3.Semester3.DesktopClient.Services;

namespace Group3.Semester3.DesktopClient.Views.Partials
{
    public partial class CreateFolderModal : Window
    {
        private IApiService _apiService;
        private MyFiles _view;
        private Guid parentId;
        
        public CreateFolderModal(IApiService apiService, MyFiles view, Guid parentId) : base()
        {
            InitializeComponent();

            _apiService = apiService;
            _view = view;
            this.parentId = parentId;
        }

        private void CreateFolder_Click(object sender, RoutedEventArgs e)
        {
            var name = this.FolderName.Text;

            try
            {
                var file = _apiService.CreateFolder(parentId, name);
                this.Hide();
                _view.ShowDirectoryFiles(parentId);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
        }
    }
}