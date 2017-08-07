using System.Windows;

namespace RjisFilter
{
    public interface IMyDialog
    {
        void Show(Window parent, object dataContext);
    }
}