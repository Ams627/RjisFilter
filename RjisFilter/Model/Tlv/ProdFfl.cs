using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TlvSerialise;

namespace RjisFilter.Model
{
    public class ProdFfl : TlvSerialisable
    {
        [Tlv(TlvTypes.UInt, TlvTags.ID_PROD_FFL_NUMERO_VERSION)] public int Version { get; set; }
        [Tlv(TlvTypes.String, TlvTags.ID_PROD_FFL_IAP)] public string TvmId { get; set; } = "TVM";
        [Tlv(TlvTypes.Array, TlvTags.ID_PROD_FFL_PRODUCT)] public List<FflProduct> FflProducts { get; set; }
        public ProdFfl()
        {
            FflProducts = new List<FflProduct>();
        }
    }
}
