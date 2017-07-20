using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RjisFilter
{
    public class Model
    {
        public Settings Settings { get; private set; }
        public RJIS Rjis { get; private set; }
        public Idms Idms { get; private set; }
        public Timetable Timetable { get; private set; }
        public RouteingGuide RouteingGuide { get; set; }

        public Model(Settings settings, RJIS rjis, Idms idms, Timetable timetable, RouteingGuide routeingGuide)
        {
            Settings = settings;
            Rjis = rjis;
            Idms = idms;
            Timetable = timetable;
            RouteingGuide = routeingGuide;
        }

        public void Generate(string toc)
        {
            Rjis.GenerateOutputFiles(toc);
        }
    }
}
