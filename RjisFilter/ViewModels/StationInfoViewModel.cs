using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RjisFilter.Model;

namespace RjisFilter
{
    class StationInfoViewModel
    {
        private Settings settings;
        private RJIS rjis;
        private Idms idms;

        public StationInfoViewModel(Settings settings, RJIS rjis, Idms idms)
        {
            this.settings = settings;
            this.rjis = rjis;
            this.idms = idms;
        }
    }
}
