using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using RjisFilter.Model;

namespace RjisFilter.ViewModels
{
    class GeneratingViewModel : ViewModelBase
    {
        private readonly MainModel model;
        private readonly object parameter;

        public GeneratingViewModel(MainModel model, object parameter)
        {
            this.model = model;
            this.parameter = parameter;
            model.Rjis.PropertyChanged += Rjis_PropertyChanged;
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
