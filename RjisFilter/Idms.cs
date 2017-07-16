﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        private Dictionary<string, string> crsToNlc;
        private Dictionary<string, string> tiplocToCrs;

        private string idmsFolder = null;

        public List<string> Warnings { get; private set; }

        public bool Ready { get; set; } = false;

        public Idms(Settings settings)
        {
            this.settings = settings;
            var (ok, idmsFolder) = settings.GetFolder("idms");
            System.Diagnostics.Debug.WriteLine($"IDMS folder 0 {idmsFolder}");

            if (ok)
            {
                this.idmsFolder = idmsFolder;
                var tasklist = new List<Action> { ProcessFareLocations, ProcessStations };
                var tlist = tasklist.Select(t => Task.Run(t));

                Task.WhenAll(tlist).ContinueWith((task) =>
                {
                    crsToNlc = (from entry in nlcToStationName
                                from crs in entry.Value.Crs
                                select new
                                {
                                    crs,
                                    key = entry.Key
                                }).ToDictionary(x => x.crs, x => x.key);
                    Ready = true;

                });

            }
        }

        private void ProcessFareLocations()
        {

            System.Diagnostics.Debug.WriteLine($"IDMS folder {idmsFolder}");
            var locFilename = Path.Combine(idmsFolder, IdmsFareLocationsName);
            if (!File.Exists(locFilename))
            {
                throw new Exception($"IDMS fare location file not found: {locFilename}");
            }
            var fareDoc = XDocument.Load(locFilename, LoadOptions.SetLineInfo);
            var ns = fareDoc.Root.GetDefaultNamespace();
            nlcToFarelocName = (from location in fareDoc.Root.Elements(ns + "FareLocation")
                                where string.Equals(location.Element(ns + "UnattendedTIS").Value, "true", StringComparison.OrdinalIgnoreCase)
                                select new
                                {
                                    Nlc = location.Element(ns + "Nlc").Value,
                                    Name = location.Element(ns + "OJPDisplayName").Value,
                                }).ToDictionary(x => x.Nlc, x => x.Name);
        }

        private void ProcessStations()
        {
            var stationFilename = Path.Combine(idmsFolder, IdmsStationsFileName);
            if (!File.Exists(stationFilename))
            {
                throw new Exception($"IDMS station file not found: {stationFilename}");
            }

            var stationDoc = XDocument.Load(stationFilename, LoadOptions.SetLineInfo);
            var ns = stationDoc.Root.GetDefaultNamespace();

            var validStationElements = stationDoc.Root.Elements(ns + "Station")
                                .Where(x => string.Equals(x.Element(ns + "UnattendedTIS").Value, "true", StringComparison.OrdinalIgnoreCase))
                                .Where(x => !x.Element(ns + "CRS")?.IsEmpty ?? false)
                                .Where(x => !x.Element(ns + "Nlc")?.IsEmpty ?? false)
                                .Where(x => Regex.Match(x.Element(ns + "Nlc")?.Value, "^[0-9A-Z][0-9A-Z][0-9][0-9]$").Success);
               ;

            // an NLC can have several CRS codes - we produce a dictionary mapping an NLC to an "inner" dictionary.
            // the inner dictionary maps CRS to Tiplocs.
            var nlcToCRSToTiploc = validStationElements.GroupBy(x=>x.Element("Nlc").Value)
                .ToDictionary(x=>x.Key, x=>x.GroupBy(y=>y.Element("CRS").Value)
                .ToDictionary(y=>y.Key, y=>y.Select(z=>z.Element("Tiploc")?.Value).ToList()));

            // a dictionary mapping tiploc codes to CRS codes:
            var tiplocToCRS2 = validStationElements.Where(x=>(x.Element("Tiploc")?.IsEmpty ?? true) == false)
                .GroupBy(x=>x.Element("Tiploc").Value).ToDictionary(x=>x.Key, x=>x.Select(y=>y.Element("CRS").Value).First());

            // a dictionary mapping CRS to a list of Tiplocs - we must aggregate lists from all occurences of CRS codes
            var crsToTipLocLookup = nlcToCRSToTiploc.SelectMany(x => x.Value, (element, res) => new { K = res.Key, V = res.Value }).ToLookup(x=>x.K, x=>x.V);
            var crsToTipLoc = crsToTipLocLookup.ToDictionary(x => x.Key, x => x.Aggregate(Enumerable.Empty<string>(), (acc, list) => acc.Concat(list)).ToList());
            

            Console.WriteLine();

            var multiCRS = nlcToStationName.Where(x => x.Value.Crs.Count() > 1).ToList();
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
