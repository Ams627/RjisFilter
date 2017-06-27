using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvvmFoundation.Wpf;
using System.Windows;

namespace RjisFilter
{
    class ViewModel : DependencyObject
    {
        public class Station
        {
            public string Nlc { get; set; }
            public string Crs { get; set; }
            public string Name { get; set; }
        }
        private Settings settings;
        private Idms idms;
        private RJIS rjis;
        public ObservableCollection<string> Tocs { get; set; }
        public ObservableCollection<Station> TocStations { get; set; }

        public ObservableCollection<Station> AllStations { get; set; }

        public RelayCommand<string> ShowTocCommand { get; set; }

        public string CurrentToc { get; set; }

        public ViewModel(Settings settings, Idms idms, RJIS rjis)
        {
            this.settings = settings;
            this.idms = idms;
            this.rjis = rjis;
            CurrentToc = settings.PerTocNlcList.First().Key;
            Tocs = new ObservableCollection<string>(settings.PerTocNlcList.Keys);
            ShowTocCommand = new RelayCommand<string>((toc) => {
                CurrentToc = toc;
                TocStations = new ObservableCollection<Station>(settings.PerTocNlcList[toc].Select(x=>new Station { Nlc = x, Crs = idms.GetCrsFromNlc(x), Name = idms.GetNameFromNlc(x) }));
                AllStations = new ObservableCollection<Station>(idms.GetAllStations().Select(x => new Station { Nlc = x, Crs = idms.GetCrsFromNlc(x),  Name = idms.GetNameFromNlc(x) }));
            });

            rjis.PropertyChanged += Rjis_PropertyChanged;
            
        }



        public int LinesRead
        {
            get { return (int)GetValue(LinesReadProperty); }
            set { SetValue(LinesReadProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LinesRead.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LinesReadProperty =
            DependencyProperty.Register("LinesRead", typeof(int), typeof(ViewModel), new PropertyMetadata(0));



        private void Rjis_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Changed");
            if (e.PropertyName == "LinesRead")
            {
                LinesRead = rjis.LinesRead;
            }
        }

        //   ViewModel
    }
}
