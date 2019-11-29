using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Image = System.Windows.Controls.Image;
using Point = System.Windows.Point;
using Path = System.IO.Path;
using Color = System.Windows.Media.Color;

namespace RapidQA
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
       
        MainViewModel mvm = new MainViewModel();
        List<Layer> layers = new List<Layer>();
        Row row = new Row();
        string fileDirectory;

        private UIElement _dummyDragSource = new UIElement();
        private Layer movingRow = new Layer();
        private bool isOpaque = false;

        // HIGH PRIORITY
        // use Alpha channel to select/move layers https://stackoverflow.com/questions/2250965/wpf-cursor-on-a-partially-transparent-image
        // save an image (whith all visible layers)
        // size of displayed images
        // Zoom from ImUI

        // EXTRAS
        // ability to move a layer with arrows
        // accesskeys (press 1 for activating layer 1, ctrl + S to save image, ctrl + Z to revert image movement)
        // rename layers
        // custom made button design (eye for visibility, padlock for locking)

        // Making changes, blablabla        


        public MainWindow()
        {
            InitializeComponent();
            DataContext = mvm;
            //mvm.LoadFiles(folderPath);  
            BtnAddLayer_Click(null, null);

            BtnAddLayer.Click += BtnAddLayer_Click;
            BtnWhite.Click += BtnWhite_Click;
            BtnGray.Click += BtnGray_Click;
            BtnBlack.Click += BtnBlack_Click;

            //sp.MouseMove += Sp_MouseMove;
            sp.MouseLeftButtonDown += Sp_MouseLeftButtonDown;
            sp.MouseLeftButtonUp += Sp_MouseLeftButtonUp;
            sp.DragEnter += Sp_DragEnter;
            sp.Drop += Sp_Drop;

            LoadImages();
        }


        private void LoadImages()
        {
            foreach (Layer layer in layers)
            {
                Grid grid = layer.Row.Grid;
                ComboBox cb = layer.Row.ComboBox;
                CheckBox vis = layer.Row.CBVisibility;
                CheckBox lo = layer.Row.CBLock;

                //grid.MouseEnter += delegate (object sender, MouseEventArgs e) { Row_MouseEnter(sender, e, layer); };
                //grid.MouseLeave += delegate (object sender, MouseEventArgs e) { Row_MouseLeave(sender, e, layer); };
                //grid.MouseDown += delegate (object sender, MouseButtonEventArgs e) { Row_MouseDown(sender, e, layer); };

                //grid.MouseEnter += delegate (object sender, MouseEventArgs e) { RowMoveArea_MouseEnter(sender, e, grid); };
                //grid.MouseLeave += RowMoveArea_MouseLeave;

                cb.SelectionChanged += delegate (object sender, SelectionChangedEventArgs e) { Cbx_SelectionChanged(sender, e, layer); };
                lo.Click += delegate (object sender, RoutedEventArgs e) { Ckb_Lock_Click(sender, e, layer); };
                vis.Click += delegate (object sender, RoutedEventArgs e) { Ckb_Visibility_Click(sender, e, layer); };
            }
        }

        private void RowMoveArea_MouseLeave(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Arrow;
        }


        // Only works when the cursor comes from outside the row (thats the only time the event is called)
        private void RowMoveArea_MouseEnter(object sender, MouseEventArgs e, Grid grid)
        {
            double x = e.GetPosition(grid).X;
            double start = 0.0;
            int column = 0;
            foreach (ColumnDefinition cd in grid.ColumnDefinitions)
            {
                start += cd.ActualWidth;
                if (x < start)
                {
                    break;
                }
                column++;
            }
            if (column == 0) Cursor = Cursors.Hand;
        }

        private void Btn_SelectImages_Click(object sender, RoutedEventArgs e, Layer layer)
        {
            // Get the image files from the user.
            var openDlg = new Microsoft.Win32.OpenFileDialog();

            openDlg.Filter = "Image files(*.PNG;*.JPG;*.JPEG;*.TIFF)|*.PNG;*.JPG;*.JPEG;*.TIFF|All files (*.*)|*.*";
            if (fileDirectory == null) openDlg.InitialDirectory = "c:\\";
            if (fileDirectory != null) openDlg.InitialDirectory = fileDirectory;
            openDlg.Multiselect = true;

            bool? result = openDlg.ShowDialog(this);

            // Return if canceled.
            if (!(bool)result)
            {
                return;
            }

            string[] selectedFiles = openDlg.FileNames;

            List<Asset> assets = new List<Asset>();
            foreach (string filepath in selectedFiles)
            {
                assets.Add(new Asset(filepath));
            }

            // Saves the directory that was used last
            fileDirectory = new FileInfo(assets[0].Filepath).Directory.FullName;

            if (layer.Image != null)
            {
                sp.Children.Remove(layer.Row.ComboBox); // Den försvinner inte....
                GridImages.Children.Remove(layer.Image);
            }

            // Add combobox to Row
            ComboBox combobox = new ComboBox();
            combobox.SetValue(Grid.ColumnProperty, 3);
            combobox.VerticalAlignment = VerticalAlignment.Center;
            combobox.HorizontalAlignment = HorizontalAlignment.Left;
            combobox.Margin = new Thickness(5, 0, 0, 0);
            layer.Row.Grid.Children.Add(combobox);
            layer.Row.ComboBox = combobox;
            combobox.ItemsSource = assets;
            layer.Row.ComboBox.SelectedIndex = 0;

            // Adjust browse button
            Button btn = layer.Row.Button;
            btn.Content = "...";
            btn.Width = 22;
            btn.Height = 22;
            btn.HorizontalAlignment = HorizontalAlignment.Left;
            btn.HorizontalContentAlignment = HorizontalAlignment.Center;

            layer.Row.CBVisibility.RenderTransformOrigin = new Point(3.14, 0.461);
            layer.Row.CBVisibility.IsEnabled = true;
            layer.Row.CBLock.IsEnabled = true;

            Image image = CreateNewImage(layer);
            layer.Image = image;

            LoadImages();
        }

        private void BtnAddLayer_Click(object sender, RoutedEventArgs e)
        {
            Layer newLayer = new Layer();
            Row newRow = row.CreateNewRow(layers.Count);
            sp.Children.Add(newRow.Grid);
            newLayer.Row = newRow;
            newLayer.Row.Button.Click += delegate (object sender, RoutedEventArgs e) { Btn_SelectImages_Click(sender, e, newLayer); };

            layers.Add(newLayer);
            LoadImages();
        }

        private Image CreateNewImage(Layer newLayer)
        {
            Asset selectedAsset = (Asset)newLayer.Row.ComboBox.SelectedItem;
            newLayer.Asset = selectedAsset;

            Image image = new Image();
            var uriSource = new Uri(selectedAsset.Filepath);
            image.Source = new BitmapImage(uriSource);
            //image.Stretch = Stretch.Uniform;                        

            GridImages.Children.Add(image);

            //image.MouseEnter += LayerImage_MouseEnter;
            //image.MouseLeave += LayerImage_MouseLeave;

            //image.MouseUp += delegate (object sender, MouseButtonEventArgs e) { LayerImage_MouseUp(sender, e, newLayer); };
            //image.MouseDown += delegate (object sender, MouseButtonEventArgs e) { LayerImage_MouseDown(sender, e, newLayer); };
            //image.MouseMove += delegate (object sender, MouseEventArgs e) { LayerImage_MouseMove(sender, e, newLayer); };

            return image;
        }


        private void ChangeImageFilePath(Layer layer)
        {
            Asset selectedAsset = (Asset)layer.Row.ComboBox.SelectedItem;
            var uriSource = new Uri(selectedAsset.Filepath);
            layer.Image.Source = new BitmapImage(uriSource);
        }

        private void Ckb_Lock_Click(object sender, RoutedEventArgs e, Layer layer)
        {
            if ((bool)layer.Row.CBLock.IsChecked) layer.IsLocked = true;
            if (!(bool)layer.Row.CBLock.IsChecked) layer.IsLocked = false;
        }


        private void Ckb_Visibility_Click(object sender, RoutedEventArgs e, Layer layer)
        {
            if (layer.Image != null) SetLayerVisibility(layer);
        }

        private void Cbx_SelectionChanged(object sender, SelectionChangedEventArgs e, Layer layer)
        {
            ChangeImageFilePath(layer);
            SetLayerVisibility(layer);
        }

        private void SetLayerVisibility(Layer layer)
        {
            CheckBox cb = layer.Row.CBVisibility;

            if ((bool)cb.IsChecked) layer.Image.Visibility = Visibility.Visible;
            if (!(bool)cb.IsChecked) layer.Image.Visibility = Visibility.Collapsed;
        }

        // Not used at the moment
        #region ROW CONTROLS
        private void Row_MouseDown(object sender, MouseButtonEventArgs e, Layer layer)
        {
            List<Grid> rows = new List<Grid>();

            foreach (var l in layers)
            {
                rows.Add(l.Row.Grid);
            }

            foreach (Grid row in rows)
            {
                if (((SolidColorBrush)row.Background).Color == Colors.LightGray)
                {
                    row.Background = new SolidColorBrush(Color.FromArgb(100, 221, 221, 221));
                    row.Tag = "not active";
                }
            }

            layer.Row.Grid.Background = new SolidColorBrush(Colors.LightGray);
            layer.Row.Grid.Tag = "active";
        }

        private void Row_MouseLeave(object sender, MouseEventArgs e, Layer layer)
        {
            //if ((string)layer.Row.Grid.Tag == "active") return;
            Cursor = Cursors.Arrow;
            layer.Row.Grid.Background = new SolidColorBrush(Color.FromArgb(100, 221, 221, 221));
        }

        private void Row_MouseEnter(object sender, MouseEventArgs e, Layer layer)
        {
            Cursor = Cursors.Hand;
            layer.Row.Grid.Background = new SolidColorBrush(Colors.LightGray);
        }

        #endregion

        #region LAYER MOVING
        private void LayerImage_MouseEnter(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Hand;
        }

        private void LayerImage_MouseLeave(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Arrow;
        }

        private void LayerImage_MouseDown(object sender, MouseButtonEventArgs e, Layer layer)
        {
            foreach (var l in layers)
            {
                if (l.Image != null) l.Image.IsHitTestVisible = false;
            }

            var point = e.GetPosition(layer.Image);
            CheckPixelTransparencey(layer.Image, point);

            if (isOpaque)
            {
                if (layer.ImagePosition == null)
                    layer.ImagePosition = layer.Image.TransformToAncestor(GridImages).Transform(new Point(0, 0));
                var mousePosition = Mouse.GetPosition(GridImages);
                layer.DeltaX = mousePosition.X - layer.ImagePosition.Value.X;
                layer.DeltaY = mousePosition.Y - layer.ImagePosition.Value.Y;
                layer.IsMoving = true;

                layer.Image.IsHitTestVisible = true;
            }

            else return;
            
        }

        private void LayerImage_MouseUp(object sender, MouseButtonEventArgs e, Layer layer)
        {
            layer.CurrentTT = layer.Image.RenderTransform as TranslateTransform;
            layer.IsMoving = false;

            foreach (var l in layers)
            {
                if (l.Image != null) l.Image.IsHitTestVisible = true;
            }
        }

        private void CheckPixelTransparencey(Image img, Point p)
        {
            int x = (int)p.X;
            int y = (int)p.Y;

            Bitmap bmpOut = null;

            using (MemoryStream ms = new MemoryStream())
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create((BitmapSource)img.Source));
                encoder.Save(ms);

                using (Bitmap bmp = new Bitmap(ms))
                {
                    bmpOut = new Bitmap(bmp);
                }
            }


            var pixel = bmpOut.GetPixel(x, y); // Get pixel är tyligen väldigt långsamt ---
            if (pixel.A >= 200) isOpaque = true;
            else isOpaque = false;
        }

        private void LayerImage_MouseMove(object sender, MouseEventArgs e, Layer layer)
        {
            //if (layer.Image == null) return;
            if (!layer.IsMoving) return;
            if (layer.IsLocked) return;

            var point = e.GetPosition(layer.Image);

            CheckPixelTransparencey(layer.Image, point);

            if (isOpaque)
            {
                Cursor = Cursors.Hand;
                var mousePoint = Mouse.GetPosition(GridImages);

                var offsetX = (layer.CurrentTT == null ? layer.ImagePosition.Value.X : layer.ImagePosition.Value.X - layer.CurrentTT.X) + layer.DeltaX - mousePoint.X;
                var offsetY = (layer.CurrentTT == null ? layer.ImagePosition.Value.Y : layer.ImagePosition.Value.Y - layer.CurrentTT.Y) + layer.DeltaY - mousePoint.Y;

                layer.Image.RenderTransform = new TranslateTransform(-offsetX, -offsetY);
            }

            else { Cursor = Cursors.Arrow; }
        }

        #endregion

        #region ROW MOVING
        private void Sp_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is Grid)
            {
                foreach (var layer in layers)
                {
                    if (layer.Row.Grid == (Grid)e.Source)
                    {
                        movingRow = layer;
                    }
                }

                movingRow.Row.IsDown = true;
                movingRow.Row.StartPoint = e.GetPosition(sp);
            }

            else return;
        }

        private void Sp_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            movingRow.Row.IsDown = false;
            movingRow.Row.IsDragging = false;
            movingRow.Row.Grid.ReleaseMouseCapture();
        }

        private void Sp_MouseMove(object sender, MouseEventArgs e)
        {
            if (movingRow.Row.IsDown)
            {
                if ((movingRow.Row.IsDragging == false) && ((Math.Abs(e.GetPosition(sp).X - movingRow.Row.StartPoint.X) > SystemParameters.MinimumHorizontalDragDistance) ||
                    (Math.Abs(e.GetPosition(sp).Y - movingRow.Row.StartPoint.Y) > SystemParameters.MinimumVerticalDragDistance)))
                {
                    movingRow.Row.IsDragging = true;
                    movingRow.Row.Grid.CaptureMouse();
                    DragDrop.DoDragDrop(_dummyDragSource, new DataObject("Row", movingRow.Row.Grid, true), DragDropEffects.Move);
                }
            }
        }

        private void Sp_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("Row"))
            {
                e.Effects = DragDropEffects.Move;
            }
        }

        private void Sp_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("Row"))
            {
                UIElement droptarget = e.Source as UIElement;
                int droptargetIndex = -1, i = 0;

                foreach (var child in sp.Children)
                {
                    if (child is Grid && child.Equals(droptarget))
                    {
                        droptargetIndex = i;
                        break;
                    }
                    else i++;
                }

                if (droptargetIndex != -1)
                {
                    sp.Children.Remove(movingRow.Row.Grid);
                    sp.Children.Insert(droptargetIndex, movingRow.Row.Grid);

                    if (movingRow.Image != null)
                    {
                        GridImages.Children.Remove(movingRow.Image);
                        GridImages.Children.Insert(droptargetIndex, movingRow.Image);
                    }
                }

                movingRow.Row.IsDown = false;
                movingRow.Row.IsDragging = false;
                movingRow.Row.Grid.ReleaseMouseCapture();
            }
        }
        #endregion

        private void BtnBlack_Click(object sender, RoutedEventArgs e)
        {
            GridBackground.Background = new SolidColorBrush(Colors.Black);
        }

        private void BtnGray_Click(object sender, RoutedEventArgs e)
        {
            GridBackground.Background = new SolidColorBrush(Colors.Gray);
        }

        private void BtnWhite_Click(object sender, RoutedEventArgs e)
        {
            GridBackground.Background = new SolidColorBrush(Colors.White);
        }
        
    }
}
