using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RjisFilter.Model;
using MvvmFoundation.Wpf;
using System.Windows;

namespace RjisFilter.ViewModels
{
    class AddTocViewModel : ViewModelBase
    {
        private MainModel model;
        public RelayCommand<object> OkCommand { get; set; }
        public string TocsToAdd { get; set; }

        public AddTocViewModel(MainModel model, object parameter)
        //public AddTocViewModel(MainModel model, object param)
        {
            this.model = model;
            OkCommand = new RelayCommand<object>((w) =>
            {
                if (w is Window window)
                {
                    window.Close();
                }
            });
        }
    }
}
