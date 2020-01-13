using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Serialization;

namespace RapidQA
{    
    public class Layer
    {
        public bool IsLocked { get; set; }
        public string Name { get; set; }
        
        // Layer moving
        public bool IsMoving { get; set; }
        public Point ImagePosition { get; set; }
        public double DeltaX { get; set; }
        public double DeltaY { get; set; }
        public TranslateTransform CurrentTT { get; set; }
        
        [XmlIgnore]
        public Row Row { get; set; }
        [XmlIgnore]
        public Image Image { get; set; }
        [XmlIgnore]
        public Border Border { get; set; }

        public ObservableCollection<Asset> Assets { get; set; } = new ObservableCollection<Asset>();
        public int SelectedIndex { get; set; }
 
        public Layer()
        {
            Row = new Row();
            CurrentTT = new TranslateTransform(0, 0);
        }
    }
}
