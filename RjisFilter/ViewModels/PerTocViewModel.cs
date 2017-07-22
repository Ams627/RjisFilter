using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RjisFilter.Model;

namespace RjisFilter.ViewModels
{
    /// <summary>
    /// This class provides all the information for editing a single TOC. We should be able to edit
    /// a ticket type list and a station list for the TOC. The ticket type list is read from the
    /// settings file. 
    /// </summary>
    class PerTocViewModel : ViewModelBase
    {
        public class Station
        {
            public string Nlc { get; set; }
            public string Crs { get; set; }
            public string Name { get; set; }
        }

        public string Toc { get; set; }
        public ObservableCollection<Station> TocStations { get; set; }
        public ObservableCollection<Station> AllStations { get; set; }

        private readonly MainModel model;
        public PerTocViewModel(MainModel model, object param)
        {
            this.model = model;
            Toc = param as string;
            TocStations = new ObservableCollection<Station>(model.Settings.PerTocNlcList[Toc].Select(x => new Station { Nlc = x, Crs = model.Idms.GetCrsFromNlc(x), Name = model.Idms.GetNameFromNlc(x) }));
            AllStations = new ObservableCollection<Station>(model.Idms.GetAllStations().Select(x => new Station { Nlc = x, Crs = model.Idms.GetCrsFromNlc(x), Name = model.Idms.GetNameFromNlc(x) }));
        }
    }
}
