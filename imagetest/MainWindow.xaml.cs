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
using System.Xml.Serialization;
using System.Windows.Documents;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Ink;

namespace imagetest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
       // ImageEditor editor = new ImageEditor();
        public MainWindow()
        {
            InitializeComponent();
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, execSave));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, execOpen));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Undo, execUndo));
        }

        private void execOpen(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            if (dlg.ShowDialog() ?? false)
            {
                editor.Open(new System.IO.FileInfo(dlg.FileName));
            }
            e.Handled = true;
        }

        //private void hasChanged(object sender, CanExecuteRoutedEventArgs e)
        //{
        //    e.CanExecute = editor.Strokes.Count > 0;
        //    e.Handled = true;
        //}

        private void execUndo(object sender, ExecutedRoutedEventArgs e)
        {
            editor.Strokes.Remove(editor.Strokes.Last());
            e.Handled = true;
        }

        private void execSave(object sender, ExecutedRoutedEventArgs e)
        {
            editor.Save();
            e.Handled = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            editor.Dispose();
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            editor.Next();
        }
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            editor.Back();
        }
    }
}
