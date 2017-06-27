using System;

namespace RjisFilter
{
    public partial class RJIS
    {
        class RJISTicketRecord
        {
            public string TicketCode { get; set; }
            public string RestrictionCode { get; set; }
            public int Price { get; set; }
            public RJISTicketRecord(string line)
            {
                TicketCode = line.Substring(9, 3);
                Price = Convert.ToInt32(line.Substring(12, 8));
                RestrictionCode = line.Substring(20, 2);
            }
        }
    }
}
