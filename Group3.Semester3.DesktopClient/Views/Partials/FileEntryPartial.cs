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
        public FileEntity File { get; set; }
        public TextBlock FileNameTextBlock { get; set; }

        public FileEntryPartial(FileEntity file) : base()
        {
            this.File = file;

            this.Init();
        }

        public void Init()
        {
            ColumnDefinition colDef1 = new ColumnDefinition();
            colDef1.Width = GridLength.Auto;

            this.ColumnDefinitions.Add(colDef1);

            this.VerticalAlignment = VerticalAlignment.Center;
            this.HorizontalAlignment = HorizontalAlignment.Stretch;

            FileNameTextBlock = new TextBlock();
            FileNameTextBlock.Text = File.Name;
            {
                Thickness margin = FileNameTextBlock.Margin;
                margin.Right = 12;
                FileNameTextBlock.Margin = margin;
            }
            FileNameTextBlock.FontWeight = FontWeights.Bold;
            Grid.SetColumn(FileNameTextBlock, 0);

            this.Children.Add(FileNameTextBlock);
        }
    }
}
