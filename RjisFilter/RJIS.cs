using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using System.Windows;

namespace RjisFilter
{
    public partial class RJIS : INotifyPropertyChanged
    {
        private readonly Settings settings;
        private ILookup<string, string> rjisLookup;

        public Dictionary<string, List<string>> ClusterToStationList { get; private set; }
        public Dictionary<string, List<string>> StationToClusterList { get; private set; }
        public Dictionary<string, List<RJISStationInfo>> LocationList { get; private set; }
        public Dictionary<string, List<RJISFlowValue>> FlowDict { get; private set; }
        public Dictionary<int, List<RJISTicketRecord>> TicketDict { get; private set; }
        public Dictionary<string, List<string>> StationToGroupIds { get; private set; }
        public Dictionary<string, List<string>> GroupIdToStationList { get; private set; }
        public Dictionary<string, string> StationtToZoneNlc { get; private set; }

        public Dictionary<string, List<RjisNDF>> NdfList { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private bool ready = false;
        private int linesRead = 0;

        public bool Ready { get => ready; set { ready = value; NotifyPropertyChanged(); } }
        public int LinesRead { get => linesRead; set { linesRead = value; NotifyPropertyChanged(); } }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static void AddEntry<T, U>(Dictionary<T, List<U>> d, T key, U listEntry)
        {
            if (!d.TryGetValue(key, out var list))
            {
                list = new List<U>();
                d.Add(key, list);
            }
            list.Add(listEntry);
        }

        private void AddLine()
        {
            linesRead++;
            if (linesRead % 10_000 == 0)
            {
                // cause a notification:
                Application.Current.Dispatcher.Invoke(() => LinesRead = linesRead);
            }
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

                var tasklist = new List<Action> { }; // ProcessClustersFile, ProcessLocationFile, ProcessFlowFile, ProcessNDFFile };
                var tlist = tasklist.Select(t => Task.Run(t));

                var start = DateTime.Now;
                Task all = Task.WhenAll(tlist).ContinueWith((task) =>
                {
                    if (task.IsCanceled)
                    {
                        System.Diagnostics.Debug.WriteLine("Cancelled");
                    }
                    else if (task.IsFaulted)
                    {
                        var ex = task.Exception;
                        System.Diagnostics.Debug.WriteLine($"Fault: exception is {ex.Message}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Task OK");
                        Application.Current.Dispatcher.Invoke(() => LinesRead = linesRead);
                        Ready = true;
                        var end = DateTime.Now;
                        var result = (end - start).TotalSeconds;

                        System.Diagnostics.Debug.WriteLine($"total seconds {result}");
                    }
                });
            }
        }

        private void ProcessClustersFile()
        {
            var rjisClusterFile = rjisLookup["FSC"].First();

            if (!string.IsNullOrEmpty(rjisClusterFile))
            {
                ClusterToStationList = new Dictionary<string, List<string>>();
                StationToClusterList = new Dictionary<string, List<string>>();
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
                            AddEntry(ClusterToStationList, clusterId, member);
                            AddEntry(StationToClusterList, member, clusterId);
                        }
                        AddLine();
                    }
                }
            }
        }

        private void ProcessLocationFile()
        {
            var rjisLocationFile = rjisLookup["LOC"].First();
            if (!string.IsNullOrEmpty(rjisLocationFile))
            {
                LocationList = new Dictionary<string, List<RJISStationInfo>>();
                using (var fileStream = File.OpenRead(rjisLocationFile))
                using (var streamReader = new StreamReader(fileStream))
                {
                    string line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        if (line.Length == 289 && line[0] != '/' && line[1] == 'L')
                        {
                            var nlc = line.Substring(36, 4);

                            var (endOk, endDate) = RjisUtils.GetRjisDate(line.Substring(9, 8));
                            var (startOk, startDate) = RjisUtils.GetRjisDate(line.Substring(17, 8));
                            var (quoteOk, quoteDate) = RjisUtils.GetRjisDate(line.Substring(25, 8));

                            CheckDates(endOk, startOk, quoteOk, endDate, startDate, quoteDate);
                            if (endDate.Date >= DateTime.Now.Date)
                            {
                                var stationInfo = new RJISStationInfo
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
                                };
                                AddEntry(LocationList, nlc, stationInfo);
                                if (nlc != stationInfo.FareGroupNLC)
                                {
                                    AddEntry(StationToGroupIds, nlc, stationInfo.FareGroupNLC);
                                    AddEntry(GroupIdToStationList, stationInfo.FareGroupNLC, nlc);
                                }
                                if (stationInfo.ZoneNumber.All(x=>Char.IsDigit(x)))
                                {
                                    StationtToZoneNlc.Add(nlc, stationInfo.ZoneNumber);
                                }
                            }
                        }
                        AddLine();
                    }
                }
            }

            var poole = LocationList["5883"];
            var multi = LocationList.Where(x => x.Value.Count() > 1);
            Console.WriteLine();
        }


        void ProcessFlowFile()
        {
            var rjisFlowFile = rjisLookup["FFL"].First();
            using (var reader = new StreamReader(rjisFlowFile))
            {
                var linenumber = 0;
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Substring(0, 2) == "RF")
                    {
                        var flow = new RjisFlow(line.Substring(2, 8));
                        var flowValue = new RJISFlowValue(line);
                        if (flowValue.EndDate.Date >= DateTime.Now.Date)
                        {
                            DictUtils.AddEntry(FlowDict, flow.FlowKey, flowValue);
                            if (line[19] == 'R')
                            {
                                DictUtils.AddEntry(FlowDict, flow.GetReversedFlow().FlowKey, flowValue);
                            }
                        }
                    }
                    else if (line.Substring(0, 2) == "RT")
                    {
                        var key = Convert.ToInt32(line.Substring(2, 7));
                        var ticketValue = new RJISTicketRecord(line);
                        DictUtils.AddEntry(TicketDict, key, ticketValue);
                    }
                    linenumber++;
                    AddLine();
                }
            }
        }

        void ProcessNDFFile()
        {
            try
            {
                var rjisFlowFile = rjisLookup["NFO"].First();

                using (var reader = new StreamReader(rjisFlowFile))
                {
                    NdfList = new Dictionary<string, List<RjisNDF>>();
                    var linenumber = 0;
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line[0] == 'R')
                        {
                            var flow = new RjisFlow(line.Substring(1, 8));
                            var (endOk, endDate) = RjisUtils.GetRjisDate(line.Substring(21, 8));
                            var (startOk, startDate) = RjisUtils.GetRjisDate(line.Substring(29, 8));
                            var (quoteOk, quoteDate) = RjisUtils.GetRjisDate(line.Substring(37, 8));
                            CheckDates(endOk, startOk, quoteOk, endDate, startDate, quoteDate);

                            var ndf = new RjisNDF
                            {
                                Route = line.Substring(9, 5),
                                Railcard = line.Substring(14, 3),
                                TicketCode = line.Substring(17, 3),
                                EndDate = endDate,
                                StartDate = startDate,
                                QuoteDate = quoteDate,
                                AdultFare = Convert.ToInt32(line.Substring(46, 8)),
                                ChildFare = Convert.ToInt32(line.Substring(54, 8)),
                                RestrictionCode = line.Substring(62, 2),
                                CrossLondon = line[65],
                                PrivateInd = line[66]
                            };

                            if (endDate.Date >= DateTime.Now.Date)
                            {
                                DictUtils.AddEntry(NdfList, flow.FlowKey, ndf);
                            }
                        }
                        linenumber++;
                        AddLine();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
                throw;
            }
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

        void GenerateOutputFiles(string toc)
        {
            var (ok, outputFolder) = settings.GetFolder("output");
            if (ok)
            {
                var (oktemp, tempFolder) = settings.GetFolder("temp");
                if (oktemp)
                {
                    var outputFfl = Path.Combine(tempFolder, Path.GetFileName(rjisLookup["FFL"].First()));
                    var outputNFO = Path.Combine(tempFolder, Path.GetFileName(rjisLookup["NFO"].First()));
                    settings.PerTocNlcList.TryGetValue(toc, out var originSet);
                    if (originSet != null && originSet.Count > 0)
                    {
                        var groupList = originSet.SelectMany(x => DictUtils.GetResults(StationToGroupIds, x)).GroupBy(x => x).Select(y => y.First());
                        var stationsAndGroupList = groupList.Concat(originSet);
                        var clusterList = stationsAndGroupList.SelectMany(x => DictUtils.GetResults(StationToClusterList, x)).GroupBy(x => x).Select(y => y.First());
                        var zoneList = originSet.Select(x => DictUtils.GetResult(StationtToZoneNlc, x)).Where(x => x != string.Empty).GroupBy(x => x).Select(y => y.First());
                        var allSearchStations = clusterList.Concat(groupList).Concat(zoneList).Concat(originSet);

                        var outputFlowDictionary = new Dictionary<string, List<RJISFlowValue>>();
                        var flowIdList = new List<int>();

                        using (var outputStream = new StreamWriter(outputFfl))
                        {
                            foreach (var flow in FlowDict)
                            {
                                foreach (var flowV in flow.Value)
                                {
                                    var origin = flow.Key.Substring(0, 4);
                                    if (allSearchStations.Contains(origin))
                                    {

                                    }
                                    outputStream.Write(flow.Key);
                                    outputStream.Write(flow.Value);
                                    flowIdList.AddRange(flow.Value.Select(x => x.FlowId));
                                }
                            }
                        }
                        var allSearchStationsNDF = groupList.Concat(zoneList).Concat(originSet);
                        foreach (var flow in NdfList)
                        {
                            var origin = flow.Key.Substring(0, 4);
                            if (allSearchStations.Contains(origin))
                            {

                            }
                        }
                    }
                }
            }

        }
    }
}
