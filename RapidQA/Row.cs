using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public Grid ButtonGrid { get; set; }


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
            Grid buttonGrid = new Grid();

            for (int i = 0; i < 7; i++)
            {
                ColumnDefinition coldef1 = new ColumnDefinition();

                switch (i)
                {
                    case 0:
                        coldef1.Width = new GridLength(1, GridUnitType.Star);
                        grid.ColumnDefinitions.Add(coldef1);
                        break;
                    case 1:
                        coldef1.Width = new GridLength(2.5, GridUnitType.Star);
                        grid.ColumnDefinitions.Add(coldef1);
                        break;
                    case 2:
                        coldef1.Width = new GridLength(4, GridUnitType.Star);
                        grid.ColumnDefinitions.Add(coldef1);
                        break;
                    case 3:
                        coldef1.Width = new GridLength(1, GridUnitType.Star);
                        grid.ColumnDefinitions.Add(coldef1);
                        break;
                    case 4:
                        coldef1.Width = new GridLength(1, GridUnitType.Star);
                        grid.ColumnDefinitions.Add(coldef1);
                        break;
                    case 5:
                        coldef1.Width = new GridLength(1, GridUnitType.Star);
                        grid.ColumnDefinitions.Add(coldef1);
                        break;
                }
            }

            grid.Height = 30;
            grid.Background = new SolidColorBrush(Color.FromArgb(100, 221, 221, 221));

            label.MaxLines = 1;
            label.MinLines = 1;
            label.MaxLength = 25;
            label.Text = "Layer " + layerCount;

            cbVis.IsChecked = true;
            ColumnDefinition coldef2 = new ColumnDefinition();
            coldef2.Width = new GridLength(20, GridUnitType.Pixel);
            buttonGrid.ColumnDefinitions.Add(coldef2);

            ColumnDefinition coldef3 = new ColumnDefinition();
            coldef3.Width = new GridLength(3, GridUnitType.Star);
            buttonGrid.ColumnDefinitions.Add(coldef3);
            button.SetValue(Grid.ColumnProperty, 0);

            drag.SetValue(Grid.ColumnProperty, 0);
            label.SetValue(Grid.ColumnProperty, 1);
            buttonGrid.SetValue(Grid.ColumnProperty, 2);
            cbVis.SetValue(Grid.ColumnProperty, 3);
            cbLock.SetValue(Grid.ColumnProperty, 4);
            delete.SetValue(Grid.ColumnProperty, 5);

            label.VerticalAlignment = VerticalAlignment.Center;
            cbVis.VerticalAlignment = VerticalAlignment.Center;
            cbLock.VerticalAlignment = VerticalAlignment.Center;

            button.HorizontalAlignment = HorizontalAlignment.Left;
            cbVis.HorizontalAlignment = HorizontalAlignment.Center;
            cbLock.HorizontalAlignment = HorizontalAlignment.Center;
            cbVis.IsEnabled = false;
            cbLock.IsEnabled = false;

            label.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            label.BorderBrush = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

            button.Width = 85;
            button.Margin = new Thickness(0,4,4,4);
            button.SetValue(Grid.ColumnSpanProperty, 2);
            button.Content = "Select Images";
            button.Padding = new Thickness(4,0,4,0);
            button.BorderBrush = new SolidColorBrush(Color.FromArgb(100, 130, 130, 130));
            button.Background = new SolidColorBrush(Colors.WhiteSmoke);

            delete.Width = 14;
            delete.Height = 14;
            delete.Content = "X";                                
            delete.Padding = new Thickness(0, -2, 0, 0);

            drag.Margin = new Thickness(6,6,4,4);
            buttonGrid.Children.Add(button);

            grid.Children.Add(label);
            grid.Children.Add(buttonGrid);
            grid.Children.Add(cbVis);
            grid.Children.Add(cbLock);
            grid.Children.Add(delete);
            grid.Children.Add(drag);
                      
            Row newRow = new Row();
            newRow.Grid = grid;
            newRow.CBVisibility = cbVis;
            newRow.CBLock = cbLock;
            newRow.Button = button;
            newRow.ButtonGrid = buttonGrid;
            newRow.Delete = delete;
            newRow.Label = label;
            newRow.Drag = drag;
           
            return newRow;
        }

        public void AddRowComponents(Layer layer, ObservableCollection<Asset> assets)
        {
            ComboBox combobox = new ComboBox();
            combobox.VerticalAlignment = VerticalAlignment.Center;
            combobox.HorizontalAlignment = HorizontalAlignment.Stretch;
            combobox.SetValue(Grid.ColumnProperty, 1);
            combobox.Margin = new Thickness(0, 0, 5, 0);
            combobox.BorderBrush = new SolidColorBrush(Color.FromArgb(100, 130, 130, 130));
            combobox.Background = new SolidColorBrush(Colors.WhiteSmoke);
            combobox.ItemsSource = assets;
            //layer.Row.Grid.Children.Add(combobox);
            layer.Row.ComboBox = combobox;
            if (layer.SelectedIndex > 0) layer.Row.ComboBox.SelectedIndex = layer.SelectedIndex;
            else layer.Row.ComboBox.SelectedIndex = 0;
            layer.Row.ButtonGrid.Children.Add(combobox);

            Button btn = layer.Row.Button;
            btn.Content = "...";
            btn.Margin = new Thickness(0);
            btn.Width = 20;
            btn.Height = 22;
            btn.HorizontalAlignment = HorizontalAlignment.Left;
            //btn.HorizontalContentAlignment = HorizontalAlignment.Center;

            layer.Row.CBVisibility.RenderTransformOrigin = new Point(3.14, 0.461);
            layer.Row.CBVisibility.IsEnabled = true;
            layer.Row.CBLock.IsEnabled = true;           
        }
    }

}
