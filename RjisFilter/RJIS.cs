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

        Dictionary<string, List<RJISStationInfo>> locationList;

        private static void AddEntry<T, U>(Dictionary<T, List<U>> d, T key, U listEntry)
        {
            if (!d.TryGetValue(key, out var list))
            {
                list = new List<U>();
                d.Add(key, list);
            }
            list.Add(listEntry);
        }


        //private static void AddEntry(Dictionary<string, List<string>> d, string key, string listEntry)
        //{
        //    if (!d.TryGetValue(key, out var list))
        //    {
        //        list = new List<string>();
        //        d.Add(key, list);
        //    }
        //    list.Add(listEntry);
        //}

        (bool, DateTime) GetRjisDate(string s)
        {
            bool result = DateTime.TryParseExact(s,
                "ddMMyyyy",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out var date);
            return (result, date);
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
                if (rjisNonZips.Count == 0)
                {
                    var rjisZips = Directory.GetFiles(folder, "RJFAF*.zip").ToList();
                    rjisZips.RemoveAll(s => !Regex.Match(s, @"RJFAF\d{3}.zip$", RegexOptions.IgnoreCase).Success);
                    rjisZips.Sort((x1, x2) => GetRJISFilenameSerialNumber(x1).CompareTo(GetRJISFilenameSerialNumber(x2)));
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
                    var serialNumber = GetRJISFilenameSerialNumber(rjisNonZips.OrderBy(x => GetRJISFilenameSerialNumber(x)).Last());
                    rjisLookup = rjisNonZips.ToLookup(x => Path.GetExtension(x).Substring(1), StringComparer.OrdinalIgnoreCase);
                }

                var tooMany = rjisLookup.Where(x => x.Count() > 1);
                if (tooMany.Count() > 0)
                {
                    throw new Exception("More than one RJIS set in the RJIS folder.");
                }

                ProcessClustersFile();
                ProcessLocationFile();
            }
        }

        private void ProcessClustersFile()
        {
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
        }

        private void ProcessLocationFile()
        {
            var rjisLocationFile = rjisLookup["LOC"].First();
            if (!string.IsNullOrEmpty(rjisLocationFile))
            {
                locationList = new Dictionary<string, List<RJISStationInfo>>();
                using (var fileStream = File.OpenRead(rjisLocationFile))
                using (var streamReader = new StreamReader(fileStream))
                {
                    string line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        if (line.Length == 289 && line[0] != '/' && line[1] == 'L')
                        {
                            var nlc = line.Substring(36, 4);

                            var (endOk, endDate) = GetRjisDate(line.Substring(9, 8));
                            var (startOk, startDate) = GetRjisDate(line.Substring(17, 8));
                            var (quoteOk, quoteDate) = GetRjisDate(line.Substring(25, 8));

                            CheckDates(endOk, startOk, quoteOk, endDate, startDate, quoteDate);
                            if (endDate.Date >= DateTime.Now.Date)
                            {
                                AddEntry(locationList, nlc, new RJISStationInfo
                                {
                                    AdminAreaCode = line.Substring(33, 2),
                                    FareGroupNLC = line.Substring(69, 4),
                                    CountyCode = line.Substring(75, 2),
                                    ZoneNumber = line.Substring(79, 4),
                                    ZoneIndicator = line.Substring(83, 1),
                                    Region = line[85],
                                    Hierarchy = line[86],
                                    EndDate = endDate,
                                    StartDate = startDate,
                                    QuoteDate = quoteDate,
                                });
                            }
                        }
                    }
                }
            }

            var poole = locationList["5883"];
            var multi = locationList.Where(x => x.Value.Count() > 1);
            Console.WriteLine();
        }

        private void CheckDates(bool endOk, bool startOk, bool quoteOk, DateTime endDate, DateTime startDate, DateTime quoteDate)
        {
            if (!endOk)
            {
                throw new Exception("End date invalid in RJIS location file.");
            }
            if (!startOk)
            {
                throw new Exception("End date invalid in RJIS location file.");
            }
            if (!quoteOk)
            {
                throw new Exception("End date invalid in RJIS location file.");
            }
            if (endDate.Date < startDate.Date)
            {
                throw new Exception("End date is before start date");
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
