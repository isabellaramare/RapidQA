using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RapidQA
{
    internal class Layer
    {
        public bool IsLocked { get; set; }
        public bool IsSelected { get; set; }
        
        // Layer moving
        public bool IsMoving { get; set; }
        public Point ImagePosition { get; set; }
        public double DeltaX { get; set; }
        public double DeltaY { get; set; }
        public TranslateTransform CurrentTT { get; set; }
        
        public Row Row { get; set; }
        public Image Image { get; set; }
        public Asset Asset { get; set; }

        public Layer()
        {
            Row = new Row();           
        }
    }
}
