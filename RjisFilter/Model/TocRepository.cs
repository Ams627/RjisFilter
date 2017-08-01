using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;

namespace RjisFilter.Model
{
    public class TocRepository
    {
        /// <summary>
        /// milliseconds after which to autosave:
        /// </summary>
        private const int autoSaveOnIdleTime = 2000;
        private Idms idms;
        private string filename;
        private bool autosave;
        private bool saving;
        private bool isDirty;
        private HashSet<string> tocs;
        private HashSet<string> globalTicketRepo; // ticket types must be in this list unless it is empty
        private Dictionary<string, HashSet<string>> ticketRepo;
        private Dictionary<string, HashSet<string>> stationRepo;
        private Dictionary<string, HashSet<string>> routeRepo;
        private List<string> Warnings { get; set; }

        public bool Saving
        {
            get
            {
                return saving;
            }
        }

        public TocRepository(Idms idms, bool autosave)
        {
            this.idms = idms;
            this.autosave = autosave;
            isDirty = false;
            tocs = new HashSet<string>();
            ticketRepo = new Dictionary<string, HashSet<string>>();
            stationRepo = new Dictionary<string, HashSet<string>>();
            routeRepo = new Dictionary<string, HashSet<string>>();
            globalTicketRepo = new HashSet<string>();

            var versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
            var companyName = versionInfo.CompanyName;
            var productName = versionInfo.ProductName;
            var version = versionInfo.ProductVersion;

            var appdataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var programFolder = Path.Combine(appdataFolder, companyName, productName);
            if (File.Exists(programFolder))
            {
                throw new Exception($"File {programFolder} exists but this program requires that path as a folder. It must not be an existing file.");
            }
            // create the program folder if it does not exist. We should never need to do this but we will do
            // it as an emergency procedure:
            Directory.CreateDirectory(programFolder);
            filename = Path.Combine(programFolder, "tocs2.xml");
            Load();

            if (autosave)
            {
                var timer = new DispatcherTimer()
                {
                    Interval = TimeSpan.FromSeconds(1)
                };
                timer.Tick += (s, e) =>
                {
                    var timeSinceLastInput = User32Interop.GetLastInput();
                    if (timeSinceLastInput.TotalMilliseconds > 2000 && isDirty && !saving)
                    {
                        Save();
                    }
                };
                timer.Start();
            }
        }
        public void Save()
        {
            saving = true;

            saving = false;
        }

        void XmlDocumentCheck(XDocument doc)
        {
            // Check all XML element names are in sentence case:
            var nodes = doc.Descendants().Where(x => x.Name.LocalName.Length > 0 && !char.IsUpper(x.Name.LocalName[0])).Distinct();
            foreach (var node in nodes)
            {
                var li = node as IXmlLineInfo;
                Warnings.Add($"Node name {node.Name.LocalName} does not meet schema rules (must start with upper case) at line {li.LineNumber}");
            }

            // Check all XML attribute names are in sentence case:
            var attributes = doc.Descendants().SelectMany(x => x.Attributes()).Where(y => y.Name.LocalName.Length > 0 && !char.IsUpper(y.Name.LocalName[0]));
            foreach (var attribute in attributes)
            {
                var li = attribute as IXmlLineInfo;
                Warnings.Add($"attribute name {attribute.Name.LocalName} does not meet schema rules (must start with upper case) at line {li.LineNumber}");
            }
        }

        void Load()
        {
            if (!File.Exists(filename))
            {
                var tocDoc = new XDocument(
                                    new XDeclaration("1.0", "utf-8", "no"),
                                    new XElement("TocRepository", new XAttribute("Version", "1.0.0.0"),
                                        new XElement("StationSets")));
                tocDoc.Save(filename);
            }
            var doc = XDocument.Load(filename, LoadOptions.SetLineInfo);
            XmlDocumentCheck(doc);

            // Check for stations without Nlc codes - not doc.Descendants never returns null:
            var stations = doc.Descendants("Station").Where(x => x.Attribute("Nlc") == null);
            foreach (var invalidStation in stations)
            {
                var li = invalidStation as IXmlLineInfo;
                Warnings.Add($"Warning: <Station> node does not have an NLC code at line {li.LineNumber}");
            }

            // Check for routes without route codes:
            var routes = doc.Descendants("Route").Where(x => x.Attribute("Code") == null);
            foreach (var invalidStation in stations)
            {
                var li = invalidStation as IXmlLineInfo;
                Warnings.Add($"Warning: <Route> node does not have a route code at line {li.LineNumber} in file {filename}.");
            }

            // check for ticket types without code:
            var allTicketTypes = doc.Descendants("TicketTypes").Where(x => x.Attribute("Code") == null);
            foreach (var ticket in allTicketTypes)
            {
                var li = ticket as IXmlLineInfo;
                Warnings.Add($"Warning: <TicketType> node does not have a 'Code' attribute at line {li.LineNumber} in file {filename}.");
            }

            // Check all NLC codes are valid:
            stations = doc.Descendants("Station").Where(x => x.Attribute("Nlc") != null && !Regex.Match(x.Attribute("Nlc").Value, "^[0-9A-Z][0-9A-Z][0-9][0-9]$").Success);
            foreach (var invalidStation in stations)
            {
                var li = invalidStation as IXmlLineInfo;
                Warnings.Add($"Warning: <Station> node does not have a valid NLC code at line {li.LineNumber} in file {filename}.");
            }

            // check ticket codes are all 3 characters and only upper case letters or digits:
            var allTicketCodes = doc.Descendants("TicketType").Where(x => x.Attribute("Code") != null && !Regex.Match(x.Attribute("Code").Value, "^[0-9A-Z][0-9A-Z][0-9A-Z]$").Success);
            foreach (var element in allTicketCodes)
            {
                var li = element as IXmlLineInfo;
                throw new Exception($"Invalid ticket type {element.Value} specified at line {li.LineNumber} in file {filename}.");
            }

            // check route codes are all 5 digits:
            var routeCodes = doc.Descendants("Route").Where(x => x.Attribute("Code") != null && !Regex.Match(x.Attribute("Code").Value, "^[0-9]{5}$").Success);
            foreach (var element in allTicketCodes)
            {
                var li = element as IXmlLineInfo;
                throw new Exception($"Invalid route code {element.Value} specified at line {li.LineNumber} in file {filename}.");
            }

            var validStationSets = doc.Element("TocRepository").Elements("StationSets").Elements("StationSet").Where(x => x.Attribute("Name") != null);
            stationRepo = (from set in validStationSets
                           select new
                           {
                               SetName = set.Attribute("Name").Value,
                               HashSet = (from station in set.Elements("Station") select station.Attribute("Nlc").Value).ToHashSet()
                           }
                          ).ToDictionary(x => x.SetName, x => x.HashSet);
            ticketRepo = (from set in validStationSets
                          select new
                          {
                              Set = set.Attribute("Name").Value,
                              List = (from station in set.Elements("Ticket") select station.Attribute("Code").Value).ToHashSet()
                          }
                          ).ToDictionary(x => x.Set, x => x.List);
            routeRepo = (from set in validStationSets
                         select new
                         {
                             Set = set.Attribute("Name").Value,
                             List = (from station in set.Elements("Route") select station.Attribute("Code").Value).ToHashSet()
                         }
                          ).ToDictionary(x => x.Set, x => x.List);


            globalTicketRepo = doc.Element("TocRepository").Elements("GlobalTicketTypes").Elements("TicketType").Select(x => x.Attribute("Code").Value).ToHashSet();
            tocs = (from set in validStationSets select set.Attribute("Name")?.Value).ToHashSet();
            Console.WriteLine();
        }

        public void AddToc(string newToc)
        {
            tocs.Add(newToc);
            isDirty = true;
        }

        public void DeleteToc(string existingToc)
        {
            tocs.Remove(existingToc);
            ticketRepo.Remove(existingToc);
            routeRepo.Remove(existingToc);
            stationRepo.Remove(existingToc);
        }

        public void Add(string toc, string code)
        {
            if (!tocs.Contains(toc))
            {
                // should never happen as the GUI does not allow it. It could occur from manual editing of the tocs.xml file:
                throw new Exception("Request to add station to unknown toc.");
            }
            // check for CRS code:
            if (Regex.Match(code, "^[0-9A-Z][0-9A-Z][0-9A-Z]$").Success)
            {
                var nlc = idms.GetCrsFromNlc(code);
                DictUtils.AddEntry(stationRepo, toc, nlc);
            }
            else if (Regex.Match(code, "^[0-9A-Z][0-9A-Z][0-9][0-9]$").Success)
            {
                // it's an NLC:
                DictUtils.AddEntry(stationRepo, toc, code);
            }
            else
            {
                throw new Exception($"Cannot add station code {code} - it is not a valid CRS nor NLC code.");
            }
        }

        public void AddRange(string toc, IEnumerable<string> range)
        {
            if (!tocs.Contains(toc))
            {
                // should never happen as the GUI does not allow it. It could occur from manual editing of the tocs.xml file:
                throw new Exception("Request to add range of stations to unknown toc.");
            }
            foreach (var station in range)
            {
                Add(toc, station);
            }
        }

        public void AddTicket(string toc, string ticketCode)
        {
            if (!tocs.Contains(toc))
            {
                // should never happen as the GUI does not allow it. It could occur from manual editing of the tocs.xml file:
                throw new Exception("Request to add ticket to unknown toc.");
            }
            if (!Regex.Match(ticketCode, "^[0-9A-Z][0-9A-Z][0-9][0-9]$").Success)
            {
                throw new Exception($"Request to add invalid ticket code {ticketCode} to toc {toc}");
            }
            DictUtils.AddEntry(ticketRepo, toc, ticketCode);
        }

        public void AddTicketRange(string toc, IEnumerable<string> range)
        {
            if (!tocs.Contains(toc))
            {
                // should never happen as the GUI does not allow it. It could occur from manual editing of the tocs.xml file:
                throw new Exception("Request to add ticket range to unknown toc.");
            }
            foreach (var ticketCode in range)
            {
                AddTicket(toc, ticketCode);
            }
        }

        public HashSet<string> GetStations(string toc)
        {
            if (!tocs.Contains(toc))
            {
                throw new Exception("request to return stations for unknown toc.");
            }
            stationRepo.TryGetValue(toc, out var result);
            return result;
        }

        public HashSet<string> GetTickets(string toc)
        {
            if (!tocs.Contains(toc))
            {
                throw new Exception("request to return tickets for unknown toc.");
            }
            ticketRepo.TryGetValue(toc, out var result);
            return result;
        }

        public HashSet<string> GetRoutes(string toc)
        {
            if (!tocs.Contains(toc))
            {
                throw new Exception("request to return routes for unknown toc.");
            }
            routeRepo.TryGetValue(toc, out var result);
            return result;
        }

        public HashSet<string> GetTocs()
        {
            return tocs;
        }
    }
}