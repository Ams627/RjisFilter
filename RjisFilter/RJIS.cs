using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;

namespace RjisFilter
{
    partial class RJIS
    {

        private readonly Settings settings;
        private ILookup<string, string> rjisLookup;

        Dictionary<string, List<string>> clusterToStationList;
        Dictionary<string, List<string>> stationToClusterList;

        private static void AddEntry(Dictionary<string, List<string>> d, string key, string listEntry)
        {
            if (!d.TryGetValue(key, out var list))
            {
                list = new List<string>();
                d.Add(key, list);
            }
            list.Add(listEntry);
        }

        public RJIS(Settings settings)
        {
            this.settings = settings;
            var (ok, folder) = settings.GetFolder("RJISZIPS");
            if (ok)
            {
                var rjisNonZips = Directory.GetFiles(folder, "RJFAF*").ToList();

                // remove non-rjis files that match the wildcard pattern but do not match a more precise regex:
                rjisNonZips.KeepRegex(@"RJFAF\d{3}\.[a-z]{3}$");

                // remove zip files:
                rjisNonZips.RemoveRegex(@"RJFAF\d{3}.zip$");
                if (rjisNonZips.Count != 0)
                {
                    var rjisZips = Directory.GetFiles(folder, "RJFAF*.zip").ToList();
                    rjisZips.RemoveAll(s => !Regex.Match(s, @"RJFAF\d{3}.zip$", RegexOptions.IgnoreCase).Success);
                    rjisZips.Sort((x1,x2) => GetRJISFilenameSerialNumber(x1).CompareTo(GetRJISFilenameSerialNumber(x2)));
                    if (rjisZips.Count > 0)
                    {
                        var latestRjisZip = rjisZips.Last();
                        var zipSerialNumber = Path.GetFileNameWithoutExtension(latestRjisZip).Substring(5, 3);

                        // Create temporary directory for unzipping RJIS zips:
                        var tempfolder = Path.GetTempPath();
                        var progname = Settings.ProductName;
                        tempfolder = Path.Combine(tempfolder, progname, "RJISUnzip", zipSerialNumber);
                        Directory.CreateDirectory(tempfolder);

                        var unzipNames = new List<string>();
                        using (var archive = ZipFile.OpenRead(latestRjisZip))
                        {
                            foreach (var zipEntry in archive.Entries)
                            {
                                var fullname = Path.Combine(tempfolder, zipEntry.FullName);
                                unzipNames.Add(fullname);
                                // extract if file doesn't exist
                                if (!File.Exists(fullname))
                                {
                                    zipEntry.ExtractToFile(fullname, false);
                                }
                            }
                        }
                        rjisLookup = unzipNames.ToLookup(x => Path.GetExtension(x).Substring(1), StringComparer.OrdinalIgnoreCase);
                    }
                }
                else
                {
                    var serialNumber = GetRJISFilenameSerialNumber(rjisNonZips.OrderBy(x=>GetRJISFilenameSerialNumber(x)).Last());
                    rjisLookup = rjisNonZips.ToLookup(x => Path.GetExtension(x).Substring(1), StringComparer.OrdinalIgnoreCase);
                }

                var tooMany = rjisLookup.Where(x => x.Count() > 1);
                if (tooMany.Count() > 0)
                {
                    throw new Exception("More than one RJIS set in the RJIS folder.");
                }

                var rjisClusterFile = rjisLookup["FSC"].First();

                if (!string.IsNullOrEmpty(rjisClusterFile))
                {
                    clusterToStationList = new Dictionary<string, List<string>>();
                    stationToClusterList = new Dictionary<string, List<string>>();
                    using (var fileStream = File.OpenRead(rjisClusterFile))
                    using (var streamReader = new StreamReader(fileStream))
                    {
                        string line;
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            if (line.Length == 25 && line[0] != '/')
                            {
                                var clusterId = line.Substring(1, 4);
                                var member = line.Substring(5, 4);
                                AddEntry(clusterToStationList, clusterId, member);
                                AddEntry(stationToClusterList, member, clusterId);
                            }
                        }
                    }
                }

                var rjisLocationFile = rjisLookup["LOC"].First();
                if (!string.IsNullOrEmpty(rjisLocationFile))
                {
                    using (var fileStream = File.OpenRead(rjisClusterFile))
                    using (var streamReader = new StreamReader(fileStream))
                    {
                        string line;
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            if (line[1] == 'L')
                            {
                                Console.WriteLine();
                            }
                            if (line.Length == 289 && line[0] != '/' && line[1] == 'L')
                            {
                                var nlc = line.Substring(1, 4);
                                if (!DateTime.TryParseExact(line.Substring(11, 8),
                                    "ddMMyyyy",
                                    System.Globalization.CultureInfo.InvariantCulture,
                                    System.Globalization.DateTimeStyles.None, out var tempDateTime))
                                {
                                    throw new Exception("End date invalid in RJIS location file.");
                                }

                                if (!DateTime.TryParseExact(line.Substring(19, 8),
                                    "ddMMyyyy",
                                    System.Globalization.CultureInfo.InvariantCulture,
                                    System.Globalization.DateTimeStyles.None, out tempDateTime))
                                {
                                    throw new Exception("Start date invalid in RJIS location file.");
                                }

                                if (!DateTime.TryParseExact(line.Substring(27, 8),
                                    "ddMMyyyy",
                                    System.Globalization.CultureInfo.InvariantCulture,
                                    System.Globalization.DateTimeStyles.None, out tempDateTime))
                                {
                                    throw new Exception("Quote date invalid in RJIS location file.");
                                }
                                Console.WriteLine("");
                            }

                        }
                    }
                }
            }
        }

        private static int GetRJISFilenameSerialNumber(string path)
        {
            int i = -1;
            var name = Path.GetFileNameWithoutExtension(path);
            var l = name.Length;
            if (l >= 3)
            {
                i = Convert.ToInt32(name.Substring(l - 3));
            }
            return i;
        }
    }
}
