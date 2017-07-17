using System.Windows;

namespace RjisFilter
{
    class ActualDialog<T> : IDialogService where T : Window, new()
    {
        object datacontext;
        public ActualDialog(object datacontext)
        {
            this.datacontext = datacontext;
        }

        public void ShowDialog()
        {
            var window = new T()
            {
                DataContext = datacontext
            };
            window.ShowDialog();
        }
    }
}
