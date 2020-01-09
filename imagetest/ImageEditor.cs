﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace imagetest
{
    public class ImageEditor : IDisposable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private List<FileInfo> images = new List<FileInfo>();

        private FileInfo _ImageFile;
        public static readonly PropertyChangedEventArgs FilenameProperty = new PropertyChangedEventArgs(nameof(ImageFile));

        public FileInfo ImageFile
        {
            get { return _ImageFile; }
            set
            {
                _ImageFile = value;
                Strokes.Clear();
                PropertyChanged?.Invoke(this, ImageFrameProperty);
            }
        }

        private int selectedIndex;
        public static readonly PropertyChangedEventArgs SelectedIndexProperty = new PropertyChangedEventArgs(nameof(SelectedIndex));
        public int SelectedIndex
        {
            get { return selectedIndex; }
            private set
            {
                if (value < images.Count && value > -1)
                {
                    selectedIndex = value;
                    PropertyChanged?.Invoke(this, SelectedIndexProperty);
                    ImageFile = images[selectedIndex];
                    Load();
                }
            }
        }

        MemoryStream mem;
        private BitmapSource _ImageFrame;
        public static readonly PropertyChangedEventArgs ImageFrameProperty = new PropertyChangedEventArgs(nameof(ImageFrame));
        public BitmapSource ImageFrame
        {
            get { return _ImageFrame; }
            set
            {
                _ImageFrame = value;
                PropertyChanged?.Invoke(this, ImageFrameProperty);
            }
        }

        public StrokeCollection Strokes { get; } = new StrokeCollection();

        public void Open(FileInfo file)
        {
            images.Add(file);
            SelectedIndex = images.Count - 1;
        }
        public void Next()
        {
            SelectedIndex++;
        }

        public void Back()
        {
            SelectedIndex--;
        }

        public void Load()
        {
            ImageFile.Refresh();
            if (ImageFile.Exists)
            {
                if (mem != null)
                {
                    mem.Dispose();
                }
                using (var stream = ImageFile.OpenRead())
                {
                    mem = new MemoryStream();
                    stream.CopyTo(mem);
                }
                ImageFrame = BitmapFrame.Create(mem);
            }
        }
        public void Dispose()
        {
            if (mem != null)
            {
                mem.Dispose();
            }
            ImageFrame = null;
        }

        public void Save()
        {
            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawImage(ImageFrame, new Rect(0, 0, ImageFrame.Width, ImageFrame.Height));
                foreach (var item in Strokes)
                {
                    item.Draw(drawingContext);
                }
                drawingContext.Close();
                Strokes.Clear();
                var width = ImageFrame.Width;
                var height = ImageFrame.Height;
                var bitmap = new RenderTargetBitmap((int)width, (int)height, 96, 96, PixelFormats.Pbgra32);
                bitmap.Render(drawingVisual);

                ImageFrame = bitmap;

                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(ImageFrame));

                using (var stream = ImageFile.OpenWrite())
                {
                    encoder.Save(stream);
                }
            }
        }
    }
}
