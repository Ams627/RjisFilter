using System;

namespace RjisFilter
{
    public class RjisNDF
    {
        public string Origin { get; set; }
        public string Destination { get; set; }
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

        public override string ToString()
        {
            return $"R{Origin}{Destination}{Route}{Railcard}{TicketCode}Y{EndDate:ddMMyyyy}{StartDate:ddMMyyyy}{QuoteDate:ddMMyyyy}N{AdultFare:D8}{ChildFare:D8}{RestrictionCode}{CrossLondon}{PrivateInd}";
        }
    }
}
