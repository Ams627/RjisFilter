using System.Windows;
using MvvmFoundation.Wpf;

namespace RjisFilter.ViewModels
{
    public class ViewModelBase : DependencyObject
    {
        public object Parameter { get; set; }
    }
}
