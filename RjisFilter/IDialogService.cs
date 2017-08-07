using RjisFilter.Model;
using System.Windows;

namespace RjisFilter
{
    public interface IDialogService
    {
        void ShowDialog(MainModel model, object owner, object viewModel);
    }
}
