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
using Microsoft.Win32;

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

        // HIGH PRIORITY
        // use Alpha channel to select/move layers https://stackoverflow.com/questions/2250965/wpf-cursor-on-a-partially-transparent-image
        // OR layer must be selected to be moved
        // save an image (whith all visible layers)
        // config file for automatic image-to-layer distribution
        // Added Zoom but now you can't move individual layers

        // EXTRAS
        // ability to move a layer with arrows
        // accesskeys (press 1 for activating layer 1, ctrl + S to save image, ctrl + Z to revert image movement)
        // rename layers
        // custom made button design (eye for visibility, padlock for locking)

        public MainWindow()
        {
            InitializeComponent();
            DataContext = mvm;
            //mvm.LoadFiles(folderPath);  
            BtnAddLayer_Click(null, null);

            BtnAddLayer.Click += BtnAddLayer_Click;
            BtnComposite.Click += BtnComposite_Click;
            BtnWhite.Click += BtnWhite_Click;
            BtnGray.Click += BtnGray_Click;
            BtnBlack.Click += BtnBlack_Click;

            sp.MouseMove += Sp_MouseMove;
            sp.MouseLeftButtonDown += Sp_MouseLeftButtonDown;
            sp.MouseLeftButtonUp += Sp_MouseLeftButtonUp;
            sp.DragEnter += Sp_DragEnter;
            sp.Drop += Sp_Drop;

            Zoom.MouseMove += Zoom_MouseMove;

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

                grid.MouseMove += delegate (object sender, MouseEventArgs e) { RowMoveArea_MouseMove(sender, e, layer); };
                grid.MouseLeave += delegate (object sender, MouseEventArgs e) { RowMoveArea_MouseLeave(sender, e, layer); };
                grid.MouseRightButtonDown += delegate (object sender, MouseButtonEventArgs e) { RowMoveArea_MouseRightButtonDown(sender, e, layer); };                                        

                cb.SelectionChanged += delegate (object sender, SelectionChangedEventArgs e) { Cbx_SelectionChanged(sender, e, layer); };
                lo.Click += delegate (object sender, RoutedEventArgs e) { Ckb_Lock_Click(sender, e, layer); };
                vis.Click += delegate (object sender, RoutedEventArgs e) { Ckb_Visibility_Click(sender, e, layer); };
            }
        }

        private void RowMoveArea_MouseRightButtonDown(object sender, MouseButtonEventArgs e, Layer layer)
        {
            foreach (Layer l in layers)
            {
                l.IsSelected = false;
                layer.Row.Grid.Background = new SolidColorBrush(Color.FromArgb(100, 221, 221, 221));
            }
            layer.Row.Grid.Background = new LinearGradientBrush(Color.FromArgb(100, 119, 119, 119), Color.FromArgb(100, 221, 221, 221), new Point(0, 0), new Point(1, 1));
            layer.IsSelected = true;            
        }

        private void RowMoveArea_MouseLeave(object sender, MouseEventArgs e, Layer layer)
        {
            if (!layer.IsSelected)
            {
                Cursor = Cursors.Arrow;
                layer.Row.Grid.Background = new SolidColorBrush(Color.FromArgb(100, 221, 221, 221));
            }
        }     

        private void RowMoveArea_MouseMove(object sender, MouseEventArgs e, Layer layer)
        {
            double x = e.GetPosition(layer.Row.Grid).X;
            double start = 0.0;
            int column = 0;
            foreach (ColumnDefinition cd in layer.Row.Grid.ColumnDefinitions)
            {
                start += cd.ActualWidth;
                if (x < start)
                {
                    break;
                }
                column++;
            }
            if (column == 0)
            {
                Cursor = Cursors.Hand;                
                layer.Row.Grid.Background = new LinearGradientBrush(Color.FromArgb(100, 119, 119, 119), Color.FromArgb(100, 221, 221, 221), new Point(0, 0), new Point(1, 1));                                        
            }
            if (column != 0 && layer.IsSelected == false) 
            {
                Cursor = Cursors.Arrow;
                layer.Row.Grid.Background = new SolidColorBrush(Color.FromArgb(100, 221, 221, 221));
            }
        }

        // Denna behöver fixas till, gamla bilder sysn fortfarande fast man valt nya
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
                sp.Children.Remove(layer.Row.ComboBox);                                      
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

            if (layer.Image == null)
            {
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
            }

            AddNewImage(layer);
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

        private void AddNewImage(Layer layer)
        {

            if (layer.Image != null) ImageGrid.Children.Remove(layer.Image);

            Asset selectedAsset = (Asset)layer.Row.ComboBox.SelectedItem;
            layer.Asset = selectedAsset;

            Image image = new Image();
            var uriSource = new Uri(selectedAsset.Filepath);
            image.Source = new BitmapImage(uriSource);

            ImageGrid.Children.Add(image);
            ImageGrid.Width = image.Source.Width;
            ImageGrid.Height = image.Source.Height;

            image.MouseEnter += LayerImage_MouseEnter;
            image.MouseLeave += delegate (object sender, MouseEventArgs e) { LayerImage_MouseLeave(sender, e, layer); };                

            image.MouseUp += delegate (object sender, MouseButtonEventArgs e) { LayerImage_MouseUp(sender, e, layer); };
            image.MouseDown += delegate (object sender, MouseButtonEventArgs e) { LayerImage_MouseDown(sender, e, layer); };
            image.MouseMove += delegate (object sender, MouseEventArgs e) { LayerImage_MouseMove(sender, e, layer); };

            layer.Image = image;
        }

        private void BtnComposite_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();

            dialog.Title = "Save image view";

            dialog.AddExtension = true;

            dialog.RestoreDirectory = true;

            dialog.Filter = "PNG images (*.png)|*.png";

            // show dialog

            if (dialog.ShowDialog().Value)
            {
                System.Drawing.Image image = ConvertImage(GridToRenderTargetBitmap(ImageGrid)); // Catch exception when there is no image (also, only saves layer 0)
                // Tar inte hänsyn till Zoom

                image.Save(dialog.FileName);
            }
        }

        // Om man använder den här kanske den tar hänsyn till zoom? - Nej
        private RenderTargetBitmap GridToRenderTargetBitmap(Grid grid)
        {
            Transform transform = grid.LayoutTransform;
            grid.LayoutTransform = null;

            Thickness margin = grid.Margin;
            grid.Margin = new Thickness(0, 0, margin.Right - margin.Left, margin.Bottom - margin.Top);

            System.Windows.Size size = new System.Windows.Size(grid.ActualWidth, grid.ActualHeight);

            grid.Measure(size);
            grid.Arrange(new Rect(size));

            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(
                (int)grid.ActualWidth,
                (int)grid.ActualHeight,
                96,
                96,
                PixelFormats.Pbgra32);

            renderTargetBitmap.Render(grid);
            grid.LayoutTransform = transform;
            grid.Margin = margin;
            return renderTargetBitmap;
        }

        private System.Drawing.Image ConvertImage(ImageSource image)
        {
            MemoryStream ms = new MemoryStream();
            var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
            encoder.Frames.Add(
                System.Windows.Media.Imaging.BitmapFrame.Create(image as System.Windows.Media.Imaging.BitmapSource));
            encoder.Save(ms);
            ms.Flush();
            return System.Drawing.Image.FromStream(ms);
        }

        // Fungerar ej just nu
        private void Zoom_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                BitmapSource bitmapSource;                                
                bitmapSource = layers.FirstOrDefault().Image.Source as BitmapSource;                

                if (bitmapSource != null)
                {
                    // get color from bitmap pixel.
                    // convert coordinates from WPF pixels to Bitmap pixels and restrict them by the Bitmap bounds.

                    double x;
                    double y;
     
                    x = Mouse.GetPosition(ZoomImage).X;
                    x *= bitmapSource.PixelWidth / ZoomImage.ActualWidth;
                    y = Mouse.GetPosition(ZoomImage).Y;
                    y *= bitmapSource.PixelHeight / ZoomImage.ActualHeight;

                    if ((int)x > bitmapSource.PixelWidth - 1)
                    {
                        x = bitmapSource.PixelWidth - 1;
                    }

                    else if (x < 1)
                    {
                        x = 0;
                    }
                    if ((int)y > bitmapSource.PixelHeight - 1)
                    {
                        y = bitmapSource.PixelHeight - 1;
                    }
                    else if (y < 1)
                    {
                        y = 0;
                    }

                    var pixels = new byte[16];

                    var stride = (bitmapSource.PixelWidth * bitmapSource.Format.BitsPerPixel + 7) / 8;

                    bitmapSource.CopyPixels(new Int32Rect((int)x, (int)y, 1, 1), pixels, stride, 0); // Goes to catch

                    // fill color rectangle

                    if (bitmapSource.Format == PixelFormats.Rgb24)
                    {
                        byte red = pixels[0];

                        byte green = pixels[1];

                        byte blue = pixels[2];

                        imageColorRectangle.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(red, green, blue));

                        // rgb

                        imageRedLabel.Content = red.ToString();

                        imageGreenLabel.Content = green.ToString();

                        imageBlueLabel.Content = blue.ToString();

                        // luminance 

                        double luminance = 0.2126 * Convert.ToInt32(red) + 0.7152 * Convert.ToInt32(green) + 0.0722 * Convert.ToInt32(blue);

                        imageLuminanceLabel.Content = Convert.ToInt32(luminance).ToString();
                    }
                    else if (bitmapSource.Format == PixelFormats.Bgr24 ||
                             bitmapSource.Format == PixelFormats.Bgr32 ||
                             bitmapSource.Format == PixelFormats.Bgra32 ||
                             bitmapSource.Format == PixelFormats.Pbgra32)
                    {
                        byte red = pixels[2];

                        byte green = pixels[1];

                        byte blue = pixels[0];

                        imageColorRectangle.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(red, green, blue));

                        // rgb

                        imageRedLabel.Content = red.ToString();

                        imageGreenLabel.Content = green.ToString();

                        imageBlueLabel.Content = blue.ToString();

                        // luminance 

                        double luminance = 0.2126 * Convert.ToInt32(red) + 0.7152 * Convert.ToInt32(green) + 0.0722 * Convert.ToInt32(blue);

                        imageLuminanceLabel.Content = Convert.ToInt32(luminance).ToString();
                    }
                    else if (bitmapSource.Format == PixelFormats.Rgba64)
                    {
                        UInt16 red = BitConverter.ToUInt16(pixels, 0);

                        UInt16 green = BitConverter.ToUInt16(pixels, 2);

                        UInt16 blue = BitConverter.ToUInt16(pixels, 4);

                        imageColorRectangle.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(Quantize16to8(red, UInt16.MaxValue),
                                                                                                          Quantize16to8(green, UInt16.MaxValue),
                                                                                                          Quantize16to8(blue, UInt16.MaxValue)));

                        // rgb

                        imageRedLabel.Content = red.ToString();

                        imageGreenLabel.Content = green.ToString();

                        imageBlueLabel.Content = blue.ToString();

                        // luminance 

                        double luminance = 0.2126 * Convert.ToInt32(red) + 0.7152 * Convert.ToInt32(green) + 0.0722 * Convert.ToInt32(blue);

                        imageLuminanceLabel.Content = Convert.ToInt32(luminance).ToString();
                    }
                    else
                    {
                        imageColorRectangle.Fill = new SolidColorBrush(Colors.Black);

                        imageRedLabel.Content = "0";

                        imageGreenLabel.Content = "0";

                        imageBlueLabel.Content = "0";
                    }
                }
            }
            catch (Exception)
            {
                
            }
        }
        private byte Quantize16to8(UInt16 d, UInt16 max)
        {
            return (byte)(((double)d / max) * 255.0);
        }

        private void ChangeComboBoxItemsSource(Layer layer, List<Asset> assets)
        {
            layer.Row.ComboBox.ItemsSource = assets;
            layer.Asset = (Asset)layer.Row.ComboBox.SelectedItem;            
        }

        private void Cbx_SelectionChanged(object sender, SelectionChangedEventArgs e, Layer layer)
        {
            AddNewImage(layer);
            SetLayerVisibility(layer);
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

        private void SetLayerVisibility(Layer layer)
        {
            CheckBox cb = layer.Row.CBVisibility;

            if ((bool)cb.IsChecked) layer.Image.Visibility = Visibility.Visible;
            if (!(bool)cb.IsChecked) layer.Image.Visibility = Visibility.Collapsed;
        }

        #region LAYER MOVING
        private void LayerImage_MouseEnter(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Hand;
        }

        private void LayerImage_MouseLeave(object sender, MouseEventArgs e, Layer layer)
        {
            Cursor = Cursors.Arrow;
            LayerImage_MouseUp(null, null, layer);
        }

        private void LayerImage_MouseDown(object sender, MouseButtonEventArgs e, Layer layer)
        {
            foreach (var l in layers)
            {
                if (l.Image != null) l.Image.IsHitTestVisible = false;
            }

            //var point = e.GetPosition(layer.Image);
            //CheckPixelTransparencey(layer.Image, point);

            //if (isOpaque)
            //{
                if (layer.ImagePosition == null)
                    layer.ImagePosition = layer.Image.TransformToAncestor(ImageGrid).Transform(new Point(0, 0));
                var mousePosition = Mouse.GetPosition(ImageGrid);
                layer.DeltaX = mousePosition.X - layer.ImagePosition.Value.X;
                layer.DeltaY = mousePosition.Y - layer.ImagePosition.Value.Y;
                layer.IsMoving = true;

                layer.Image.IsHitTestVisible = true;
            //}

            //else return;
            
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

        //private void CheckPixelTransparencey(Image img, Point p)
        //{
        //    int x = (int)p.X;
        //    int y = (int)p.Y;

        //    Bitmap bmpOut = null;

        //    using (MemoryStream ms = new MemoryStream())
        //    {
        //        PngBitmapEncoder encoder = new PngBitmapEncoder();
        //        encoder.Frames.Add(BitmapFrame.Create((BitmapSource)img.Source));
        //        encoder.Save(ms);

        //        using (Bitmap bmp = new Bitmap(ms))
        //        {
        //            bmpOut = new Bitmap(bmp);
        //        }
        //    }


        //    var pixel = bmpOut.GetPixel(x, y); // Get pixel är tyligen väldigt långsamt ---
        //    if (pixel.A >= 200) isOpaque = true;
        //    else isOpaque = false;
        //}

        private void LayerImage_MouseMove(object sender, MouseEventArgs e, Layer layer)
        {
            Cursor = Cursors.Hand;
            //if (layer.Image == null) return;
            if (!layer.IsMoving) return;
            if (layer.IsLocked) return;
               
            var mousePoint = Mouse.GetPosition(ImageGrid);
            
            var offsetX = (layer.CurrentTT == null ? layer.ImagePosition.Value.X : layer.ImagePosition.Value.X - layer.CurrentTT.X) + layer.DeltaX - mousePoint.X;
            var offsetY = (layer.CurrentTT == null ? layer.ImagePosition.Value.Y : layer.ImagePosition.Value.Y - layer.CurrentTT.Y) + layer.DeltaY - mousePoint.Y;

            layer.Image.RenderTransform = new TranslateTransform(-offsetX, -offsetY);                       
        }

        #endregion

        #region ROW MOVING
        private void Sp_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is Grid)
            {               
                movingRow.Row.IsDown = true;            
                movingRow.Row.StartPoint = e.GetPosition(sp);

                foreach (Layer l in layers)
                {
                    l.IsSelected = false;
                    l.Row.Grid.Background = new SolidColorBrush(Color.FromArgb(100, 221, 221, 221));
                }
                movingRow.Row.Grid.Background = new LinearGradientBrush(Color.FromArgb(100, 119, 119, 119), Color.FromArgb(100, 221, 221, 221), new Point(0, 0), new Point(1, 1));
                movingRow.IsSelected = true;
            }
            else return;
        }

        private void Sp_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is Grid)
            {
                movingRow.Row.IsDown = false;
                movingRow.Row.IsDragging = false;
                movingRow.Row.Grid.ReleaseMouseCapture(); // error occurs if you drag an image "onto" sp and release it  
            }
            else return;
        }

        private void Sp_MouseMove(object sender, MouseEventArgs e)
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
            }
            else return;

            // Drag and drop
            if (movingRow.Row.IsDown)
            {
                if ((movingRow.Row.IsDragging == false) && ((Math.Abs(e.GetPosition(sp).X - movingRow.Row.StartPoint.X) > SystemParameters.MinimumHorizontalDragDistance) ||
                    (Math.Abs(e.GetPosition(sp).Y - movingRow.Row.StartPoint.Y) > SystemParameters.MinimumVerticalDragDistance)))
                {
                    movingRow.Row.IsDragging = true;
                    movingRow.Row.Grid.CaptureMouse();
                    DragDrop.DoDragDrop(_dummyDragSource, new DataObject("UIElement", movingRow.Row.Grid, true), DragDropEffects.Move);
                }
            }
        }

        private void Sp_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("UIElement"))
            {
                e.Effects = DragDropEffects.Move;
            }
        }

        private void Sp_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("UIElement"))
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
                        ImageGrid.Children.Remove(movingRow.Image);
                        ImageGrid.Children.Insert(droptargetIndex, movingRow.Image);
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
            Zoom.Background = new SolidColorBrush(Colors.Black);
        }

        private void BtnGray_Click(object sender, RoutedEventArgs e)
        {
            Zoom.Background = new SolidColorBrush(Colors.Gray);
        }

        private void BtnWhite_Click(object sender, RoutedEventArgs e)
        {
            Zoom.Background = new SolidColorBrush(Colors.White);
        }
        
    }
}
