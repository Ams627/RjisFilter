using System;
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

namespace RjisFilter.Windows
{
    /// <summary>
    /// Interaction logic for Generating.xaml
    /// </summary>
    public partial class Generating : Window
    {
        public Generating()
        {
            InitializeComponent();
            this.PreviewKeyDown += ((s, e) => { if ((Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)) && Keyboard.IsKeyDown(Key.F4))
                    e.Handled = true;
            });

            this.MouseLeftButtonDown += (s, e) => DragMove();

        }
    }
}
