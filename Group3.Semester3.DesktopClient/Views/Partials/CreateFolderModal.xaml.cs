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
        
        /// <summary>
        /// Constructor for the CreateFolderModal class.
        /// </summary>
        /// <param name="apiService">The ApiService object used across the whole application</param>
        /// <param name="view">The currently open MyFiles object</param>
        /// <param name="parentId">The Guid of the parent folder of the new folder that we are creating</param>
        public CreateFolderModal(IApiService apiService, MyFiles view, Guid parentId) : base()
        {
            InitializeComponent();

            _apiService = apiService;
            _view = view;
            this.parentId = parentId;
        }

        /// <summary>
        /// This method is called when the Create Folder button is clicked.
        /// It calls the ApiService.CreateFolder() method and tries to create the folder.
        /// If successful, it closes this modal and refreshes parent directory in the MyFiles view to show the newly created folder.
        /// If there is an exception, it catches it and prints the exception message on the console (???)
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
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
                Console.WriteLine(exception); // ???
                throw;
            }
        }
    }
}