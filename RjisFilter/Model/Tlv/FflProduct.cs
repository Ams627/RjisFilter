using System;
using System.Collections.Generic;
using TlvSerialise;

namespace RjisFilter.Model
{
    public class FflProduct : TlvSerialisable
    {
        [Tlv(TlvTypes.String, TlvTags.ID_PROD_FFL_ORIG_CODE)] public string Orig { get; set; }
        [Tlv(TlvTypes.String, TlvTags.ID_PROD_FFL_DEST_CODE)] public string Dest { get; set; }
        [Tlv(TlvTypes.String, TlvTags.ID_PROD_FFL_ROUTE_CODE)] public string Route { get; set; }
        [Tlv(TlvTypes.String, TlvTags.ID_PROD_FFL_STATUS_CODE)] public string Status { get; private set; } = "000";
        [Tlv(TlvTypes.Date, TlvTags.ID_PROD_FFL_END_DATE), TlvEndDate] public DateTime EndDate { get; set; }
        [Tlv(TlvTypes.Date, TlvTags.ID_PROD_FFL_START_DATE)] public DateTime StartDate { get; set; }
        [Tlv(TlvTypes.String, TlvTags.ID_PROD_FFL_TOC)] public string Toc { get; set; }
        [Tlv(TlvTypes.UInt, TlvTags.ID_PROD_FFL_CROSS_LONDON_IND)] public int CrossLondonInd { get; set; }
        [Tlv(TlvTypes.UInt, TlvTags.ID_PROD_FFL_NS_DISC_IND)] public int NsDiscountInd { get; set; }
        [Tlv(TlvTypes.Array, TlvTags.ID_PROD_FFL_PRODUCT_DESC)] public List<FflFare> FareRecords { get; set; }

        public FflProduct()
        {
            FareRecords = new List<FflFare>();
        }
    }
}