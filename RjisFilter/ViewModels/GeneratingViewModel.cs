using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RjisFilter.ViewModels
{
    class GeneratingViewModel : ViewModelBase
    {
        private readonly Model model;
        private readonly object parameter;

        public GeneratingViewModel(Model model, object parameter)
        {
            this.model = model;
            this.parameter = parameter;
        }
    }
}
