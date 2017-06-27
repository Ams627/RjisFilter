using System;

namespace RjisFilter
{
    public partial class RJIS
    {
        class RJISStationInfo
        {
            public string AdminAreaCode { get; set; }
            public char Hierarchy { get; set; }
            public string FareGroupNLC { get ; set ; }
            public char Region { get; set; }
            public string ZoneIndicator { get; set; }
            public string ZoneNumber { get; set; }
            public string CountyCode { get; set; }
            public DateTime EndDate { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime QuoteDate { get; set; }
        }
    }
}
