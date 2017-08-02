using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RjisFilter.Model;
using System.Windows;

namespace RjisFilter
{
    public class DialogManager
    {
        public class DialogX
        {
            public IDialogService DialogService { get; set; }
            public IViewModelCreator ViewModelCreator { get; set; }
        }

        private MainModel model;

        private Dictionary<string, DialogX> dialogDict;
        public DialogManager(MainModel model)
        {
            dialogDict = new Dictionary<string, DialogX>();
            this.model = model;
        }

        void AddDialog(string key, IDialogService dialog, IViewModelCreator vmc)
        {
            dialogDict.Add(key, new DialogX { DialogService = dialog, ViewModelCreator = vmc });
        }

        /// <summary>
        /// Create a view model and a dialog
        /// </summary>
        /// <param name="dialogName">String representing the dialog to show</param>
        /// <param name="owner">Window owning the dialog</param>
        /// <param name="dialogParameter">parameter to pass to the dialog</param>
        /// <param name="vmParameter">parameter to pass to the view model behind the dialog</param>
        public void ShowDialog(string dialogName, Window owner, object dialogParameter = null, object vmParameter = null)
        {
            dialogDict.TryGetValue(dialogName, out var dialog);
            var vmc = dialog.ViewModelCreator.Create(model, this, vmParameter);

            dialog.DialogService.ShowDialog(model, owner, dialogParameter);
        }
    }
}
