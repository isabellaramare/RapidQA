using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Drawing;
using Point = System.Windows.Point;
using Color = System.Windows.Media.Color;
using Image = System.Windows.Controls.Image;
using Microsoft.Win32;
using System.Configuration;
using System.Collections.Specialized;
using ImSystem.Log;

namespace RapidQA
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Log Log { get; set; }

        readonly MainViewModel mvm = new MainViewModel();
        List<Layer> layers = new List<Layer>();
        List<MovingImage> movingImages = new List<MovingImage>(); // Warning! "infinite" lenght!
     
        Row row = new Row();
        string fileDirectory;
        private int createdLayers = 0;
        private bool imagesUpdated = false;
        private UIElement dummyDragSource = new UIElement();
        private Layer selectedLayer = new Layer();
        private bool xAxis;
        private bool yAxis;


        // HIGH PRIORITY
        // config file for automatic image-to-layer distribution https://support.microsoft.com/en-us/help/815786/how-to-store-and-retrieve-custom-information-from-an-application-confi                 
        // Save layer/image position for repeated use
        // show all, delete all, lock all <= make show all disabled if no images 

        // BUGS
        // weird white box appears when saving view
        // Change movement info (eg. CTRL + scroll)
        // Hela programmet hänger sig om man har många bilder i ett lager och bläddrar mellan dem. <= verkar bättre nu
        // Cursor is hand all over the place
        // Delete? Box stays after you press "yes" (not in my version?)
        // MovingImages relative position is sometimes saved several times in a row

        // EXTRAS
        // ability to move a layer with arrows
        // accesskeys (press 1 for activating layer 1, ctrl + S to save image, ctrl + Z to revert image movement) adding layer   
        // custom made button design (eye for visibility, padlock for locking)
        // different colors for layers
        // Snapping      
        // Save layers and images from last time the app was used
        // Rows should be possible to grab anywhere or atleast more obvious
        // Move images one axis at the time Works for Y but not X!


        public MainWindow()
        {
            InitializeComponent();
            DataContext = mvm;
            Log = LogWindow.Log;
            Log.AddInfo("Application loaded");     // HOTKEYS declaration should be available in some good place       
            Log.AddHeader("HOTKEYS");
            Log.AddInfo("Middle Mouse Button + Drag to pan");
            Log.AddInfo("CTRL + Scroll to zoom");
            Log.AddInfo("CTRL + S to save workarea");
            Log.AddInfo("CTRL + L to add a layer");
            Log.AddInfo("DEL to delete selected layer");
            Log.AddInfo("Hold Y to move image vertically");
            Log.AddInfo("Hold X to move image horizontally");
            //mvm.LoadFiles(folderPath);          
            BtnAddLayer_Click(null, null);
            ImageGrid.Background = new SolidColorBrush(ClrPcker_Background.SelectedColor);

            BtnAddLayer.Click += BtnAddLayer_Click;
            BtnSaveWorkArea.Click += BtnSaveWorkArea_Click;
            BtnSaveView.Click += BtnSaveView_Click;

            Btn_DeleteAll.Click += Btn_DeleteAll_Click;
            Ckb_AllVisible.Click += Ckb_AllVisible_Click;
            Ckb_AllLocked.Click += Ckb_AllLocked_Click;
            Ckb_AllVisible.IsChecked = true;
            Ckb_AllVisible.IsEnabled = false;

            ClrPcker_Background.ColorChanged += ClrPcker_Background_ColorChanged;
            workarea_width.TextChanged += Workarea_Width_TextChanged;
            workarea_height.TextChanged += Workarea_Height_TextChanged;
            
            sp.MouseMove += Sp_MouseMove;
            sp.MouseLeftButtonDown += Sp_MouseLeftButtonDown;
            sp.MouseLeftButtonUp += Sp_MouseLeftButtonUp;
            sp.DragEnter += Sp_DragEnter;
            sp.Drop += Sp_Drop;

            Zoom.MouseMove += Zoom_MouseMove;
            this.KeyDown += MainWindow_KeyDown;
            this.KeyUp += MainWindow_KeyUp;

            scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;    
        }

        private void Ckb_AllLocked_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Ckb_AllVisible_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)Ckb_AllVisible.IsChecked)
            {
                foreach (Layer l in layers)
                {
                    if (l.Image == null) return;
                    l.Image.Visibility = Visibility.Visible;
                    l.Row.CBVisibility.IsChecked = true;
                }
            }
            else
            {
                foreach (Layer l in layers)
                {
                    if (l.Image == null) return;
                    l.Image.Visibility = Visibility.Hidden;
                    l.Row.CBVisibility.IsChecked = false;
                }
            }
        }

        private void Btn_DeleteAll_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Delete all layers?", "Delete Layer", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes)
            {
                int la = layers.Count();
                for (int i = 0; i < la; i++)
                {
                    DeleteLayer(layers[0]);
                }                                
            }
        }

        private void MainWindow_KeyUp(object sender, KeyEventArgs e)
        {
            xAxis = false;
            yAxis = false;
        }

        int step = 0;
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                switch (e.Key)
                {
                    // Step back
                    case Key.Z:
                        if (step >= (movingImages.Count() - 1)) return;
                        step += 1;
                        Border bz = movingImages[step].Layer.Border;
                        Point pz = movingImages[step].RelativePosition;
                        bz.RenderTransform = new TranslateTransform(pz.X, pz.Y);
                        break;

                    // Go forward
                    case Key.Y:
                        if (step <= 0) return;
                        step -= 1;
                        Border by = movingImages[step].Layer.Border;
                        Point py = movingImages[step].RelativePosition;
                        by.RenderTransform = new TranslateTransform(py.X, py.Y);
                        break;

                    // Add new layer
                    case Key.L:
                        BtnAddLayer_Click(null, null);
                        break;

                    // Save workarea
                    case Key.S:                        
                        BtnSaveWorkArea_Click(null, null);
                        break;
                }
            }

            if (Keyboard.IsKeyDown(Key.X))
            {
                xAxis = true;
            }
            if (Keyboard.IsKeyDown(Key.Y))
            {
                yAxis = true;
            }
      
            if (Keyboard.IsKeyDown(Key.Delete))
            {               
                Btn_Delete_Click(null, null, selectedLayer);
            }
        }

        internal void DeleteLayer(Layer layer)
        {
            string layerName = layer.Row.Label.Text.ToString();           
            // delete image
            foreach (UIElement uIElement in ImageGrid.Children)
            {
                if (uIElement.Equals(layer.Border))
                {
                    ImageGrid.Children.Remove(uIElement);                    
                    break;
                }
            }

            // delete row
            foreach (Grid g in sp.Children)
            {
                if (g.Equals(layer.Row.Grid))
                {
                    sp.Children.Remove(g);
                    break;
                }
            }

            // delete from layers
            foreach (Layer l in layers)
            {
                if (l.Equals(layer))
                {
                    layers.Remove(l);
                    break;
                }
            }
            Log.AddSuccess("Deleted " + layerName);
        }

        private void Workarea_Height_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                double height = int.Parse(workarea_height.Text);

                foreach (Layer l in layers)
                {
                    if (l.Image.Source.Height >= height)
                    {                        
                        height = l.Image.Source.Height;
                    }                    
                }
                ImageGrid.Height = height;
                workarea_height.Text = height.ToString();
            }
            catch (Exception ex)
            {
                Log.AddError("Unable to resize " + ex);
            }
        }

        private void Workarea_Width_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                double width = int.Parse(workarea_width.Text);

                foreach (Layer l in layers)
                {
                    if (l.Image.Source.Width >= width)
                    {
                        width = l.Image.Source.Width;
                    }                   
                }
                ImageGrid.Width = width;
                workarea_width.Text = width.ToString();
            }
            catch (Exception ex)
            {
                Log.AddError("Unable to resize " + ex);
            }
        }

        private void ClrPcker_Background_ColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            ImageGrid.Background = new SolidColorBrush(ClrPcker_Background.SelectedColor);
        }

        private void Btn_Delete_Click(object sender, RoutedEventArgs e, Layer layer)
        {
            MessageBoxResult result = MessageBox.Show("Delete this layer?", "Delete Layer", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes)
            {                
                DeleteLayer(layer);
            }
        }
       
        private void Btn_SelectImages_Click(object sender, RoutedEventArgs e, Layer layer)
        {
            if (layer.Image is null)
            {
                List<Asset> assets = SelectImages();
                if (assets.Count > 0)
                {
                    row.AddRowComponents(layer, assets);
                    ComboBox cb = layer.Row.ComboBox;
                    cb.SelectionChanged += delegate (object s, SelectionChangedEventArgs ev) { Cbx_SelectionChanged(s, ev, layer); };
                    AddNewImage(layer);
                }
            } 
            else
            {
                imagesUpdated = true;
                layer.Row.ComboBox.ItemsSource = SelectImages();
                layer.Row.ComboBox.SelectedIndex = 0;
                ChangeImageFilepath(layer);                
            }

            imagesUpdated = false;
            MakeLayerSelected(layer);          
        }

        private List<Asset> SelectImages()
        {
            // Get the image files from the user.
            var openDlg = new Microsoft.Win32.OpenFileDialog();

            openDlg.Filter = "Image files(*.PNG;*.JPG;*.JPEG;*.TIFF)|*.PNG;*.JPG;*.JPEG;*.TIFF|All files (*.*)|*.*";
            if (fileDirectory == null) openDlg.InitialDirectory = "c:\\";
            if (fileDirectory != null) openDlg.InitialDirectory = fileDirectory;
            openDlg.Multiselect = true;

            bool? result = openDlg.ShowDialog(this);

            string[] selectedFiles = openDlg.FileNames;

            List<Asset> assets = new List<Asset>();
            foreach (string filepath in selectedFiles)
            {
                assets.Add(new Asset(filepath));
                Log.AddSuccess("Loaded Image - " + filepath);                
            }

            // Saves the directory that was used last
            if (assets.Count > 0)
            {
                fileDirectory = new FileInfo(assets[0].Filepath).Directory.FullName;
                Log.AddInfo("Saved Directory " + fileDirectory);
            }

            return assets;
        }

        private void ChangeImageFilepath(Layer layer)
        {            
            Asset selectedItem = (Asset)layer.Row.ComboBox.SelectedItem;
            var uriSource = new Uri(selectedItem.Filepath);
            layer.Image.Source = new BitmapImage(uriSource);

            Workarea_Height_TextChanged(null, null);
            Workarea_Width_TextChanged(null, null);            
        }      

        private void BtnAddLayer_Click(object sender, RoutedEventArgs e)
        {
            Layer layer = new Layer();
            Row newRow = row.CreateNewRow(createdLayers);
            createdLayers += 1;
            sp.Children.Add(newRow.Grid);
            layer.Row = newRow;
            AddEvents(layer);                       
            layers.Add(layer);
            Log.AddInfo("Created  " + layer.Row.Label.Text.ToString());
            MakeLayerSelected(layer);  
        }

        private void AddEvents(Layer layer)
        {
            Grid grid = layer.Row.Grid;          
            CheckBox vis = layer.Row.CBVisibility;
            CheckBox lo = layer.Row.CBLock;
            Button del = layer.Row.Delete;
            Button add = layer.Row.Button;

            grid.MouseMove += delegate (object sender, MouseEventArgs e) { RowMoveArea_MouseMove(sender, e, layer); };
            grid.MouseLeave += delegate (object sender, MouseEventArgs e) { RowMoveArea_MouseLeave(sender, e, layer); };

            add.Click += delegate (object s, RoutedEventArgs ev) { Btn_SelectImages_Click(s, ev, layer); };
            lo.Click += delegate (object sender, RoutedEventArgs e) { Ckb_Lock_Click(sender, e, layer); };
            vis.Click += delegate (object sender, RoutedEventArgs e) { Ckb_Visibility_Click(sender, e, layer); };
            del.Click += delegate (object sender, RoutedEventArgs e) { Btn_Delete_Click(sender, e, layer); };
        }

        private void AddNewImage(Layer layer)
        {           
            if (layer.Image != null)
            {                
                ImageGrid.Children.Remove(layer.Border);
            }

            Border border = new Border();

            Asset selectedAsset = (Asset)layer.Row.ComboBox.SelectedItem;
            layer.Asset = selectedAsset;

            Image image = new Image();
            var uriSource = new Uri(selectedAsset.Filepath);
            image.Source = new BitmapImage(uriSource);

            image.Stretch = Stretch.None;

            if (ImageGrid.Children.Count <= 1)
            {
                for (int i = 0; i <= 100; i++)
                {
                    ImageGrid.Children.Add(new UIElement());
                }
            }

            border.BorderBrush = new SolidColorBrush(Colors.Aqua);
            layer.Border = border;
            border.Child = image;

            // Image is added at same index as its layer
            for (int i = 0; i < layers.Count; i++)
            {
                if (layers[i] == layer)
                {
                    ImageGrid.Children.Insert(i, border);
                }
            }

            layer.Image = image;
            Log.AddInfo("Image added to " + layer.Row.Label.Text.ToString());
            Workarea_Height_TextChanged(null, null);
            Workarea_Width_TextChanged(null, null);
            SetLayerVisibility(layer);
            Ckb_AllVisible.IsEnabled = true;
        }

        private void Cbx_SelectionChanged(object sender, SelectionChangedEventArgs e, Layer layer)
        {
            if (imagesUpdated) return;
            ChangeImageFilepath(layer);
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
            if (!(bool)cb.IsChecked) layer.Image.Visibility = Visibility.Hidden;
        }

        #region SAVE IMAGE
        private void BtnSaveView_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = OpenFileSaveDialog();

            // show dialog
            if (dialog.ShowDialog().Value)
            {                             
                System.Drawing.Image image = ConvertImage(GridToRenderTargetBitmap(GridImages));
                image.Save(dialog.FileName);
                Log.AddSuccess("saved image - " + dialog.FileName);                
            }
        }

        private SaveFileDialog OpenFileSaveDialog()
        {
            SaveFileDialog dialog = new SaveFileDialog();

            dialog.Title = "Save image view";

            dialog.AddExtension = true;

            dialog.RestoreDirectory = true;

            dialog.Filter = "PNG images (*.png)|*.png";

            return dialog;
        }

        private void BtnSaveWorkArea_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = OpenFileSaveDialog();
          
            if (dialog.ShowDialog().Value)
            {
                try
                {
                    System.Drawing.Image image = ConvertImage(GridToRenderTargetBitmap(ImageGrid));
                    image.Save(dialog.FileName);
                    Log.AddSuccess("Saved image - " + dialog.FileName);
                    
                }
                catch (Exception ex)
                {
                    Log.AddError($"Failed to save image. {ex.Message}" );
                }               
            }
        }

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
        #endregion

        #region PIXEL COLOR
        private void Zoom_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                BitmapSource bitmapSource;
                bitmapSource = selectedLayer.Image.Source as BitmapSource;

                if (bitmapSource != null)
                {
                    // get color from bitmap pixel.
                    // convert coordinates from WPF pixels to Bitmap pixels and restrict them by the Bitmap bounds.

                    double x;
                    double y;

                    x = Mouse.GetPosition(selectedLayer.Image).X;
                    x *= bitmapSource.PixelWidth / selectedLayer.Image.ActualWidth;
                    y = Mouse.GetPosition(selectedLayer.Image).Y;
                    y *= bitmapSource.PixelHeight / selectedLayer.Image.ActualHeight;

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
        #endregion

        #region IMAGE MOVING
        private void LayerImage_MouseLeave(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Arrow;
            selectedLayer.Border.BorderThickness = new Thickness(0);
            
            if (selectedLayer.Image == null) return;
            selectedLayer.CurrentTT = selectedLayer.Border.RenderTransform as TranslateTransform;
            selectedLayer.IsMoving = false;
        }

        private void LayerImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (selectedLayer.Image == null) return;
            foreach (var l in layers)
            {
                if (l.Image != null) l.Image.IsHitTestVisible = false;
            }
      
            if (selectedLayer.ImagePosition == null)
                selectedLayer.ImagePosition = (selectedLayer.Border.TransformToAncestor(ImageGrid).Transform(new Point(0, 0)));
            var mousePosition = Mouse.GetPosition(ImageGrid);
            selectedLayer.DeltaX = mousePosition.X - selectedLayer.ImagePosition.X;
            selectedLayer.DeltaY = mousePosition.Y - selectedLayer.ImagePosition.Y;
            selectedLayer.IsMoving = true;

            selectedLayer.Image.IsHitTestVisible = true;       
        }

        private void LayerImage_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (selectedLayer.Image == null) return;
            selectedLayer.CurrentTT = selectedLayer.Border.RenderTransform as TranslateTransform;
            selectedLayer.IsMoving = false;

            SaveMovement();
        }

        private void SaveMovement()
        {            
            MovingImage mi = new MovingImage();
            mi.Layer = selectedLayer;
            if (movingImages.Count() == 0)
            {
                mi.RelativePosition = new Point(0, 0);
                movingImages.Add(mi);
            }
            else
            {
                // Gets image position realtive to parent
                Point relativeLocation = selectedLayer.Border.TranslatePoint(new Point(0, 0), ImageGrid);
                mi.RelativePosition = relativeLocation;
                //Log.AddInfo(relativeLocation.ToString());
                movingImages.Insert(step, mi);
            }
        }

        private void LayerImage_MouseMove(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Hand;
            selectedLayer.Border.BorderThickness = new Thickness(1);
            
            if (selectedLayer.Image == null) return;
            if (!selectedLayer.IsMoving) return;
            if (selectedLayer.IsLocked) return;
               
            var mousePoint = Mouse.GetPosition(ImageGrid);
            
            var offsetX = (selectedLayer.CurrentTT == null ? selectedLayer.ImagePosition.X : selectedLayer.ImagePosition.X - selectedLayer.CurrentTT.X) + selectedLayer.DeltaX - mousePoint.X;
            var offsetY = (selectedLayer.CurrentTT == null ? selectedLayer.ImagePosition.Y : selectedLayer.ImagePosition.Y - selectedLayer.CurrentTT.Y) + selectedLayer.DeltaY - mousePoint.Y;


            if (xAxis)
            {
                selectedLayer.Border.RenderTransform = new TranslateTransform(-offsetX, selectedLayer.CurrentTT.Y);
            }
            if (yAxis)
            {
                selectedLayer.Border.RenderTransform = new TranslateTransform(selectedLayer.CurrentTT.X, -offsetY);
            }
            
            else selectedLayer.Border.RenderTransform = new TranslateTransform(-offsetX, -offsetY);                       

        }
        #endregion

        #region ROW MOVING
        private void RowMoveArea_MouseLeave(object sender, MouseEventArgs e, Layer layer)
        {
            if (selectedLayer.Equals(layer))
            {
                layer.Row.Grid.Background = new LinearGradientBrush(
                    Color.FromArgb(100, 119, 119, 119),
                    Color.FromArgb(100, 221, 221, 221),
                    new Point(0, 0),
                    new Point(1, 1));                
            }
            else
            {
                layer.Row.Grid.Background = new SolidColorBrush(Color.FromArgb(100, 221, 221, 221));
            }
                Cursor = Cursors.Arrow;
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
            }
            if (column != 0)
            {
                Cursor = Cursors.Arrow;
            }

            layer.Row.Grid.Background = new LinearGradientBrush(
                Color.FromArgb(100, 119, 119, 119),
                Color.FromArgb(100, 221, 221, 221),
                new Point(0, 0),
                new Point(1, 1));
        }


        private void MakeLayerSelected(Layer layer)
        {
            selectedLayer = layer;

            foreach (Layer l in layers)
            {
                if (l.Image != null)
                {
                    l.Image.IsHitTestVisible = false;
                    l.Image.MouseLeave -= LayerImage_MouseLeave;
                    l.Image.MouseUp -= LayerImage_MouseUp;
                    l.Image.MouseDown -= LayerImage_MouseDown;
                    l.Image.MouseMove -= LayerImage_MouseMove;

                    l.Border.BorderThickness = new Thickness(0);
                }
                if (l.Equals(selectedLayer))
                {
                    l.Row.Grid.Background = new LinearGradientBrush(
                            Color.FromArgb(100, 119, 119, 119),
                            Color.FromArgb(100, 221, 221, 221),
                            new Point(0, 0),
                            new Point(1, 1));

                    if (l.Image != null)
                    {
                        
                        selectedLayer.Image.IsHitTestVisible = true;
                        selectedLayer.Image.MouseLeave += LayerImage_MouseLeave;
                        selectedLayer.Image.MouseUp += LayerImage_MouseUp;
                        selectedLayer.Image.MouseDown += LayerImage_MouseDown;
                        selectedLayer.Image.MouseMove += LayerImage_MouseMove;
                    }
                }
                else
                {
                    l.Row.Grid.Background = new SolidColorBrush(Color.FromArgb(100, 221, 221, 221));             
                }
            }
        }
        private void Sp_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is Grid)
            {
                foreach (var layer in layers)
                {
                    if (layer.Row.Grid == (Grid)e.Source)
                    {
                        MakeLayerSelected(layer);
                    }
                }
            }
            else return;
            
            if (e.Source is Grid)
            {                               
                selectedLayer.Row.IsDown = true;            
                selectedLayer.Row.StartPoint = e.GetPosition(sp);
            }
            else return;
        }

        private void Sp_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is Grid)
            {
                selectedLayer.Row.IsDown = false;
                selectedLayer.Row.IsDragging = false;
                selectedLayer.Row.Grid.ReleaseMouseCapture();
            }
            else return;
        }

        private void Sp_MouseMove(object sender, MouseEventArgs e)
        {
            // Drag and drop
            if (selectedLayer.Row.IsDown)
            {
                if ((selectedLayer.Row.IsDragging == false) && ((Math.Abs(e.GetPosition(sp).X - selectedLayer.Row.StartPoint.X) > SystemParameters.MinimumHorizontalDragDistance) ||
                    (Math.Abs(e.GetPosition(sp).Y - selectedLayer.Row.StartPoint.Y) > SystemParameters.MinimumVerticalDragDistance)))
                {
                    selectedLayer.Row.IsDragging = true;
                    selectedLayer.Row.Grid.CaptureMouse();
                    DragDrop.DoDragDrop(dummyDragSource, new DataObject("UIElement", selectedLayer.Row.Grid, true), DragDropEffects.Move);
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
                    sp.Children.Remove(selectedLayer.Row.Grid);
                    sp.Children.Insert(droptargetIndex, selectedLayer.Row.Grid);

                    foreach (Layer layer in layers)
                    {
                        if (layer.Equals(selectedLayer))
                        {
                            layers.Remove(layer);
                            layers.Insert(droptargetIndex, layer);
                            break;
                        }
                    }           

                    if (selectedLayer.Image != null)
                    {
                        ImageGrid.Children.Remove(selectedLayer.Border);
                        ImageGrid.Children.Insert(droptargetIndex, selectedLayer.Border);
                    }
                }

                selectedLayer.Row.IsDown = false;
                selectedLayer.Row.IsDragging = false;
                selectedLayer.Row.Grid.ReleaseMouseCapture();
            }
        }
        #endregion
    }
}
