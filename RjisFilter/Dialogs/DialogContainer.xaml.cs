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
    /// Interaction logic for DialogContainer.xaml
    /// </summary>
    public partial class DialogContainer : Window
    {
        public DialogContainer()
        {
            InitializeComponent();
            this.Loaded += (s,e)=> MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }
    }
}
