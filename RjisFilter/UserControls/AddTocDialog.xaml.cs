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
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class AddTocDialog : UserControl
    {
        public AddTocDialog()
        {
            InitializeComponent();
            Loaded += (sender, e) =>
            {
                MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            };
            KeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    System.Diagnostics.Debug.WriteLine("Esc");
                    if (text.Text.Length == 0)
                    {
                        Window w = Window.GetWindow(this);
                        w.Close();
                    }
                    else
                    {
                        text.Text = "";
                    }
                }
            };

        }
    }
}
