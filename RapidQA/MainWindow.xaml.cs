using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using Point = System.Windows.Point;
using Color = System.Windows.Media.Color;
using Image = System.Windows.Controls.Image;
using Microsoft.Win32;
using ImSystem.Log;
using System.Xml.Serialization;
using System.Windows.Documents;

namespace RapidQA
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Log Log { get; set; }
        List<Layer> layers = new List<Layer>();
        Dictionary<Layer, List<Point>> Undo = new Dictionary<Layer, List<Point>>();
        Dictionary<Layer, List<Point>> Redo = new Dictionary<Layer, List<Point>>();
        Row row = new Row();
        string fileDirectory;
        private bool imagesUpdated = false;
        private UIElement dummyDragSource = new UIElement();
        private Layer selectedLayer = new Layer();
        private bool moveXAxis;
        private bool moveYAxis;
        private bool snapping = false;

        // HIGH PRIORITY
        // config file for automatic image-to-layer distribution https://support.microsoft.com/en-us/help/815786/how-to-store-and-retrieve-custom-information-from-an-application-confi                     

        // BUGS
        // ctrl + Z only works for selected layer

        // EXTRAS         
        // different colors for layers
        // Snapping => kinda works but the layers snap to different "grids"...   
        // Make several layers be selectable at once
        // Gridsplitter so that layerlabel can be scalable
        // save background color in preset

        public MainWindow()
        {
            InitializeComponent();        
            Log = LogWindow.Log;
            Log.AddInfo("Application loaded");

            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            Workarea.Background = new SolidColorBrush(ClrPcker_Background.SelectedColor);

            BtnAddLayer_Click(null, null);
            workarea_width.IsEnabled = false;
            workarea_height.IsEnabled = false;
            
            BtnAddLayer.Click += BtnAddLayer_Click;
            BtnSaveImage.Click += BtnSaveImage_Click;
            BtnSaveView.Click += BtnSaveView_Click;
            BtnCopyView.Click += BtnCopyView_Click;
            BtnCopyImage.Click += BtnCopyImage_Click;
            BtnSavePreset.Click += BtnSavePreset_Click;
            BtnLoadPreset.Click += BtnLoadPreset_Click;
            BtnInfo.Click += BtnInfo_Click;
            BtnExpand.Click += BtnExpand_Click;
            BtnDeleteAll.Click += Btn_DeleteAll_Click;

            Ckb_AllVisible.Click += Ckb_AllVisible_Click;
            Ckb_AllLocked.Click += Ckb_AllLocked_Click;
            Ckb_AllVisible.IsChecked = true;
            Ckb_AllVisible.IsEnabled = false;         
            Ckb_AllLocked.IsEnabled = false;
            CkbToggleLog.Click += CkbToggleLog_Click;

            ClrPcker_Background.ColorChanged += ClrPcker_Background_ColorChanged;          
            
            sp.MouseMove += Sp_MouseMove;
            sp.MouseLeftButtonDown += Sp_MouseLeftButtonDown;
            sp.MouseLeftButtonUp += Sp_MouseLeftButtonUp;
            sp.DragEnter += Sp_DragEnter;
            sp.Drop += Sp_Drop;

            Zoom.MouseMove += Zoom_MouseMove;
            this.PreviewKeyDown += MainWindow_PreviewKeyDown;
            this.KeyUp += MainWindow_KeyUp;
            this.Closed += MainWindow_Closed;

            scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
        }

        private void AddEvents(Layer layer)
        {
            Grid grid = layer.Row.Grid;
            CheckBox vis = layer.Row.CBVisibility;
            CheckBox lo = layer.Row.CBLock;
            Button del = layer.Row.Delete;
            Button add = layer.Row.Button;
            TextBox lab = layer.Row.Label;

            grid.MouseMove += delegate (object sender, MouseEventArgs e) 
            { RowMoveArea_MouseMove(sender, e, layer); };
            grid.MouseLeave += delegate (object sender, MouseEventArgs e) 
            { RowMoveArea_MouseLeave(sender, e, layer); };
            grid.Loaded += delegate (object sender, RoutedEventArgs e)
            { Grid_Loaded(sender, e, layer); };
            add.Click += delegate (object s, RoutedEventArgs ev) 
            { Btn_SelectImages_Click(s, ev, layer); };
            lo.Click += delegate (object sender, RoutedEventArgs e) 
            { Ckb_Lock_Click(sender, e, layer); };
            vis.Click += delegate (object sender, RoutedEventArgs e) 
            { Ckb_Visibility_Click(sender, e, layer); };
            del.Click += delegate (object sender, RoutedEventArgs e) 
            { Btn_Delete_Click(sender, e, layer); };           
            lab.LostKeyboardFocus += delegate (object sender, KeyboardFocusChangedEventArgs e)
            { Label_LostKeyboardFocus(sender, e, layer); };
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e, Layer layer)
        {
            AdornerLayer.GetAdornerLayer(layer.Row.Grid).Add(new DottedLineAdorner(layer.Row.Drag));
        }

        bool showInfo = false;
        private void BtnInfo_Click(object sender, RoutedEventArgs e)
        {
            if (showInfo)
            {
                BorderInfo.Visibility = Visibility.Collapsed;
                showInfo = false;
            }
            else
            {
                showInfo = true;
                BorderInfo.Visibility = Visibility.Visible;
                TxtInfo.Text =
                "HOTKEYS\n" +
                "Middle Mouse Button + Drag to pan\n" +
                "CTRL + Scroll to zoom\n" +
                "CTRL + S to save workarea\n" +
                "CTRL + L to add a layer\n" +
                "DEL to delete selected layer\n" +
                "Hold Y to move image vertically\n" +
                "Hold X to move image horizontally\n" +
                "CTRL + Z to undo\n" +
                "CTRL + Y to redo\n" +
                "Press 1-9 to select layers\n" +
                "Press L to toggle Log\n" +
                "Press Q to reset image position\n" +
                "Hold Z for snapping\n" +
                "Press I to toggle this"; 
            }
        }

        private void CkbToggleLog_Click(object sender, RoutedEventArgs e)
        {            
            if ((bool)CkbToggleLog.IsChecked)
            {
                EnableLog();
            }
            else
            {
                DisableLog();
            }
        }

        private void EnableLog()
        {
            CkbToggleLog.IsChecked = true;
            var rowdef = GridImages.RowDefinitions[3];
            rowdef.Height = new GridLength(200);
            LogWindow.ShowLog();
            LogWindow.Height = 200;
        }

        private void DisableLog()
        {
            var rowdef = GridImages.RowDefinitions[3];
            rowdef.Height = new GridLength(5);
            LogWindow.HideLog();
            LogWindow.Height = 5;
        }

        private void Ckb_AllLocked_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)Ckb_AllLocked.IsChecked)
            {
                foreach (Layer layer in layers)
                {
                    layer.IsLocked = true;
                    layer.Row.CBLock.IsChecked = true;
                }
            }
            if (!(bool)Ckb_AllLocked.IsChecked)
            {
                foreach (Layer layer in layers)
                {
                    layer.IsLocked = false;
                    layer.Row.CBLock.IsChecked = false;
                }
            }
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
            MessageBoxResult result = MessageBox.Show(
                "Delete all layers?", 
                "Delete Layer", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question, 
                MessageBoxResult.No);

            if (result == MessageBoxResult.No){ return; }

            if (result == MessageBoxResult.Yes)
            {                
                foreach (Layer l in layers)
                {
                    DeleteLayer(l);
                }

                layers.Clear();
                EnableWorkareaScaling();
                workarea_height.Text = "0";
                workarea_width.Text = "0";
            }
        }

        internal void DeleteLayer(Layer layer)
        {
            string layerName = layer.Row.Label.Text.ToString();           
            
            // delete image
            foreach (UIElement uIElement in Workarea.Children)
            {
                if (uIElement.Equals(layer.Border))
                {
                    Workarea.Children.Remove(uIElement);                    
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

            Log.AddSuccess("Deleted " + layerName);
            Redo.Clear();
            Undo.Clear();
        }

        private void Btn_Delete_Click(object sender, RoutedEventArgs e, Layer layer)
        {
            MessageBoxResult result = MessageBox.Show(
                "Delete this layer?", 
                "Delete Layer", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question, 
                MessageBoxResult.No);

            if (result == MessageBoxResult.No) return;
            
            if (result == MessageBoxResult.Yes)
            {                
                DeleteLayer(layer);
                layers.Remove(layer);
                EnableWorkareaScaling();
                SetWorkareaWidth();
                SetWorkareaHeight();
            }
        }

        private void ClrPcker_Background_ColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            Workarea.Background = new SolidColorBrush(ClrPcker_Background.SelectedColor);
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
                    layer.Assets = assets;
                    cb.SelectionChanged += delegate (object s, SelectionChangedEventArgs ev) 
                    { Cbx_SelectionChanged(s, ev, layer); };
                    AddImage(layer);
                }
            }
            else
            {
                imagesUpdated = true;
                List<Asset> assets = SelectImages();
                if (assets.Count > 0)
                {
                    layer.Assets = assets;
                    layer.Row.ComboBox.ItemsSource = assets;
                    layer.Row.ComboBox.SelectedIndex = 0;
                    ChangeImageFilepath(layer);
                }              
            }
            imagesUpdated = false;
            MakeLayerSelected(layer);
        }

        private void ChangeImageFilepath(Layer layer)
        {
            Asset selectedItem = (Asset)layer.Row.ComboBox.SelectedItem;
            //var uriSource = new Uri(selectedItem.Filepath);
            //layer.Image.Source = new BitmapImage(uriSource);
            layer.Image.Source = GetBitmapImage(selectedItem.Filepath);

            layer.Border.Width = selectedLayer.Image.Source.Width;
            layer.Border.Height = selectedLayer.Image.Source.Height;
            SetWorkareaHeight();
            SetWorkareaWidth();
        }

        private BitmapImage GetBitmapImage(string filepath)
        {
            var bitmap = new BitmapImage();
            var stream = File.OpenRead(filepath);

            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            stream.Close();
            stream.Dispose();
            return bitmap;
        }

        private void BtnAddLayer_Click(object sender, RoutedEventArgs e)
        {
            Layer layer = new Layer();
            if (layers.Count >= 100)
            {
               MessageBoxResult result = MessageBox.Show(
               "Can't add more layers.",
               "Layer limit reached.",
               MessageBoxButton.OK,
               MessageBoxImage.Error,
               MessageBoxResult.OK);
               Log.AddError("Can't add more layers. Layer limit reached.");
            }
            else
            {
                layers.Add(layer);
                AddLayer(layer);
                MakeLayerSelected(layer);
            }
        }

        private void AddLayer(Layer layer)
        {
            Row newRow = row.CreateNewRow(layers.Count);
            sp.Children.Add(newRow.Grid);
            layer.Row = newRow;
            if (layer.Name == null) layer.Name = layer.Row.Label.Text;
            else layer.Row.Label.Text = layer.Name;
            AddEvents(layer);            
            Log.AddInfo("Created " + layer.Name);       
        }

        private void Label_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e, Layer l)
        {
            l.Row.Label.SelectionStart = 0;
            l.Name = l.Row.Label.Text;
        }

        private void AddImage(Layer layer)
        {
            if (layer.Image != null)
            {
                Workarea.Children.Remove(layer.Border);
            }

            Border border = new Border();
            Asset selectedAsset = (Asset)layer.Row.ComboBox.SelectedItem;
            Image image = new Image();
            var bmp = GetBitmapImage(selectedAsset.Filepath);
            image.Source = bmp;
            image.Stretch = Stretch.None;

            if (Workarea.Children.Count == 0)
            {
                for (int i = 0; i <= 100; i++)
                {
                    Workarea.Children.Add(new UIElement());
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
                    Workarea.Children.Insert(i, border);
                }
            }

            layer.Image = image;
            Log.AddInfo("Image added to " + layer.Row.Label.Text.ToString());
            SetWorkareaWidth();
            SetWorkareaHeight();
            SetLayerVisibility(layer);
            EnableWorkareaScaling();
            Ckb_AllVisible.IsEnabled = true;
            Ckb_AllLocked.IsEnabled = true;

            layer.Border.Width = layer.Image.Source.Width;
            layer.Border.Height = layer.Image.Source.Height;
        }

        private void Cbx_SelectionChanged(object sender, SelectionChangedEventArgs e, Layer layer)
        {
            if (imagesUpdated) return;
            ChangeImageFilepath(layer);
            SetLayerVisibility(layer);
            layer.SelectedIndex = layer.Row.ComboBox.SelectedIndex;
            MakeLayerSelected(layer);
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

        #region WORKAREA
        private void EnableWorkareaScaling()
        {
            bool hasImage = false;
            foreach (Layer l in layers)
            {
                if (l.Image != null) hasImage = true;
            }

            if (hasImage)
            {
                workarea_height.IsEnabled = true;
                workarea_width.IsEnabled = true;
            }
            else
            {
                workarea_height.Text = "0";
                workarea_width.Text = "0";
                Workarea.Height = 0;
                Workarea.Width = 0;
                workarea_height.IsEnabled = false;
                workarea_width.IsEnabled = false;
            }
        }

        private void SetWorkareaWidth()
        {
            string text = workarea_width.Text;
            int widest = 0;
            int width = 0;

            try
            {
                width = int.Parse(text);
            }
            catch (Exception ex)
            {
                Log.AddWarning($"Unable to scale workarea {ex}");
            }

            foreach (Layer l in layers)
            {
                if (l.Image == null) return;
                if (l.Image.Source.Width > widest)
                {
                    double d = l.Image.Source.Width;
                    int i = Convert.ToInt32(d);
                    widest = i;
                }
            }

            if (width >= 50000) width = 50000;
            if (widest >= 50000) widest = 50000;

            if (width <= widest || text == "" || text == null)
            {
                Workarea.Width = widest;
                workarea_width.Text = widest.ToString();
            }

            if (width >= widest)
            {
                Workarea.Width = width;
                workarea_width.Text = width.ToString();
            }
        }

        private void SetWorkareaHeight()
        {
            string text = workarea_height.Text;
            int highest = 0;
            int height = 0;

            try
            {
                height = int.Parse(text);
            }
            catch (Exception ex)
            {
                Log.AddWarning($"Unable to scale workarea {ex}");
            }

            foreach (Layer l in layers)
            {
                if (l.Image == null) return;
                if (l.Image.Source.Height > highest)
                {
                    double d = l.Image.Source.Height;
                    int i = Convert.ToInt32(d);
                    highest = i;
                }
            }

            if (height >= 50000) height = 50000;
            if (highest >= 50000) highest = 50000;

            if (height < highest || text == "" || text == null)
            {
                Workarea.Height = highest;
                workarea_height.Text = highest.ToString();
            }

            if (height > highest)
            {
                Workarea.Height = height; 
                workarea_height.Text = height.ToString();
            }

        }
        #endregion

        #region HOTKEYS        
        private void MainWindow_KeyUp(object sender, KeyEventArgs e)
        {
            moveXAxis = false;
            moveYAxis = false;
            snapping = false;
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            foreach(Layer l in layers)
            {
                if (l.Row.Label.IsFocused) return;

                if (Keyboard.IsKeyDown(Key.Z))
                {
                    snapping = true;
                }

                //if (Keyboard.IsKeyDown(Key.Q))
                //{
                //    selectedLayer.Border.RenderTransform = new TranslateTransform(0, 0);
                //    selectedLayer.CurrentTT = new TranslateTransform(0, 0);
                //    SaveImagePosition();
                //}

                if (Keyboard.IsKeyDown(Key.I))
                {
                    BtnInfo_Click(null, null);
                }
            }          

            if (!Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.L))
            {
                if ((bool)CkbToggleLog.IsChecked) CkbToggleLog.IsChecked = false;
                else CkbToggleLog.IsChecked = true;
                CkbToggleLog_Click(null, null);
            }

            if (workarea_height.IsFocused || workarea_width.IsFocused)
            {
                if (Keyboard.IsKeyDown(Key.Enter))
                {
                    SetWorkareaHeight();
                    SetWorkareaWidth();
                }
            }

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                switch (e.Key)
                {
                    // Step back
                    case Key.Z:
                        if (!Undo.ContainsKey(selectedLayer) || Undo[selectedLayer].Count() == 0) return;
                        var c = Undo[selectedLayer].Count;

                        // sends the last/current position to Redo
                        var forRedo = Undo[selectedLayer].Last();

                        if (Redo.ContainsKey(selectedLayer))
                        {
                            Redo[selectedLayer].Add(forRedo);
                        }
                        else
                        {
                            Redo.Add(selectedLayer, new List<Point>() { forRedo });
                        }

                        // And removes it from undo
                        Undo[selectedLayer].RemoveAt(c - 1);

                        // Moves the layer to the second to last (now last) position 
                        if (Undo[selectedLayer].Count() == 0) return;
                        var lastPos = Undo[selectedLayer].Last();
                        selectedLayer.Border.RenderTransform = new TranslateTransform(lastPos.X, lastPos.Y);
                        selectedLayer.CurrentTT = new TranslateTransform(lastPos.X, lastPos.Y);
                        break;

                    // Go forward
                    case Key.Y:
                        if (!Redo.ContainsKey(selectedLayer) || Redo[selectedLayer].Count() == 0) return;
                        var count = Redo[selectedLayer].Count;
                        var forUndo = Redo[selectedLayer].Last();
                        Undo[selectedLayer].Add(forUndo);

                        if (Redo[selectedLayer].Count() == 0) return;
                        var lP = Redo[selectedLayer].Last();
                        selectedLayer.Border.RenderTransform = new TranslateTransform(lP.X, lP.Y);

                        Redo[selectedLayer].RemoveAt(count - 1);
                        break;

                    // Add new layer
                    case Key.L:
                        BtnAddLayer_Click(null, null);
                        break;

                    // Save workarea
                    case Key.S:
                        BtnSaveImage_Click(null, null);
                        break;
                }
            }

            if (workarea_height.IsFocused || workarea_width.IsFocused) return;
            else
            {         
                switch (e.Key)
                {
                    case Key.X:
                        moveXAxis = true;
                        break;

                    case Key.Y:
                        moveYAxis = true;
                        break;

                    case Key.Delete:
                        Btn_Delete_Click(null, null, selectedLayer);
                        break;

                    case Key.D1:
                        MakeLayerSelected(layers[0]);
                        break;
                    case Key.D2:
                        MakeLayerSelected(layers[1]);
                        break;
                    case Key.D3:
                        MakeLayerSelected(layers[2]);
                        break;
                    case Key.D4:
                        MakeLayerSelected(layers[3]);
                        break;
                    case Key.D5:
                        MakeLayerSelected(layers[4]);
                        break;
                    case Key.D6:
                        MakeLayerSelected(layers[5]);
                        break;
                    case Key.D7:
                        MakeLayerSelected(layers[6]);
                        break;
                    case Key.D8:
                        MakeLayerSelected(layers[7]);
                        break;
                    case Key.D9:
                        MakeLayerSelected(layers[8]);
                        break;
                }

                foreach (Layer l in layers)
                {
                    if (l.Row.ComboBox.IsFocused) return;
                }

                // nudgeing lenght
                int pixels = 5;

                if (selectedLayer.Border != null && !selectedLayer.IsLocked && selectedLayer.CurrentTT != null) // <= Not working
                {
                    if (Keyboard.IsKeyDown(Key.Left))
                    {
                        selectedLayer.Border.RenderTransform = new TranslateTransform((selectedLayer.CurrentTT.X - pixels), selectedLayer.CurrentTT.Y);
                        selectedLayer.CurrentTT = new TranslateTransform((selectedLayer.CurrentTT.X - pixels), selectedLayer.CurrentTT.Y);
                        SaveImagePosition();
                    }
                    if (Keyboard.IsKeyDown(Key.Right))
                    {
                        selectedLayer.Border.RenderTransform = new TranslateTransform((selectedLayer.CurrentTT.X + pixels), selectedLayer.CurrentTT.Y);
                        selectedLayer.CurrentTT = new TranslateTransform((selectedLayer.CurrentTT.X + pixels), selectedLayer.CurrentTT.Y);
                        SaveImagePosition();
                    }
                    if (Keyboard.IsKeyDown(Key.Up))
                    {
                        selectedLayer.Border.RenderTransform = new TranslateTransform(selectedLayer.CurrentTT.X, (selectedLayer.CurrentTT.Y - pixels));
                        selectedLayer.CurrentTT = new TranslateTransform(selectedLayer.CurrentTT.X, (selectedLayer.CurrentTT.Y - pixels));
                        SaveImagePosition();
                    }
                    if (Keyboard.IsKeyDown(Key.Down))
                    {
                        selectedLayer.Border.RenderTransform = new TranslateTransform(selectedLayer.CurrentTT.X, (selectedLayer.CurrentTT.Y + pixels));
                        selectedLayer.CurrentTT = new TranslateTransform(selectedLayer.CurrentTT.X, (selectedLayer.CurrentTT.Y + pixels));
                        SaveImagePosition();
                    }
                }
            }
        }
        #endregion

        #region WINDOWS
        private void MainWindow_Closed(object sender, EventArgs e)
        {
            if (isExpanded) window.Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            DockWindow();
        }

        Window window;
        bool isExpanded = false;
        private void BtnExpand_Click(object sender, RoutedEventArgs e)
        {
            if (isExpanded) DockWindow();
            else FloatWindow();
        }

        private void DockWindow()
        {
            BtnExpand.Content = "[ ]";
            var content = window.Content;
            window.Content = null;
            MainGrid.Children.Add(GridImages);
            window.Hide();
            GrdMenu.SetValue(Grid.ColumnSpanProperty, 1);
            Splitter.Visibility = Visibility.Visible;
            this.Width = 1200;
            isExpanded = false;
        }

        private void FloatWindow()
        {
            window = new Window();
            window.Closed += Window_Closed;

            MainGrid.Children.Remove(GridImages);
            window.Content = GridImages;
            BtnExpand.Content = "[]";
            window.Show();
            GrdMenu.SetValue(Grid.ColumnSpanProperty, 3);
            Splitter.Visibility = Visibility.Collapsed;
            this.Width = 300;
            isExpanded = true;
        }

        #endregion

        #region SAVE & LOAD
        private void BtnCopyImage_Click(object sender, RoutedEventArgs e)
        {
            RenderTargetBitmap renderTarget = GridToRenderTargetBitmap(Workarea);
            Image img = new Image();
            img.Source = renderTarget;
            BitmapSource bitmap = (BitmapSource)img.Source;

            SwapClipboardImage(bitmap);
        }

        private void BtnCopyView_Click(object sender, RoutedEventArgs e)
        {
            RenderTargetBitmap renderTarget = GridToRenderTargetBitmap(ViewGrid);
            Image img = new Image();
            img.Source = renderTarget;
            BitmapSource bitmap = (BitmapSource)img.Source;
            SwapClipboardImage(bitmap);
            Height += 1; 
        }

        public void SwapClipboardImage(BitmapSource replacementImage)
        {
            Clipboard.SetImage(replacementImage);
        }

        OpenFileDialog openDlg = new Microsoft.Win32.OpenFileDialog();
        private List<Asset> SelectImages()
        {
            // Get the image files from the user.
            var openDlg = new Microsoft.Win32.OpenFileDialog();

            openDlg.Filter = "Image files(*.PNG;*.JPG;*.JPEG;*.TIFF)|*.PNG;*.JPG;*.JPEG;*.TIFF|All files (*.*)|*.*";
            if (fileDirectory == null) openDlg.InitialDirectory = "c:\\";
            if (fileDirectory != null) openDlg.InitialDirectory = fileDirectory;
            openDlg.Multiselect = true;
            
            bool? result = openDlg.ShowDialog();

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

        private void BtnSaveView_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = OpenFileSaveDialog();

            // show dialog
            if (dialog.ShowDialog().Value)
            {                             
                System.Drawing.Image image = ConvertImage(GridToRenderTargetBitmap(ViewGrid));
                image.Save(dialog.FileName);
                Log.AddSuccess("Saved image " + dialog.FileName);                
            }
            Height += 1;
        }

        private SaveFileDialog OpenFileSaveDialog()
        {
            SaveFileDialog dialog = new SaveFileDialog();

            dialog.Title = "Save image";

            dialog.AddExtension = true;

            dialog.RestoreDirectory = true;

            dialog.Filter = "PNG images (*.png)|*.png";

            return dialog;
        }

        private void BtnSaveImage_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = OpenFileSaveDialog();
          
            if (dialog.ShowDialog().Value)
            {
                try
                {
                    System.Drawing.Image image = ConvertImage(GridToRenderTargetBitmap(Workarea));
                    image.Save(dialog.FileName);
                    Log.AddSuccess("Saved image " + dialog.FileName);
                    
                }
                catch (Exception ex)
                {
                    Log.AddError($"Failed to save image. {ex.Message}" );
                    EnableLog();
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

        private void BtnLoadPreset_Click(object sender, RoutedEventArgs e)
        {
            // If nothing has been changed
            if (layers.Count == 1 && layers[0].Image == null && layers[0].Name == "Layer 1")
            {
                string file = SelectPresetToLoad();
                if (file == "") return;
                LoadLayerPositions(file);
                Title = "RapidQA " + file;
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Save existing layers?",
                "Save Layers?",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question,
                MessageBoxResult.No);

            if (result == MessageBoxResult.Yes)
            {
                BtnSavePreset_Click(null, null);
                string file = SelectPresetToLoad();
                if (file == "") return;
                LoadLayerPositions(file);
                Title = "RapidQA " + file;
            }

            if (result == MessageBoxResult.No)
            {
                string file = SelectPresetToLoad();               
                if (file == "") return;
                LoadLayerPositions(file);
                Title = "RapidQA " + file;
            }

            Undo.Clear();
            Redo.Clear();
        }

        private string SelectPresetToLoad()
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "XML files (*.xml)|*.xml",
            };

            if (fileDirectory == null) dialog.InitialDirectory = "c:\\";
            if (fileDirectory != null) dialog.InitialDirectory = fileDirectory;

            dialog.ShowDialog(this);
            return dialog.FileName;
        }

        private void BtnSavePreset_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();

            if (fileDirectory == null) dialog.InitialDirectory = "c:\\";
            if (fileDirectory != null) dialog.InitialDirectory = fileDirectory;

            dialog.Title = "Save presets";

            dialog.AddExtension = true;

            dialog.RestoreDirectory = true;

            dialog.Filter = "XML files (*.xml)|*.xml";

            if (dialog.ShowDialog().Value)
            {
                try
                {
                    SavePresets(dialog.FileName);
                    Log.AddSuccess($"Saved preset  {dialog.FileName}");
                }
                catch (Exception ex)
                {
                    Log.AddError($"Failed to save presets. {ex.Message}");
                    EnableLog();
                }
            }
        }

        public void LoadLayerPositions(string fileName)
        {
            using (var stream = System.IO.File.OpenRead(fileName))
            {
                var serializer = new XmlSerializer(typeof(List<Layer>));

                try
                {
                    var newlayers = serializer.Deserialize(stream) as List<Layer>;
                    Log.AddInfo("Loaded " + fileName);

                    foreach (Layer l in layers)
                    {
                        DeleteLayer(l);
                    }

                    layers.Clear();
                    EnableWorkareaScaling();

                    foreach (Layer l in newlayers)
                    {
                        layers.Add(l);
                        AddLayer(l);
                        row.AddRowComponents(l, l.Assets);
                        ComboBox cb = l.Row.ComboBox;
                        cb.SelectionChanged += delegate (object s, SelectionChangedEventArgs ev) { Cbx_SelectionChanged(s, ev, l); };
                        AddImage(l);

                        l.Border.RenderTransform = new TranslateTransform(l.CurrentTT.X, l.CurrentTT.Y);
                    }

                    MakeLayerSelected(newlayers.First());
                }
                catch (Exception ex)
                {
                    Log.AddError($"Trouble loading preset. {ex.Message}");
                    EnableLog();
                }
            }  
        }

        public void SavePresets(string FileName)
        {
            using (var writer = new System.IO.StreamWriter(FileName))
            {
                var serializer = new XmlSerializer(layers.GetType());
                serializer.Serialize(writer, layers);
                writer.Flush();
            }
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

        #region LAYER MOVING
        private void LayerImage_MouseLeave(object sender, MouseEventArgs e)
        {
            if (selectedLayer.Image == null) return;
            selectedLayer.CurrentTT = selectedLayer.Border.RenderTransform as TranslateTransform;
            selectedLayer.IsMoving = false;
        }

        private void Image_MouseLeave(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Arrow;
            selectedLayer.Border.BorderThickness = new Thickness(0);
        }

        private void LayerImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            scrollViewer.Focus(); 
            if (selectedLayer.Image == null) return;
            foreach (var l in layers)
            {
                if (l.Image != null) l.Image.IsHitTestVisible = false;
            }
      
            if (selectedLayer.ImagePosition == null)
                selectedLayer.ImagePosition = (selectedLayer.Border.TransformToAncestor(Workarea).Transform(new Point(0, 0)));
            var mousePosition = Mouse.GetPosition(Workarea);
            selectedLayer.DeltaX = mousePosition.X - selectedLayer.ImagePosition.X;
            selectedLayer.DeltaY = mousePosition.Y - selectedLayer.ImagePosition.Y;
            selectedLayer.IsMoving = true;

            selectedLayer.Image.IsHitTestVisible = true;       
        }

        private void PreviewLayerImage_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (selectedLayer.Image == null) return;
            selectedLayer.CurrentTT = selectedLayer.Border.RenderTransform as TranslateTransform;
            selectedLayer.IsMoving = false;

            SaveImagePosition();
        }

        private void SaveImagePosition()
        {
            Layer l = selectedLayer;
            // Gets image position relative to parent
            Point relativeLocation = selectedLayer.Border.TranslatePoint(new Point(0, 0), Workarea);

            var p = relativeLocation;
            if (Undo.ContainsKey(l))
            {
                if (Undo[l].Count < 1 || Undo[l].Last().Equals(p)) return;
                Undo[l].Add(p);
            }
            else
            {
                Undo.Add(l, new List<Point>() { new Point(0,0) });
                Undo[l].Add(p);
            }
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Hand;
            selectedLayer.Border.BorderThickness = new Thickness(1); 
        }

        private void PreviewLayerImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (selectedLayer.Image == null) return;
            if (!selectedLayer.IsMoving) return;
            if (selectedLayer.IsLocked) return;
               
            var mousePoint = Mouse.GetPosition(Workarea);
            
            var offsetX = (selectedLayer.CurrentTT == null ? selectedLayer.ImagePosition.X : selectedLayer.ImagePosition.X - selectedLayer.CurrentTT.X) + selectedLayer.DeltaX - mousePoint.X;
            var offsetY = (selectedLayer.CurrentTT == null ? selectedLayer.ImagePosition.Y : selectedLayer.ImagePosition.Y - selectedLayer.CurrentTT.Y) + selectedLayer.DeltaY - mousePoint.Y;

            if (selectedLayer.CurrentTT == null)
            {
                selectedLayer.CurrentTT = new TranslateTransform(0, 0);
            }

            if (moveXAxis)
            {
                selectedLayer.Border.RenderTransform = new TranslateTransform(-offsetX, selectedLayer.CurrentTT.Y);
            }
            if (moveYAxis)
            {
                selectedLayer.Border.RenderTransform = new TranslateTransform(selectedLayer.CurrentTT.X, -offsetY);
            }
            if (snapping)
            {
                var x = offsetX % 50;
                var Xsnap = offsetX - x;

                var y = offsetY % 50;
                var Ysnap = offsetY - y;

                selectedLayer.Border.RenderTransform = new TranslateTransform(-Xsnap, -Ysnap);
            }

            if (!moveXAxis && !moveYAxis && !snapping) selectedLayer.Border.RenderTransform = new TranslateTransform(-offsetX, -offsetY);                       

        }
        #endregion

        #region LAYER STACKING
        private void MakeLayerSelected(Layer layer)
        {
            selectedLayer = layer;
            if (selectedLayer.CurrentTT == null)
            {
                selectedLayer.CurrentTT = new TranslateTransform(0,0);
            }

            foreach (Layer l in layers)
            {
                if (l.Image != null)
                {
                    l.Image.IsHitTestVisible = false;
                    l.Image.MouseLeave -= Image_MouseLeave;
                    Zoom.MouseLeave -= LayerImage_MouseLeave;      
                    Zoom.PreviewMouseUp -= PreviewLayerImage_MouseUp;
                    l.Image.MouseDown -= LayerImage_MouseDown;
                    l.Image.MouseMove -= Image_MouseMove;
                    Zoom.PreviewMouseMove -= PreviewLayerImage_MouseMove;

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
                        //selectedLayer.Image.MouseLeave += LayerImage_MouseLeave;
                        selectedLayer.Image.MouseLeave += Image_MouseLeave;
                        Zoom.MouseLeave += LayerImage_MouseLeave;
                        //selectedLayer.Image.MouseUp += LayerImage_MouseUp;
                        Zoom.MouseUp += PreviewLayerImage_MouseUp;
                        selectedLayer.Image.MouseDown += LayerImage_MouseDown;
                        //Zoom.MouseDown += LayerImage_MouseDown;                 
                        selectedLayer.Image.MouseMove += Image_MouseMove;
                        Zoom.MouseMove += PreviewLayerImage_MouseMove;
                    }
                }
                else
                {
                    l.Row.Grid.Background = new SolidColorBrush(Color.FromArgb(100, 221, 221, 221));
                }
            }
        }

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

        private void Sp_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is Grid)
            {
                foreach (var layer in layers)
                {
                    if (layer.Row.Grid.Equals((Grid)e.Source))
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
                        Workarea.Children.Remove(selectedLayer.Border);
                        Workarea.Children.Insert(droptargetIndex, selectedLayer.Border);
                    }
                }

                selectedLayer.Row.IsDown = false;
                selectedLayer.Row.IsDragging = false;
                selectedLayer.Row.Grid.ReleaseMouseCapture();
            }
        }
        #endregion

        private void BtnResetAll_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
             "Reset all layer positions?",
             "Reset all?",
             MessageBoxButton.YesNo,
             MessageBoxImage.Question,
             MessageBoxResult.No);

            foreach (Layer l in layers)
            {
                l.Border.RenderTransform = new TranslateTransform(0, 0);
                l.CurrentTT = new TranslateTransform(0, 0);
                SaveImagePosition();            
            } 
        }
    }
}
