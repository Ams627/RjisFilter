using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RjisFilter
{
    class OneStationDialogHelper
    {
        public ICommand OpenOneStationDialog { get; private set; }
        public ICommand OpenOneTocDialog { get; private set; }

        public static OneStationDialogHelper Instance { get; private set; } = null;

        public OneStationDialogHelper GetInstance()
        {
            if ()
        }

        private OneStationDialogHelper()
        {
            System.Diagnostics.Debug.WriteLine("Constructed");
            OpenOneStationDialog = new RelayCommand<object>(OpenOneStation);
            OpenOneTocDialog = new RelayCommand<object>(OpenOneToc);
        }

        private void OpenOneStation(object o)
        {
            System.Diagnostics.Debug.WriteLine("One");
        }
        private void OpenOneToc(object selectedItems)
        {
            var collection = (System.Collections.IList)selectedItems;
            var tocList = collection.Cast<string>().ToList();
            System.Diagnostics.Debug.WriteLine($"jello");
        }

    }
}
