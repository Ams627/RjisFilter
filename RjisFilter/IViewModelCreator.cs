using RjisFilter.Model;

namespace RjisFilter
{
    public interface IViewModelCreator
    {
        ViewModels.ViewModelBase Create(MainModel model, DialogManager dialogManager, object parameter);
    }
}