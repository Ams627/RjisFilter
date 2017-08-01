using System;
using System.Windows;
using RjisFilter.Model;

namespace RjisFilter
{
    class ActualDialog<W, VM> : IDialogService where W : Window, new() where VM : ViewModels.ViewModelBase
    {
        Func<MainModel, object, VM> generator;
        public ActualDialog(Func<MainModel, object, VM> generator)
        {
            this.generator = generator;
        }

        public void ShowDialog(MainModel model, object owner, object parameter)
        {
            var vm = generator(model, parameter);
            var window = new W()
            {
                DataContext = vm,
                Owner = owner as Window
            };
            window.ShowDialog();
        }
       
    }
}
