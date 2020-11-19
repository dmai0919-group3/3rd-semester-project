using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Group3.Semester3.DesktopClient
{
    /// <summary>
    /// Interaction logic for UploadFile.xaml
    /// </summary>
    public partial class UploadFile : Window
    {
        public UploadFile()
        {
            InitializeComponent();
        }
        private void btnOpenFiles_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            // TODO: add filter matching our functionality
            openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string filename in openFileDialog.FileNames)
                    lbFiles.Items.Add(System.IO.Path.GetFileName(filename));
            }
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            // TODO implement file ctrl logic
        }
    }
}
