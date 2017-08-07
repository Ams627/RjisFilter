using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RjisFilter.Model;
using MvvmFoundation.Wpf;
using System.Windows;
using System.Threading;

namespace RjisFilter.ViewModels
{
    public class AddTocViewModel : ViewModelBase
    {
        private MainModel model;
        public RelayCommand<object> OkCommand { get; set; }
        public string TocsToAdd { get; set; }

        public bool IsTocEnteredInvalid
        {
            get { return (bool)GetValue(IsTocEnteredInvalidProperty); }
            set { SetValue(IsTocEnteredInvalidProperty, value); }
        }

        public static readonly DependencyProperty IsTocEnteredInvalidProperty =
            DependencyProperty.Register("IsTocEnteredInvalid", typeof(bool), typeof(AddTocViewModel), new PropertyMetadata(false));

        public AddTocViewModel(MainModel model, object parameter)
        //public AddTocViewModel(MainModel model, object param)
        {
            this.model = model;
            OkCommand = new RelayCommand<object>((w) =>
            {
                if (!string.IsNullOrWhiteSpace(TocsToAdd))
                {
                    var alltocs = TocsToAdd.Split(',').Select(x => x.Trim());
                    if (alltocs != null && alltocs.Any(x => x.IndexOfAny("&<>:;\"\t".ToCharArray()) != -1))
                    {
                        IsTocEnteredInvalid = true; Task.Run(() => Thread.Sleep(2000)).ContinueWith(x => Dispatcher.Invoke(() => IsTocEnteredInvalid = false));
                    }
                    else if (w is Window window)
                    {
                        model.TocRepository.AddTocRange(alltocs);
                        window.Close();
                    }
                }
            });
        }
    }
}
