﻿using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace RjisFilter
{
    /// <summary>
    /// Interaction logic for TocEditor.xaml
    /// </summary>
    public partial class TocEditor : Window
    {
        Point? _startPoint = null;
        public bool IsDragging { get; set; }
        public TocEditor()
        {
            InitializeComponent();

            allStationsGrid.PreviewMouseDown += (s, e) =>
            {
                if (e.LeftButton == MouseButtonState.Pressed && !IsDragging)
                {
                    var dg = s as DataGrid;

                    System.Diagnostics.Debug.WriteLine($"datagrid {(dg?.ToString() ?? "null")}");
                    Point mousePos = e.GetPosition(null);
                    System.Diagnostics.Debug.WriteLine($"POS: x={mousePos.X}, y={mousePos.Y}");

                    Vector diff = _startPoint.Value - mousePos;
                    // test for the minimum displacement to begin the drag
                    if (e.LeftButton == MouseButtonState.Pressed &&
                        (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                        Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
                    {
                        StartDrag(e);
                    }
                }
            };

            allStationsGrid.PreviewMouseLeftButtonDown += (s, e) =>
            {
                _startPoint = e.GetPosition(null);

                System.Diagnostics.Debug.WriteLine($"down: x={_startPoint.Value.X}, y={_startPoint.Value.Y}");
            };
        }
        void StartDrag(MouseButtonEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Drag");
        }

    }
}
