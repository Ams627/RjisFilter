using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvvmFoundation.Wpf;
using System.Windows;
using RjisFilter.Model;
using RjisFilter.ViewModels;

using System.Windows.Threading;

namespace RjisFilter
{
    public class MainWindowViewModel : DependencyObject
    {
        private readonly MainModel model;
        private readonly IDialogService dialogService;

        public ObservableCollection<string> Tocs { get; set; }

        public RelayCommand<object> ShowTocCommand { get; set; }
        public RelayCommand<object> DeleteTocCommand { get; set; }
        public RelayCommand<string> GenerateFilteredSetCommand { get; set; }
        public RelayCommand<string> GenerateTLVCommand { get; set; }
        public RelayCommand<object> AddTocCommand { get; set; }

        public string CurrentToc { get; set; }

        public MainWindowViewModel(MainModel model, IDialogService dialogService)
        {
            this.model = model;
            this.dialogService = dialogService;

            Tocs = new ObservableCollection<string>(model.TocRepository.GetTocs());
            //ShowTocCommand = new RelayCommand<string>((toc) => {
            //    CurrentToc = toc;
            //    TocStations = new ObservableCollection<Station>(model.Settings.PerTocNlcList[toc].Select(x => new Station { Nlc = x, Crs = model.Idms.GetCrsFromNlc(x), Name = model.Idms.GetNameFromNlc(x) }));
            //    AllStations = new ObservableCollection<Station>(model.Idms.GetAllStations().Select(x => new Station { Nlc = x, Crs = model.Idms.GetCrsFromNlc(x), Name = model.Idms.GetNameFromNlc(x) }));
            //});

            ShowTocCommand = new RelayCommand<object>((owner) => {
                var viewModel = new ViewModels.PerTocViewModel(model, null);
                dialogService.ShowDialog(model, owner, viewModel);
            });

            GenerateFilteredSetCommand = new RelayCommand<string>((owner) => {
                if (!string.IsNullOrWhiteSpace(CurrentToc))
                {
                    model.GenerateFilteredSet(CurrentToc);
                    var viewModel = new ViewModels.GeneratingViewModel(model, null);
                    dialogService.ShowDialog(model, owner, viewModel);
                }
            });

            AddTocCommand = new RelayCommand<object>((owner) =>
            {
                var viewmodel = new AddTocViewModel(model, null);
                dialogService.ShowDialog(model, owner, viewmodel);
            });

            DeleteTocCommand = new RelayCommand<object>((owner) =>
            {
                var viewmodel = new DeleteTocViewModel(model, CurrentToc);
                dialogService.ShowDialog(model, owner, viewmodel);
            });

            model.Rjis.PropertyChanged += Rjis_PropertyChanged;
            model.TocRepository.PropertyChanged += (s, e) =>
            {
                Tocs.Clear();
                foreach (var toc in model.TocRepository.GetTocs())
                {
                    Tocs.Add(toc);
                }
            };

        }


        public int LinesRead
        {
            get { return (int)GetValue(LinesReadProperty); }
            set { SetValue(LinesReadProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LinesRead.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LinesReadProperty =
            DependencyProperty.Register("LinesRead", typeof(int), typeof(MainWindowViewModel), new PropertyMetadata(0));



        private void Rjis_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "LinesRead")
            {
                LinesRead = model.Rjis.LinesRead;
            }
        }
    }
}