using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace RapidQA
{

    public class Row
    {        
        public bool IsDown { get; set; }
        public bool IsDragging { get; set; }
        public Point StartPoint { get; set; }
        //public UIElement RealDragSource { get; set; }
        public Grid Grid { get; set; }
        public TextBox Label { get; set; }
        public ComboBox ComboBox { get; set; }
        public CheckBox CBVisibility { get; set; }
        public CheckBox CBLock { get; set; }
        public Button Button { get; set; }
        public Button Delete { get; set; }
        public Border Drag { get; set; }

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
            TextBox label = new TextBox();
            CheckBox cbVis = new CheckBox();
            CheckBox cbLock = new CheckBox();
            Button button = new Button();
            Button delete = new Button();
            Border drag = new Border();

            GridSplitter splitter = new GridSplitter();

            for (int i = 0; i < 7; i++)
            {
                ColumnDefinition coldef = new ColumnDefinition();

                switch (i)
                {
                    case 0:
                        coldef.Width = new GridLength(1, GridUnitType.Star);
                        grid.ColumnDefinitions.Add(coldef);
                        break;
                    case 1:
                        coldef.Width = new GridLength(2.5, GridUnitType.Star);
                        grid.ColumnDefinitions.Add(coldef);
                        break;

                    case 2:
                        coldef.Width = new GridLength(5, GridUnitType.Pixel); // Possibly some kind of problem with the drag/hover function
                        grid.ColumnDefinitions.Add(coldef);
                        break;

                    case 3:
                        coldef.Width = new GridLength(1, GridUnitType.Star);
                        grid.ColumnDefinitions.Add(coldef);
                        break;
                    case 4:
                        coldef.Width = new GridLength(3.5, GridUnitType.Star);
                        grid.ColumnDefinitions.Add(coldef);
                        break;
                    case 5:
                        coldef.Width = new GridLength(1, GridUnitType.Star);
                        grid.ColumnDefinitions.Add(coldef);
                        break;
                    case 6:
                        coldef.Width = new GridLength(1, GridUnitType.Star);
                        grid.ColumnDefinitions.Add(coldef);
                        break;
                    case 7:
                        coldef.Width = new GridLength(1, GridUnitType.Star);
                        grid.ColumnDefinitions.Add(coldef);
                        break;
                }
            }

            //grid.RowDefinitions.Add(new RowDefinition());
            grid.Margin = new Thickness(0, 5, 0, 5);
            grid.Background = new SolidColorBrush(Color.FromArgb(100, 221, 221, 221));

            label.MaxLines = 1;
            label.MinLines = 1;
            label.MaxLength = 25;
            label.Text = "Layer " + layerCount;

            cbVis.IsChecked = true;
            splitter.HorizontalAlignment = HorizontalAlignment.Stretch;
            splitter.VerticalAlignment = VerticalAlignment.Stretch;

            drag.SetValue(Grid.ColumnProperty, 0);
            label.SetValue(Grid.ColumnProperty, 1);
            splitter.SetValue(Grid.ColumnProperty, 2);
            button.SetValue(Grid.ColumnProperty, 3);
            cbVis.SetValue(Grid.ColumnProperty, 5);
            cbLock.SetValue(Grid.ColumnProperty, 6);
            delete.SetValue(Grid.ColumnProperty, 7);

            label.VerticalAlignment = VerticalAlignment.Center;
            cbVis.VerticalAlignment = VerticalAlignment.Center;
            cbLock.VerticalAlignment = VerticalAlignment.Center;

            button.HorizontalAlignment = HorizontalAlignment.Stretch;
            cbVis.HorizontalAlignment = HorizontalAlignment.Center;
            cbLock.HorizontalAlignment = HorizontalAlignment.Center;
            cbVis.IsEnabled = false;
            cbLock.IsEnabled = false;

            label.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            label.BorderBrush = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

            button.Margin = new Thickness(0, 2, 0, 2);
            button.SetValue(Grid.ColumnSpanProperty, 2);
            button.Content = "Select Images...";
                         
            delete.Width = 14;
            delete.Height = 14;
            delete.Content = "X";                                
            delete.Padding = new Thickness(0, -2, 0, 0);

            //drag.Width = 14;
            //drag.Height = 14;
            drag.Margin = new Thickness(6,6,4,4);

            grid.Children.Add(label);
            grid.Children.Add(button);
            grid.Children.Add(cbVis);
            grid.Children.Add(cbLock);
            grid.Children.Add(delete);
            grid.Children.Add(drag);
                      
            Row newRow = new Row();
            newRow.Grid = grid;
            newRow.CBVisibility = cbVis;
            newRow.CBLock = cbLock;
            newRow.Button = button;
            newRow.Delete = delete;
            newRow.Label = label;
            newRow.Drag = drag;
           
            return newRow;
        }

        public void AddRowComponents(Layer layer, List<Asset> assets)
        {
            // Add combobox to Row
            ComboBox combobox = new ComboBox();
            combobox.SetValue(Grid.ColumnProperty, 3);
            combobox.VerticalAlignment = VerticalAlignment.Center;
            combobox.HorizontalAlignment = HorizontalAlignment.Left;
            combobox.Margin = new Thickness(0, 0, 0, 0);
            combobox.ItemsSource = assets;
            layer.Row.Grid.Children.Add(combobox);
            layer.Row.ComboBox = combobox;
            if (layer.SelectedIndex > 0) layer.Row.ComboBox.SelectedIndex = layer.SelectedIndex;
            else layer.Row.ComboBox.SelectedIndex = 0;

            // Adjust button and Checkboxes
            Button btn = layer.Row.Button;
            btn.Content = "...";
            btn.Width = 20;
            btn.Height = 20;
            btn.HorizontalAlignment = HorizontalAlignment.Left;
            btn.HorizontalContentAlignment = HorizontalAlignment.Center;

            layer.Row.CBVisibility.RenderTransformOrigin = new Point(3.14, 0.461);
            layer.Row.CBVisibility.IsEnabled = true;
            layer.Row.CBLock.IsEnabled = true;           
        }
    }

}
