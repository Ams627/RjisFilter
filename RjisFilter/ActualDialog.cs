using System;
using System.Windows;
using RjisFilter.Model;

namespace RjisFilter
{
    public class ActualDialog : IDialogService
    {
        public void ShowDialog(MainModel model, object owner, object viewModel)
        {
            var window = new Windows.DialogContainer()
            {
                Owner = owner as Window,
                DataContext = viewModel
            };
            window.ShowDialog();
        }
    }
}
