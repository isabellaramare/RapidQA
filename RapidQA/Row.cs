using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RapidQA
{
    class Row
    {
        public bool IsDown { get; set; }
        public bool IsDragging { get; set; }
        public Point StartPoint { get; set; }
        //public UIElement RealDragSource { get; set; }
        public Grid Grid { get; set; }
        public ComboBox ComboBox { get; set; }
        public CheckBox CBVisibility { get; set; }
        public CheckBox CBLock { get; set; }
        public Button Button { get; set; }


        public Row()
        {
            ComboBox = new ComboBox();
            CBVisibility = new CheckBox();
            CBLock = new CheckBox();
            Button = new Button();
        }

        public Row CreateNewRow(int layerCount)
        {
            Grid grid = new Grid();
            Label label = new Label();
            CheckBox cbVis = new CheckBox();
            CheckBox cbLock = new CheckBox();
            Button button = new Button();

            for (int i = 0; i < 6; i++)
            {
                ColumnDefinition column = new ColumnDefinition();

                switch (i)
                {
                    case 0:
                        column.Width = new GridLength(1, GridUnitType.Star);
                        grid.ColumnDefinitions.Add(column);
                        break;
                    case 1:
                        column.Width = new GridLength(2.5, GridUnitType.Star);
                        grid.ColumnDefinitions.Add(column);
                        break;
                    case 2:
                        column.Width = new GridLength(1, GridUnitType.Star);
                        grid.ColumnDefinitions.Add(column);
                        break;
                    case 3:
                        column.Width = new GridLength(3.5, GridUnitType.Star);
                        grid.ColumnDefinitions.Add(column);
                        break;
                    case 4:
                        column.Width = new GridLength(1, GridUnitType.Star);
                        grid.ColumnDefinitions.Add(column);
                        break;
                    case 5:
                        column.Width = new GridLength(1, GridUnitType.Star);
                        grid.ColumnDefinitions.Add(column);
                        break;
                }
            }

            grid.RowDefinitions.Add(new RowDefinition());
            grid.Margin = new Thickness(0, 5, 0, 5);
            grid.Background = new SolidColorBrush(Color.FromArgb(100, 221, 221, 221));

            int numberOfLayers = layerCount;
            label.Content = "Layer " + numberOfLayers;

            cbVis.IsChecked = true;

            label.SetValue(Grid.ColumnProperty, 1);
            button.SetValue(Grid.ColumnProperty, 2);

            cbVis.SetValue(Grid.ColumnProperty, 4);
            cbLock.SetValue(Grid.ColumnProperty, 5);

            label.VerticalAlignment = VerticalAlignment.Center;
            cbVis.VerticalAlignment = VerticalAlignment.Center;
            cbLock.VerticalAlignment = VerticalAlignment.Center;

            button.HorizontalAlignment = HorizontalAlignment.Stretch;
            cbVis.HorizontalAlignment = HorizontalAlignment.Center;
            cbVis.IsEnabled = false;
            cbLock.HorizontalAlignment = HorizontalAlignment.Center;
            cbLock.IsEnabled = false;

            button.Margin = new Thickness(0, 3, 0, 2);
            button.SetValue(Grid.ColumnSpanProperty, 2);
            button.Content = "Select Images...";         

            grid.Children.Add(label);
            grid.Children.Add(button);
            grid.Children.Add(cbVis);
            grid.Children.Add(cbLock);

            Row newRow = new Row();
            newRow.Grid = grid;
            newRow.CBVisibility = cbVis;
            newRow.CBLock = cbLock;
            newRow.Button = button;

            return newRow;
        }
    }
}
