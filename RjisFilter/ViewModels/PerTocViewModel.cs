using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RjisFilter.ViewModels
{
    /// <summary>
    /// This class provides all the information for editing a single TOC. We should be able to edit
    /// a ticket type list and a station list for the TOC. The ticket type list is read from the
    /// settings file. 
    /// </summary>
    class PerTocViewModel
    {
        private readonly Model model;
        public PerTocViewModel(Model model)
        {
            this.model = model;
        }
    }
}
