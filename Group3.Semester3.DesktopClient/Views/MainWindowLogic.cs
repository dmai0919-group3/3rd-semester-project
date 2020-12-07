using Group3.Semester3.DesktopClient.Views.ViewPanels;
using Group3.Semester3.WebApp.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Group3.Semester3.DesktopClient.Views
{
    public partial class MainWindow : Window
    {
        private void UpdateFilePanel(FileEntity fileEntity = null)
        {
            panelFileDetails.Children.Clear();
            if (fileEntity == null) return;

            var detailsPanel = new FileDetailsPanel(apiService, switcher, fileEntity);

            detailsPanel.HorizontalAlignment = HorizontalAlignment.Stretch;
            detailsPanel.VerticalAlignment = VerticalAlignment.Stretch;

            panelFileDetails.Children.Add(detailsPanel);
        }

        private async Task FileDownloadAndExecAsync(FileEntity file)
        {
            string fileName = System.IO.Path.GetTempPath() + file.AzureName + '-' + file.Name;

            using WebClient client = new WebClient();

            var task = new Model.MainWindowModel.DownloadTask
            {
                Abort = () => client?.CancelAsync(),
                Message = file.Name
            };
            Model.Tasks.Add(task);

            client.DownloadProgressChanged += (o, e) =>
            {
                task.ProgressPercentage = e.ProgressPercentage;
                task.Size = e.TotalBytesToReceive;
                UpdateProgress();
            };

            await client.DownloadFileTaskAsync(new Uri(apiService.GetDownloadLink(file.Id).downloadLink), fileName);
            Model.Tasks.Remove(task);
            UpdateProgress();

            ProcessStartInfo psi = new ProcessStartInfo(fileName);
            psi.UseShellExecute = true;
            Process.Start(psi);
        }


        private async Task FileSaveAsAsync(FileEntity file)
        {
            Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.Title = "Save file as...";
            dialog.FileName = file.Name;
            if(dialog.ShowDialog() == true)
            {
                using WebClient client = new WebClient();

                var task = new Model.MainWindowModel.DownloadTask
                {
                    Abort = () => client?.CancelAsync() ,
                    Message = file.Name
                };
                Model.Tasks.Add(task);

                client.DownloadProgressChanged += (o, e) =>
                {
                    task.ProgressPercentage = e.ProgressPercentage;
                    task.Size = e.TotalBytesToReceive;
                    UpdateProgress();
                };

                await client.DownloadFileTaskAsync(new Uri(apiService.GetDownloadLink(file.Id).downloadLink), dialog.FileName);
                Model.Tasks.Remove(task);
                UpdateProgress();
            }
        }

        private void UpdateProgress()
        {
            panelProgress.Visibility = (Model.Tasks.Count > 0) ? Visibility.Visible : Visibility.Hidden;

            double totalSize = 0, completeSize = 0;

            foreach(var task in Model.Tasks)
            {
                totalSize += task.Size;
                completeSize += task.Size * task.ProgressPercentage;   
            }

            if (totalSize <= 0) return;

            progressTaskProgress.Value = completeSize / totalSize;
            labelProgress.Content = $"Downloading {Model.Tasks.Count} files... (~{Math.Round(totalSize/ 1000000.0, 2)} MB)";

        }
    }
}

