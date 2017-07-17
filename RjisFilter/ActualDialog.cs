using System;
using System.Windows;

namespace RjisFilter
{
    class ActualDialog<W, VM> : IDialogService where W : Window, new() where VM : ViewModels.ViewModelBase
    {
        Func<Model, object, VM> generator;
        public ActualDialog(Func<Model, object, VM> generator)
        {
            this.generator = generator;
        }

        public void ShowDialog(Model model, object parameter)
        {
            var vm = generator(model, parameter);
            var window = new W()
            {
                DataContext = vm,
            };
            window.ShowDialog();
        }

        
    }
}
