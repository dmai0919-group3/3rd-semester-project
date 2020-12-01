using Group3.Semester3.WebApp.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Group3.Semester3.DesktopClient.Views.Partials
{
    public class FileEntryPartial : Grid
    {
        public FileEntity file { get; set; }

        public FileEntryPartial(FileEntity file) : base()
        {
            this.file = file;

            this.Init();
        }

        public void Init()
        {
            ColumnDefinition colDef1 = new ColumnDefinition();
            colDef1.Width = GridLength.Auto;

            this.ColumnDefinitions.Add(colDef1);

            this.VerticalAlignment = VerticalAlignment.Center;
            this.HorizontalAlignment = HorizontalAlignment.Stretch;

            TextBlock fileNameTextBlock = new TextBlock();
            fileNameTextBlock.Text = file.Name;
            {
                Thickness margin = fileNameTextBlock.Margin;
                margin.Right = 12;
                fileNameTextBlock.Margin = margin;
            }
            fileNameTextBlock.FontWeight = FontWeights.Bold;
            Grid.SetColumn(fileNameTextBlock, 0);

            this.Children.Add(fileNameTextBlock);
        }
    }
}
