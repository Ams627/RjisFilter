using System;

namespace RjisFilter
{
    class FactoryAttribute : Attribute
    {
        private string designator;
        public FactoryAttribute(string designator)
        {
            this.designator = designator;
        }

        public string Designator
        {
            get
            {
                return designator;
            }
        }
    }
}
