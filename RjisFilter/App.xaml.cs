using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using RjisFilter.ViewModels;
using RjisFilter.Model;

namespace RjisFilter
{
    public partial class App : Application
    {
        private Settings settings;
        /// <summary>
        /// for ShowWindow win32 API function
        /// </summary>
        private const int SW_SHOWNORMAL = 1;

        static Mutex _instanceMutex;
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // First check for an existing instance of this application:
            // Mutex constructor returns false if the named mutex already exists on the machine. If
            // it does already exist, we already have an instance of this application so we simply exit.
            // (The name of the Mutex is simply a random GUID)
            bool createdNew;
            _instanceMutex = new Mutex(true, "RJISFilter-865DB292-E4A1-4A98-AD30-05AEB74BCF4A", out createdNew);

            if (!createdNew)
            {
                ShowExistingInstance();
                // this app is already running so bring previous instance to the front then forcibly exit:
                Environment.Exit(0);
            }

            settings = new Settings();
            var idms = new Idms(settings);
            while (!idms.Ready)
            {
                Thread.Sleep(10);
            }
            var tocRepository =  new TocRepository(idms, false);
            var rjis = new RJIS(settings);
            var timetable = new Timetable(settings, idms);
            try
            {
                var model = new MainModel(settings, tocRepository, rjis, idms, timetable, new RouteingGuide());

                var tocdialog = new ActualDialog<TocEditor, PerTocViewModel>((a,b)=>new PerTocViewModel(a, b));

                var generating = new ActualDialog<Windows.Generating, ViewModels.GeneratingViewModel>((a, b) => new GeneratingViewModel(a, b));

                var mainWindowViewModel = new MainWindowViewModel(model, tocdialog, generating);
                var window = new MainWindow(mainWindowViewModel);
                window.Show();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"{ex.ToString()}");
            }
        }


        [DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        /// <summary>
        /// Show the single existing instance of this program:
        /// </summary>
        private void ShowExistingInstance()
        {
            var currentProcess = Process.GetCurrentProcess();
            var processes = Process.GetProcessesByName(currentProcess.ProcessName);
            foreach (var process in processes)
            {
                // the instance already open should have a MainWindowHandle
                if (process.MainWindowHandle != IntPtr.Zero)
                {
                    // restore the window in case it was minimized
                    ShowWindow(process.MainWindowHandle, SW_SHOWNORMAL);

                    // bring the window to the foreground
                    SetForegroundWindow(process.MainWindowHandle);
                    break;
                }
            }
        }







    }
}
