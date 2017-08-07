using TlvSerialise;

namespace RjisFilter.Model
{
    public class FflFare : TlvSerialisable
    {
        [Tlv(TlvTypes.String, TlvTags.ID_PROD_FFL_TICKET_CODE)] public string TicketCode { get; set; }
        [Tlv(TlvTypes.UInt, TlvTags.ID_PROD_FFL_FARE)] public int Price { get; set; }
        [Tlv(TlvTypes.String, TlvTags.ID_PROD_FFL_RESTRICTION_CODE)] public string RestrictionCode { get; set; }
    }
}