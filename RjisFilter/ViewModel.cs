using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvvmFoundation.Wpf;


namespace RjisFilter
{
    class ViewModel
    {
        private Settings settings;
        public ObservableCollection<string> Tocs { get; set; }
        public ObservableCollection<string> TocNLCs { get; set; }

        public RelayCommand ShowTocCommand { get; set; }

        public ViewModel(Settings settings)
        {
            this.settings = settings;
            Tocs = new ObservableCollection<string>(settings.PerTocNlcList.Keys);
            ShowTocCommand = new RelayCommand(() => { System.Diagnostics.Debug.WriteLine("Hello"); });
        }

     //   ViewModel
    }
}
