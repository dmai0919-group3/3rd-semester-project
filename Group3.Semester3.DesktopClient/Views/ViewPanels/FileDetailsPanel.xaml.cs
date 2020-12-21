using Group3.Semester3.DesktopClient.Services;
using Group3.Semester3.DesktopClient.ViewHelpers;
using Group3.Semester3.WebApp.Entities;
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

namespace Group3.Semester3.DesktopClient.Views.ViewPanels
{
    /// <summary>
    /// Interaction logic for FileDetailsPanel.xaml
    /// </summary>
    public partial class FileDetailsPanel : UserControl
    {
        private FileEntity fileEntity;
        private ApiService apiService;

        public FileDetailsPanel(ApiService apiService, FileEntity fileEntity)
        {
            this.fileEntity = fileEntity;
            this.apiService = apiService;

            InitializeComponent();

            labelFileName.Content = fileEntity.Name;
        }
    }
}
