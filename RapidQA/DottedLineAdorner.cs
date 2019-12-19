using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace RapidQA
{
    class DottedLineAdorner : Adorner
    {
        public UIElement AdornedElement { get; set; }

        public DottedLineAdorner(UIElement adornedElement) : base(adornedElement)
        {
            AdornedElement = adornedElement;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            Size eltSize = (AdornedElement as FrameworkElement).DesiredSize;
            Pen pen = new Pen(Brushes.Gray, 3.2) { DashStyle = DashStyles.Dot };
            Rect rect = new Rect(new Size(0,16));
            drawingContext.DrawRectangle(null, pen, rect);
        }
    }
}
