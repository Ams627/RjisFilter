using System;

namespace RjisFilter
{
    public class RJISFlowValue
    {
        public RJISFlowValue(string line)
        {
            if (line[0] != 'R' && line[1] != 'F' && line.Length != 49)
            {
                throw new Exception("Bad flow line");
            }
            Origin = line.Substring(2, 4);
            Destination = line.Substring(6, 4);
            Route = line.Substring(10, 5);
            UsageCode = line[18];
            bool ok;
            (ok, EndDate) = RjisUtils.GetRjisDate(line.Substring(20, 8));
            if (!ok)
            {
                throw new Exception("Invalid end date");
            }
            (ok, StartDate) = RjisUtils.GetRjisDate(line.Substring(28, 8));
            if (!ok)
            {
                throw new Exception("invalid start date");
            }
            Direction = line[19];
            Toc = line.Substring(36, 3);
            CrossLondonInd = Convert.ToInt32(line[39]);
            DiscountInd = int.Parse(line.Substring(40, 1));
            PubInd = line[41];
            FlowId = int.Parse(line.Substring(42, 7));
        }

        public string Origin { get; set; }
        public string Destination { get; set; }
        public string Route { get; private set; }
        public char UsageCode { get; set; }
        public char Direction { get; set; }
        public DateTime EndDate { get; private set; }
        public DateTime StartDate { get; private set; }
        public string Toc { get; set; }
        public int CrossLondonInd { get; set; }
        public char PubInd { get; set; }
        public int DiscountInd { get; private set; }
        public int FlowId { get; private set; }
        public bool Wanted { get; set; }

        public override string ToString()
        {
            return $"RF{Origin}{Destination}{Route}000{UsageCode}{Direction}{EndDate:ddMMyyyy}{StartDate:ddMMyyyy}{Toc}{CrossLondonInd}{PubInd}{DiscountInd}{FlowId:D7}";
        }
    }
}
