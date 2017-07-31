using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using RjisFilter.Model;
using MvvmFoundation.Wpf;
using System.Diagnostics;
using System.IO;

namespace RjisFilter.ViewModels
{
    class GeneratingViewModel : ViewModelBase
    {
        private readonly MainModel model;
        private readonly object parameter;

        public RelayCommand<Window> GeneratingOkCancel { get; set; }
        public RelayCommand ShowInFolder { get; set; }

        public GeneratingViewModel(MainModel model, object parameter)
        {
            this.model = model;
            this.parameter = parameter;
            model.Rjis.PropertyChanged += Rjis_PropertyChanged;
            GeneratingOkCancel = new RelayCommand<Window>(w => {
                if (Completed == 100 && w != null)
                {
                    w.Close();
                }
           });

            ShowInFolder = new RelayCommand(()=>
            {
                var (ok, folder) = model.Settings.GetFolder("output");

                if (ok)
                {
                    var subfolder = parameter.ToString();
                    var outputFolder = Path.Combine(folder, subfolder);
                    if (Directory.Exists(outputFolder))
                    {
                        Process.Start(new ProcessStartInfo()
                        {
                            FileName = outputFolder,
                            UseShellExecute = true,
                            Verb = "open"
                        });
                    }
                }
            });
        }

        private void Rjis_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "GenerateCompleted")
            {
                Completed = model.Rjis.GenerateCompleted;
            }
        }

        public int Completed
        {
            get { return (int)GetValue(CompletedProperty); }
            set { SetValue(CompletedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Completed.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CompletedProperty =
            DependencyProperty.Register("Completed", typeof(int), typeof(GeneratingViewModel), new PropertyMetadata(0));

    }
}
