using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace RjisFilter
{
    class NoTocsAdorner : Adorner
    {
        public NoTocsAdorner(UIElement adornedElement) : base (adornedElement)
        {
        }
        protected override void OnRender(DrawingContext drawingContext)
        {
            var adornedElementRect = new Rect(this.AdornedElement.DesiredSize);
            var text = "No TOCs defined. Double-click to add.";
            var textBlock = new TextBlock();
            textBlock.Text = text;

            if (AdornedElement is Control control)
            {
                var font = control.FontFamily;
                var flowdir = control.FlowDirection;
                var emsize = control.FontSize;

                var typeface = new Typeface(control.FontFamily, control.FontStyle, control.FontWeight, control.FontStretch);
                var ft = new FormattedText(text, CultureInfo.CurrentUICulture, flowdir, typeface, control.FontSize, Brushes.BlueViolet);
                var startPoint = new Point(control.ActualWidth / 2 - ft.Width / 2, control.ActualHeight / 2 - ft.Height / 2);
//                startPoint.Offset(adornedElementRect.Left, adornedElementRect.Top);
                drawingContext.DrawText(ft, startPoint);
            }
        }

    }
}
