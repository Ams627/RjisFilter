using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
using System.Xml;

namespace RjisFilter
{
    public class Settings
    {
        private static string settingsFile;
        public enum SetOptions
        {
            AllStations,
            AllSetsInSettingsFile,
            SpecificSets
        }

        public static string GetSettingsFile()
        {
            return settingsFile;
        }
        /// <summary>
        /// Path for settings.xml - normally %appdata%/Parkeon/RcsConverter
        /// </summary>
        public string SettingsFile { get; private set; }
        public static string ProductName { get; private set; }

        public SetOptions SetOption { get; set; }

        public List<string> SetsToProduce { get; set; }

        /// <summary>
        /// a dictionary of folder types - keys are any of the following: RJIS, RCSFLOW, TEMP, IDMS - each
        /// of these come from settings .xml under the Folders key. Each folder is specified with a Folder element with
        /// attributes Name and Location.
        /// </summary>
        private Dictionary<string, string> folders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public string CurrentTocName { get; set; }
        public HashSet<string> GlobalTicketTypes { get; private set; }
        public Dictionary<string, SortedSet<string>> PerTocNlcList { get; private set; }
        public Dictionary<string, HashSet<string>> PerTocTicketTypeList { get; private set; }
        public List<string> Warnings { get; private set; } = new List<string>();

        static Settings()
        {
            var versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
            var companyName = versionInfo.CompanyName;
            var productName = versionInfo.ProductName;
            var version = versionInfo.ProductVersion;

            // store for later access by other program areas:
            ProductName = productName;

            var appdataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var programFolder = Path.Combine(appdataFolder, companyName, productName);
            if (File.Exists(programFolder))
            {
                throw new Exception($"File {programFolder} exists but this program requires that path as a folder. It must not be an existing file.");
            }
            // create the program folder if it does not exist. We should never need to do this but we will do
            // it as an emergency procedure:
            Directory.CreateDirectory(programFolder);
            settingsFile = Path.Combine(programFolder, "settings.xml");
            if (!File.Exists(settingsFile))
            {
                var defaultStations = new Dictionary<string, List<string>>(){ {"Northern",
                new List<string> {"1988", "2048", "2107", "2160", "2257", "2325",
                    "2326", "2339", "2340", "2341", "2391", "2549", "2552", "2562", "2576", "2584", "2612", "2619",
                    "2660", "2661", "2668", "2671", "2677", "2697", "2710", "2732", "2745", "2768", "2771", "2774",
                    "2790", "2794", "2817", "2830", "2833", "2848", "2861", "2867", "2870", "2895", "2896", "2920",
                    "2924", "2946", "2963", "6574", "6664", "6688", "6690", "7501" }},

                    { "Scotrail", new List<string> { "3776", "6391", "6929", "6963", "8649", "8805",
                    "8860", "8905", "8976", "9039", "9136", "9145", "9160", "9311", "9328", "9329", "9395",
                    "9396", "9419", "9438", "9463", "9490", "9500", "9635", "9667", "9682", "9683", "9691",
                    "9723", "9727", "9739", "9753", "9790", "9796", "9813", "9876", "9888", "9914", "9950",
                    "9981", "9992" } },

                    {"RedFunnel" , new List<string> { "K279", "K280", "5932" } },

                    {"SWT", new List<string> {"1090", "1463", "1466", "1467", "1475", "1478", "1492", "1509",
                     "3047", "3048", "3051", "3052", "3053", "3054", "3055", "3056", "3057", "3059", "3061",
                     "3067", "3104", "3110", "3121", "4502", "4508", "4515", "4525", "4527", "4597", "4600", "8530"
                    }}
                };

                var settingsDoc = new XDocument(
                        new XDeclaration("1.0", "utf-8", "no"),
                        new XElement("Settings", new XAttribute("Version", version),
                            new XElement("StationSets",
                            from stationSet in defaultStations select
                                new XElement("StationSet", new XAttribute("Name", stationSet.Key),
                                from station in stationSet.Value select
                                    new XElement("Station", new XAttribute("Nlc", station))))));
                settingsDoc.Save(settingsFile);

            }
        }

        public Settings()
        {
            // copy static settings file to instance:
            SettingsFile = settingsFile;

            // LoadOptions.SetLineInfo sets the line number info for the settings file which is used for error reporting:
            var doc = XDocument.Load(SettingsFile, LoadOptions.SetLineInfo);

            folders = doc.Descendants("Folders").Elements("Folder").Select(folder => new
            {
                Name = (string)folder.Attribute("Name"),
                Location = (string)folder.Attribute("Location")
            }).ToDictionary(x => x.Name, x => x.Location);

            foreach (var folder in folders)
            {
                if (folder.Value == null)
                {
                    throw new Exception($"The location specified for {folder.Key} must not be empty.");
                }
            }

            GlobalTicketTypes = new HashSet<string>(doc.Descendants("GlobalTicketTypes").Elements("TicketType").Select(ticketType =>
                (string)ticketType.Attribute("Code")));

            // check ticket codes are all 3 characters and only upper case letters or digits:
            foreach (var ticket in GlobalTicketTypes)
            {
                if (string.IsNullOrWhiteSpace(ticket) || ticket.Length != 3 || ticket.Any(c=>!char.IsUpper(c) && !char.IsDigit(c)))
                {
                    throw new Exception($"Invalid ticket type {ticket} specified.");
                }
            }

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

            // Check for stations without Nlc codes:
            var stations = doc.Descendants("Station").Where(x => x.Attribute("Nlc") == null);
            foreach (var invalidStation in stations)
            {
                var li = invalidStation as IXmlLineInfo;
                Warnings.Add($"Warning: <Station> node does not have an NLC code at line {li.LineNumber}");
            }

            // check for ticket types without code:
            var allTicketTypes = doc.Descendants("TicketTypes").Where(x => x.Attribute("Code") == null);
            foreach (var ticket in allTicketTypes)
            {
                var li = ticket as IXmlLineInfo;
                Warnings.Add($"Warning: <TicketType> node does not have a 'Code' attribute at line {li.LineNumber}");
            }

            // Check all NLC codes are valid:
            stations = doc.Descendants("Station").Where(x => x.Attribute("Nlc") != null && !Regex.Match(x.Attribute("Nlc").Value, "^[0-9A-Z][0-9A-Z][0-9A-Z][0-9A-Z]$").Success);
            foreach (var invalidStation in stations)
            {
                var li = invalidStation as IXmlLineInfo;
                Warnings.Add($"Warning: <Station> node does not have a valid NLC code at line {li.LineNumber}");
            }

            var validStationSets = doc.Element("Settings").Elements("StationSets").Elements("StationSet").Where(x => x.Attribute("Name") != null);
            var allUsed = validStationSets.Where(x => x.Attribute("Name").Value.ToLower() == "all");
            if (allUsed.Count() != 0)
            {
                var badname = allUsed.First().Attribute("Name").Value;
                var li = allUsed.First() as IXmlLineInfo;
                throw new Exception($"You cannot use \"{badname}\" as the name of a station set (at line {li.LineNumber} in {SettingsFile}) - it is a reserved word.");
            }
            PerTocNlcList = validStationSets.ToDictionary(x => x.Attribute("Name").Value, x => x.Descendants("Station").Where(e => e.Attribute("Nlc") != null).Select(e => e.Attribute("Nlc").Value).ToSortedSet(StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase);

            PerTocTicketTypeList = validStationSets.ToDictionary(
                x => x.Attribute("Name").Value,
                x => x.Descendants("TicketType").Where(e => e.Attribute("Code") != null).Select(e => e.Attribute("Code").Value).ToHashSet(StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase);
            Console.WriteLine("");
        }

        public (bool, string) GetFolder(string name)
        {
            return (folders.TryGetValue(name, out var result), result);
        }

    }
}
