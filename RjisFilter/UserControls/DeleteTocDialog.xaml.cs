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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RjisFilter.UserControls
{
    /// <summary>
    /// Interaction logic for DeleteTocDialog.xaml
    /// </summary>
    public partial class DeleteTocDialog : UserControl
    {
        public DeleteTocDialog()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                var element1 = Keyboard.FocusedElement;
                MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                var element2 = Keyboard.FocusedElement;
                var window = Window.GetWindow(this);
                window.Width = this.ActualWidth;
                window.Height = this.ActualHeight;
            };
            KeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    System.Diagnostics.Debug.WriteLine("Esc");
                    Window w = Window.GetWindow(this);
                    e.Handled = true;
                    w.Close();
                }
                else
                {
                    e.Handled = false;
                }
            };

        }
    }
}
