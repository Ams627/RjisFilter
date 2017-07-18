using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvvmFoundation.Wpf;
using System.Windows;

namespace RjisFilter
{
    public class MainWindowViewModel : DependencyObject
    {
        private readonly Model model;
        private readonly IDialogService tocDialog;
        private readonly IDialogService generatingDialog;

        public ObservableCollection<string> Tocs { get; set; }

        public RelayCommand<string> ShowTocCommand { get; set; }
        public RelayCommand<string> GenerateFilteredSetCommand { get; set; }


        public string CurrentToc { get; set; }

        public MainWindowViewModel(Model model, IDialogService tocDialog, IDialogService generatingDialog)
        {
            this.model = model;
            this.tocDialog = tocDialog;
            this.generatingDialog = generatingDialog;
            CurrentToc = model.Settings.PerTocNlcList.First().Key;
            Tocs = new ObservableCollection<string>(model.Settings.PerTocNlcList.Keys);
            //ShowTocCommand = new RelayCommand<string>((toc) => {
            //    CurrentToc = toc;
            //    TocStations = new ObservableCollection<Station>(model.Settings.PerTocNlcList[toc].Select(x => new Station { Nlc = x, Crs = model.Idms.GetCrsFromNlc(x), Name = model.Idms.GetNameFromNlc(x) }));
            //    AllStations = new ObservableCollection<Station>(model.Idms.GetAllStations().Select(x => new Station { Nlc = x, Crs = model.Idms.GetCrsFromNlc(x), Name = model.Idms.GetNameFromNlc(x) }));
            //});

            ShowTocCommand = new RelayCommand<string>((toc) => {
                tocDialog.ShowDialog(model, toc);
            });

            GenerateFilteredSetCommand = new RelayCommand<string>((toc) => {
                model.Generate(toc);
                generatingDialog.ShowDialog(model, toc);
            });


            model.Rjis.PropertyChanged += Rjis_PropertyChanged;

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