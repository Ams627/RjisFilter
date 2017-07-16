using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RjisFilter
{
    class OneStationViewModel
    {
        public List<string> Tiplocs { get; set; }
        public List<string> Clusters { get; set; }
        public List<string> Groups { get; set; }
        public string CRS { get; set; }
    }
}
