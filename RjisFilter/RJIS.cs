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
    class RJIS
    {
        private readonly Settings settings;

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
            string rjisClusterFile = null;
            if (ok)
            {
                var rjisZips = Directory.GetFiles(folder, "RJFAF*.zip").ToList();
                rjisZips.RemoveAll(s => !Regex.Match(s, @"RJFAF\d{3}.zip$", RegexOptions.IgnoreCase).Success);
                var rjisClusters = Directory.GetFiles(folder, "RJFAF*.FSC").ToList();
                rjisClusters.RemoveAll(s => !Regex.Match(s, @"^RJFAF\d{3}.fsc$", RegexOptions.IgnoreCase).Success);
                if (rjisClusters.Count > 0)
                {
                    rjisClusterFile = rjisClusters.Last();
                }
                else if (rjisZips.Count > 0)
                {
                    var latestRjisZip = rjisZips.Last();
                    var serialNumber = Path.GetFileNameWithoutExtension(latestRjisZip).Substring(5, 3);
                    var tempfolder = Path.GetTempPath();
                    var progname = Settings.ProductName;
                    tempfolder = Path.Combine(tempfolder, progname, "RJISUnzip", serialNumber);
                    if (!Directory.Exists(tempfolder))
                    {
                        Directory.CreateDirectory(tempfolder);
                    }
                    using (var archive = ZipFile.OpenRead(latestRjisZip))
                    {
                        foreach(var zipEntry in archive.Entries)
                        {
                            var fullname = Path.Combine(tempfolder, zipEntry.FullName);
                            zipEntry.ExtractToFile(fullname, true);
                            if (Path.GetExtension(fullname).Equals(".FSC", StringComparison.OrdinalIgnoreCase))
                            {
                                rjisClusterFile = fullname;
                            }
                        }
                    }
                }
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
            }
        }
    }
}
