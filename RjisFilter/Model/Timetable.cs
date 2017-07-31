using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace RjisFilter.Model
{
    public class TrainRun
    {
        public TrainRun()
        {
            CallingPoints = new List<CallingPoint>();
        }
        public List<CallingPoint> CallingPoints { get; set; }
    }
    
    public class FullTimeTable
    {
        public FullTimeTable()
        {
            TrainRuns = new List<TrainRun>();
        }
        public List<TrainRun> TrainRuns { get; set; }
    }

    public class Connection
    {
        public TrainRun Trainrun { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public bool DoesTerminate { get; set; }
    }

    public partial class Timetable
    {
        private Settings settings;
        private Idms idms;

        private Dictionary<string, string> timetableFilenameDict;
        FullTimeTable fullTimetable = new FullTimeTable();
        Dictionary<string, List<Connection>> connections;

        public Timetable(Settings settings, Idms idms)
        {
            this.settings = settings;
            this.idms = idms;

            var (ok, folder) = settings.GetFolder("Timetable");
            if (ok)
            {
                var ttfNonZips = Directory.GetFiles(folder, "ttisf*").ToList();

                // remove non-rjis files that match the wildcard pattern but do not match a more precise regex:
                ttfNonZips.KeepRegex(@"ttisf\d{3}\.[a-z]{3}$");

                if (ttfNonZips.Count == 0)
                {
                    var rjisZips = Directory.GetFiles(folder, "ttf*.zip").ToList();
                }
                else
                {
                    var serialNumber = GetTimetableFilenameSerialNumber(ttfNonZips.OrderBy(x => GetTimetableFilenameSerialNumber(x)).Last());
                    var rjisLookup = ttfNonZips.ToLookup(x => Path.GetExtension(x).Substring(1), StringComparer.OrdinalIgnoreCase);
                    var tooMany = rjisLookup.Where(x => x.Count() > 1);
                    if (tooMany.Count() > 0)
                    {
                        throw new Exception("More than one timetable set in the timetable folder.");
                    }
                    timetableFilenameDict = rjisLookup.ToDictionary(x => x.Key, x => x.First());
                }

                var tasklist = new List<Action> { };//ProcessMcaFile };
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
                        //Application.Current.Dispatcher.Invoke(() => LinesRead = linesRead);
                        // Ready = true;
                        var end = DateTime.Now;
                        var result = (end - start).TotalSeconds;

                        System.Diagnostics.Debug.WriteLine($"total seconds {result}");
                    }
                });
            }
        }

        private int GetTimetableFilenameSerialNumber(string filename)
        {
            return Convert.ToInt32(Path.GetFileName(filename).Substring(5, 3));
        }

        class BadTip
        {
            public int LineNumber { get; set; }
            public string Tiploc { get; set; }
        }
        private void ProcessMcaFile()
        {
            Dictionary<string, List<int>> badTips = new Dictionary<string, List<int>>();

            timetableFilenameDict.TryGetValue("mca", out var filename);
            if (!string.IsNullOrWhiteSpace(filename))
            {
                using (var fileStream = File.OpenRead(filename))
                using (var streamReader = new StreamReader(fileStream))
                {
                    string line;
                    int linenumber = 0;
                    TrainRun onerun = null;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        if (line.Length == 80 && line[0] == 'L')
                        {
                            if (onerun == null)
                            {
                                onerun = new TrainRun();
                            }
                            var tiploc = line.Substring(2, 7).TrimEnd();
                            var crs = idms.GetCrsFromTiploc(tiploc);
                            if (string.IsNullOrWhiteSpace(crs) || crs.Length != 3)
                            {
                                if (line[1] == 'O' || line[1] == 'T' || (line[1] == 'I' && !IsPassingPoint(line)))
                                {
                                    DictUtils.AddEntry(badTips, tiploc, linenumber + 1);
                                }
                            }
                            if (line[1] == 'O')
                            {
                                var callingPoint = new CallingPoint
                                {
                                    CallingType = CallingType.Origin,
                                    Crs = crs,
                                    Tiploc = tiploc,
                                    PublicDeparture = GetMinutes(line, 15),
                                    LineNumber = linenumber
                                };
                                onerun.CallingPoints.Add(callingPoint);
                            }
                            else if (line[1] == 'I' && !IsPassingPoint(line))
                            {
                                // intermediate station
                                var callingPoint = new CallingPoint
                                {
                                    CallingType = CallingType.Intermediate,
                                    Crs = crs,
                                    Tiploc = tiploc,
                                    Arrival = GetMinutes(line, 10),
                                    Departure = GetMinutes(line, 15),
                                    PublicArrival = GetMinutes(line, 25),
                                    PublicDeparture = GetMinutes(line, 29),
                                    Platform = line.Substring(33, 3),
                                    LineNumber = linenumber
                                };
                                onerun.CallingPoints.Add(callingPoint);
                                if (linenumber >= 108192)
                                {
                                    Console.WriteLine();
                                }
                            }
                            else if (line[1] == 'T')
                            {
                                // terminating station
                                var callingPoint = new CallingPoint
                                {
                                    CallingType = CallingType.Terminating,
                                    Crs = crs,
                                    Tiploc = tiploc,
                                    Arrival = GetMinutes(line, 10),
                                    PublicArrival = GetMinutes(line, 15),
                                    Platform = line.Substring(19, 3),
                                    LineNumber = linenumber
                                };
                                onerun.CallingPoints.Add(callingPoint);
                                if (onerun.CallingPoints.Count() > 200)
                                {
                                    Console.WriteLine();
                                }
                                fullTimetable.TrainRuns.Add(onerun);
                                onerun = null;
                            }

                        }
                        linenumber++;
                        if (linenumber % 1_000_000 == 999_999)
                        {
                            System.Diagnostics.Debug.WriteLine($"line {linenumber + 1}");
                        }
                    }
                } // using

                connections = new Dictionary<string, List<Connection>>();

                System.Diagnostics.Debug.WriteLine($"There are {fullTimetable.TrainRuns.Count} train runs.");
                var currentRun = 0;
                var innerCount = 0;
                foreach (var run in fullTimetable.TrainRuns)
                {
                    if (run.CallingPoints[0].Crs != null)
                    {
                        for (var i = 0; i < run.CallingPoints.Count - 1; i++)
                        {
                            for (var j = 1; j < run.CallingPoints.Count; j++)
                            {
                                if (run.CallingPoints[i].Crs != null && run.CallingPoints[j].Crs != null)
                                {
                                    var crsFlow = run.CallingPoints[i].Crs + run.CallingPoints[j].Crs;
                                    var connection = new Connection { Trainrun = run, StartIndex = i, EndIndex = j, DoesTerminate = j == run.CallingPoints.Count() };
                                    DictUtils.AddEntry(connections, crsFlow, connection);
                                    innerCount++;
                                }
                            }
                        }
                    }
                    currentRun++;
                    if (currentRun % 10000 == 9999)
                    {
                        System.Diagnostics.Debug.WriteLine($"Current run: {currentRun + 1}");
                    }
                }
            }
        }

        private bool IsPassingPoint(string s)
        {
            return !(s[20] == ' ' && s[21] == ' ' && s[22] == ' ' && s[23] == ' ');
        }

        /// <summary>
        /// Return the time in minutes-from-midnight from a four character string hhmm or -1 if the input string contains spaces
        /// </summary>
        /// <param name="s">the string</param>
        /// <returns></returns>
        private int GetMinutes(string s, int index)
        {
            if (s[index] == 32 && s[index + 1] == 32 && s[index + 2] == 32 && s[index + 3] == 32)
            {
                return -1;
            }
            if (s.Skip(index).Take(4).Any(x=>!char.IsDigit(x)))
            {
                throw new Exception("Illegal character in time.");
            }
            return 600 * (s[index] - '0') + 60 * (s[index + 1] - '0') + 10 * (s[index + 2] - '0') + s[index + 3] - '0';
        }
    }
}