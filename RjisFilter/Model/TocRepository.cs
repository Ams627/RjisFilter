using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace RjisFilter.Model
{
    class TocRepository
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
        private Dictionary<string, HashSet<string>> ticketRepo;
        private Dictionary<string, HashSet<string>> stationRepo;
        private Dictionary<string, HashSet<string>> routeRepo;

        public bool Saving
        {
            get
            {
                return saving;
            }
        }

        public TocRepository(Idms idms, string filename, bool autosave)
        {
            this.idms = idms;
            this.filename = filename;
            this.autosave = autosave;
            isDirty = false;
            tocs = new HashSet<string>();
            ticketRepo = new Dictionary<string, HashSet<string>>();
            stationRepo = new Dictionary<string, HashSet<string>>();
            routeRepo = new Dictionary<string, HashSet<string>>();

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
        void Save()
        {
            saving = true;

            saving = false;
        }
        void Load()
        {

        }

        void AddToc(string newToc)
        {
            tocs.Add(newToc);
            isDirty = true;
        }

        void DeleteToc(string existingToc)
        {
            tocs.Remove(existingToc);
            ticketRepo.Remove(existingToc);
            routeRepo.Remove(existingToc);
            stationRepo.Remove(existingToc);
        }

        void Add(string toc, string code)
        {
            if (!tocs.Contains(toc))
            {
                throw new Exception("Request to add station to unknown toc.");
            }
            if (Regex.Match(code, "^[0-9A-Z][0-9A-Z][0-9A-Z]$").Success)
            {

            }
        }

        void AddRange(string toc, IEnumerable<string> range)
        {

        }

        void AddTicket(string toc, string ticketCode)
        {

        }

        void AddTicketRange(string toc, IEnumerable<string> range)
        {

        }

    }
}
