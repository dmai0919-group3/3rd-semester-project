using Group3.Semester3.WebApp.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Group3.Semester3.WebApp.Helpers.Exceptions;
using System.Windows.Interop;

namespace Group3.Semester3.DesktopClient.Views.Partials
{
    public class FileEntryPartial : Grid
    {
        public FileEntity File { get; set; }
        public TextBlock FileNameTextBlock { get; set; }

        public FileEntryPartial(FileEntity file) : base()
        {
            this.File = file;

            this.Init();
        }

        public void Init()
        {
            ColumnDefinition colDef0 = new ColumnDefinition();
            ColumnDefinition colDef1 = new ColumnDefinition();
            
            colDef0.Width = new GridLength(25);
            colDef1.Width = GridLength.Auto;

            this.ColumnDefinitions.Add(colDef0);
            this.ColumnDefinitions.Add(colDef1);

            this.VerticalAlignment = VerticalAlignment.Center;
            this.HorizontalAlignment = HorizontalAlignment.Stretch;

            var path = "pack://application:,,,/resources/icons/file-icon.png";
            
            if (File.IsFolder)
            {
                path = "pack://application:,,,/resources/icons/folder-icon.png";
            }
            
            BitmapImage bitmap = new BitmapImage();  
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(path);
            bitmap.EndInit();
            
            Image dynamicImage = new Image();
            dynamicImage.Source = bitmap;
            Grid.SetColumn(dynamicImage, 0);


            // TODO: Images are not working (fix please)

            FileNameTextBlock = new TextBlock();
            FileNameTextBlock.Text = File.Name;
            {
                Thickness margin = FileNameTextBlock.Margin;
                margin.Right = 12;
                FileNameTextBlock.Margin = margin;
            }
            FileNameTextBlock.FontWeight = FontWeights.Bold;
            Grid.SetColumn(FileNameTextBlock, 1);

            this.Children.Add(FileNameTextBlock);
        }
    }
}
