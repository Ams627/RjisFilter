using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RjisFilter
{
    public class Idms
    {
        class StationInfo
        {
            public List<string> Name { get; set; }
            public List<string> Crs { get; set; }
            public List<string> Tiploc { get; set; }
        }

        private static string IdmsFareLocationsName = "FareLocationsRefData.xml";
        private static string IdmsStationsFileName = "StationsRefData.xml";
        private Settings settings;

        private Dictionary<string, string> nlcToFarelocName;
        private Dictionary<string, StationInfo> nlcToStationName;
        //private Dictionary<string, string> crsToNlc;

        private string idmsFolder = null;


        public List<string> Warnings { get; private set; }

        public bool IdmsAvailable { get; set; } = false;

        public static async Task<Idms> CreateAsync(Settings settings)
        {
            var instance = new Idms(settings);
            return instance.StartAsync();

        }

        private async Task<Idms> StartAsync()
        {
            await Task.Run(() => Console.WriteLine("Hello"));
            //var (ok, idmsFolder) = settings.GetFolder("idms");
            //if (ok)
            //{
            //    await Task.WhenAll(new List<Task> { ProcessFareLocations(), ProcessStations() });
            //    crsToNlc = (from entry in nlcToStationName
            //                from crs in entry.Value.Crs
            //                select new
            //                {
            //                    crs,
            //                    key = entry.Key
            //                }).ToDictionary(x => x.crs, x => x.key);

            //}
            return this;
        }

        private Idms(Settings settings)
        {
            this.settings = settings;
        }

        private async Task ProcessFareLocations()
        {
            var locFilename = Path.Combine(idmsFolder, IdmsFareLocationsName);
            if (!File.Exists(locFilename))
            {
                throw new Exception($"IDMS fare location file not found: {locFilename}");
            }
            await Task.Run(() =>
            {
                var fareDoc = XDocument.Load(locFilename, LoadOptions.SetLineInfo);
                var ns = fareDoc.Root.GetDefaultNamespace();
                nlcToFarelocName = (from location in fareDoc.Root.Elements(ns + "FareLocation")
                                    where string.Equals(location.Element(ns + "UnattendedTIS").Value, "true", StringComparison.OrdinalIgnoreCase)
                                    select new
                                    {
                                        Nlc = location.Element(ns + "Nlc").Value,
                                        Name = location.Element(ns + "OJPDisplayName").Value,
                                    }).ToDictionary(x => x.Nlc, x => x.Name);
            });
        }

        private async Task ProcessStations()
        {
            var stationFilename = Path.Combine(idmsFolder, IdmsStationsFileName);
            if (!File.Exists(stationFilename))
            {
                throw new Exception($"IDMS station file not found: {stationFilename}");
            }

            await Task.Run(() =>
            {


                var stationDoc = XDocument.Load(stationFilename, LoadOptions.SetLineInfo);
                var ns = stationDoc.Root.GetDefaultNamespace();

                nlcToStationName = (from station in stationDoc.Root.Elements(ns + "Station")
                                    where string.Equals(station.Element(ns + "UnattendedTIS").Value, "true", StringComparison.OrdinalIgnoreCase)
                                    where !station.Element(ns + "CRS").IsEmpty
                                    where !station.Element(ns + "Nlc").IsEmpty
                                    where !string.IsNullOrWhiteSpace(station.Element(ns + "Nlc").Value)
                                    group station by station.Element("Nlc").Value into g
                                    select new
                                    {
                                        Nlc = g.Key,
                                        SInfo = new StationInfo
                                        {
                                            Name = (from member in g where member.Element("OJPEnabled").Value == "true" select member.Element("Name").Value).GroupBy(x => x).Select(x => x.First()).ToList(),
                                            Crs = (from member in g where member.Element("OJPEnabled").Value == "true" select member.Element("CRS").Value).GroupBy(x => x).Select(x => x.First()).ToList(),
                                            Tiploc = (from tip in g.Elements("Tiploc") select tip.Value).ToList().GroupBy(x => x).Select(x => x.First()).ToList()
                                        }
                                    }).Where(x => x.SInfo.Crs.Count() > 0).OrderBy(x => x.Nlc).ToDictionary(x => x.Nlc, x => x.SInfo);
            });

        }


        public string GetNameFromNlc(string nlc)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(nlc) && nlc.Length == 4);
            var tryResult = nlcToStationName.TryGetValue(nlc, out var station);
            var result = tryResult ? station.Name.First() : "STATION NAME NOT FOUND";
            return result;
        }

        public string GetCrsFromNlc(string nlc)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(nlc) && nlc.Length == 4);
            var tryResult = nlcToStationName.TryGetValue(nlc, out var station);
            var result = tryResult ? string.Join(", ", station.Crs) : "STATION CRS NOT FOUND";
            return result;
        }

        public List<string> GetAllStations()
        {
            return new List<string>(nlcToStationName.Keys);
        }

    }
}
