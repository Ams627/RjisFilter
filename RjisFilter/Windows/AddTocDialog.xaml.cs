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
    /// Interaction logic for AddTocDialog.xaml
    /// </summary>
    public partial class AddTocDialog : Window
    {
        public AddTocDialog()
        {
            InitializeComponent();
            Loaded += (sender, e) => MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            KeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    System.Diagnostics.Debug.WriteLine("Esc");
                    if (text.Text.Length == 0)
                    {
                        Close();
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
