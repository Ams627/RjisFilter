namespace RjisFilter
{
    public class CallingPoint
    {
        public string Crs { get; set; }
        public string Tiploc { get; set; }
        public CallingType CallingType { get; set; }
        public int Arrival { get; set; }
        public int Departure { get; set; }
        public int PublicArrival { get; set; }
        public int PublicDeparture { get; set; }
        public string Platform { get; set; }
        public int LineNumber { get; set; }
    }
}