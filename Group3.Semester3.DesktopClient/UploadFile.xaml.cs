using Group3.Semester3.DesktopClient.Model;
using Group3.Semester3.DesktopClient.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace Group3.Semester3.DesktopClient
{
    /// <summary>
    /// Interaction logic for UploadFile.xaml
    /// </summary>
    public partial class UploadFile : Window
    {
        private IApiService apiService;
        private bool submitted;

        public UploadFile()
        {
            apiService = new ApiService();
            submitted = false;
            InitializeComponent();
        }
        private void btnOpenFiles_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            // TODO: add filter matching our functionality
            openFileDialog.Filter = "All files (*.*)|*.*";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string filepath in openFileDialog.FileNames) {

                    var filename = System.IO.Path.GetFileName(filepath);
                    var file = new FileToUpload() { 
                        Name = filename , 
                        Path = filepath
                    };

                    lbFiles.Items.Add(file);
                }
            }
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Do in new thread, so app won't wait

            // Prevent multiple submits
            if (!submitted)
            {
                submitted = true;

                BackgroundWorker bw = new BackgroundWorker();

                bw.DoWork += new DoWorkEventHandler(
                delegate (object o, DoWorkEventArgs args)
                {
                    var newList = new List<FileToUpload>();

                    foreach (var item in lbFiles.Items)
                    {
                        newList.Add((FileToUpload)item);
                    }

                    if (apiService.UploadFiles(newList, "0"))
                    {
                        // On upload success
                        this.Dispatcher.Invoke(() =>
                        {
                            lbFiles.Items.Clear();
                        });
                    }
                    else
                    {
                        // Upload failure
                    }

                    this.Dispatcher.Invoke(() =>
                    {
                        submitted = false;
                    });
                });

                bw.RunWorkerAsync();
            }
        }

        private void btnUploadFile_Click(object sender, RoutedEventArgs e)
        {
            // TODO implement file ctrl logic
        }
    }
}
