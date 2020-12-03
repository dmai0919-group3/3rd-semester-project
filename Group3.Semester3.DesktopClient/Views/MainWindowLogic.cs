using Group3.Semester3.DesktopClient.Views.ViewPanels;
using Group3.Semester3.WebApp.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Group3.Semester3.DesktopClient.Views
{
    public partial class MainWindow : Window
    {
        public class FileEntityWrapper
        {
            public FileEntity FileEntity { get; set; }
            public string Icon { get; set; }
        }

        public FileEntityWrapper SelectedFile { get; set; }


        private void UpdateFilePanel(FileEntity fileEntity = null)
        {
            panelFileDetails.Children.Clear();
            if (fileEntity == null) return;

            var detailsPanel = new FileDetailsPanel(apiService, switcher, fileEntity);

            //detailsPanel.HorizontalAlignment = HorizontalAlignment.Stretch;
            //detailsPanel.VerticalAlignment = VerticalAlignment.Stretch;

            panelFileDetails.Children.Add(detailsPanel as UIElement);
        }
    }
}

