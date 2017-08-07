using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RjisFilter.Model;
using MvvmFoundation.Wpf;

namespace RjisFilter.ViewModels
{
    class DeleteTocViewModel
    {
        private MainModel model;
        public string Toc { get; private set; }
        public RelayCommand YesCommand { get; set; }
        public RelayCommand NoCommand { get; set; }
        public DeleteTocViewModel(MainModel model, object parameter)
        {
            this.model = model;
            Toc = parameter as string;
            YesCommand = new RelayCommand(() => System.Diagnostics.Debug.WriteLine("Yes"));
            NoCommand = new RelayCommand(() => System.Diagnostics.Debug.WriteLine("No"));
        }
    }
}
