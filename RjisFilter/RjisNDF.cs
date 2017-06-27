using System;

namespace RjisFilter
{
    class RjisNDF
    {
        public string Route { get; set; }
        public string Railcard { get; set; }
        public string TicketCode { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime QuoteDate { get; set; }
        public int AdultFare { get; set; }
        public int ChildFare { get; set; }
        public string RestrictionCode { get; set; }
        public char CrossLondon { get; set; }
        public char PrivateInd { get; set; }
    }
}
