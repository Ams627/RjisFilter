using System;

namespace RjisFilter
{
    class RJISFlowValue
    {
        public RJISFlowValue(string line)
        {
            if (line[0] != 'R' && line[1] != 'F' && line.Length != 49)
            {
                throw new Exception("Bad flow line");
            }
            Route = line.Substring(10, 5);
            int d, m, y;
            d = int.Parse(line.Substring(20, 2));
            m = int.Parse(line.Substring(22, 2));
            y = int.Parse(line.Substring(24, 4));
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

            DiscountInd = int.Parse(line.Substring(40, 1));
            FlowId = int.Parse(line.Substring(42, 7));
        }

        public string Route { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public int DiscountInd { get; private set; }
        public int FlowId { get; private set; }
    }
}
