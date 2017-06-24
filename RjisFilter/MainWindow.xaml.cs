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

namespace RjisFilter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Settings settings;
        public MainWindow(Settings settings)
        {
            this.settings = settings;
            var viewmodel = new ViewModel(settings);
            this.DataContext = viewmodel;
            InitializeComponent();
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                var window = new TocEditor();
                window.DataContext = this.DataContext;
                var vm = DataContext as ViewModel;
                if (vm.ShowTocCommand != null && vm.ShowTocCommand.CanExecute(null))
                {
                    vm.ShowTocCommand.Execute(null);
                }
                window.ShowDialog();
            }
        }
    }
}
