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
        public class Station
        {
            public string Nlc { get; set; }
            public string Crs { get; set; }
            public string Name { get; set; }
        }
        private Settings settings;
        private Idms idms;
        public ObservableCollection<string> Tocs { get; set; }
        public ObservableCollection<Station> TocStations { get; set; }

        public ObservableCollection<Station> AllStations { get; set; }

        public RelayCommand<string> ShowTocCommand { get; set; }

        public string CurrentToc { get; set; }

        public ViewModel(Settings settings, Idms idms)
        {
            this.settings = settings;
            this.idms = idms;
            CurrentToc = settings.PerTocNlcList.First().Key;
            Tocs = new ObservableCollection<string>(settings.PerTocNlcList.Keys);
            ShowTocCommand = new RelayCommand<string>((toc) => {
                CurrentToc = toc;
                TocStations = new ObservableCollection<Station>(settings.PerTocNlcList[toc].Select(x=>new Station { Nlc = x, Crs = idms.GetCrsFromNlc(x), Name = idms.GetNameFromNlc(x) }));
                AllStations = new ObservableCollection<Station>(idms.GetAllStations().Select(x => new Station { Nlc = x, Crs = idms.GetCrsFromNlc(x),  Name = idms.GetNameFromNlc(x) }));
            });
        }

     //   ViewModel
    }
}
