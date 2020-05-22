using System;
using System.Collections.Generic;

using System.Runtime.Serialization;

namespace Metricon.WCF.MetriconRetailSystem.Contracts
{
    [DataContract]
    public class ProductImage
    {
        [DataMember]
        public byte[] image { get; set; }

        [DataMember]
        public string suppliername { get; set; }

        [DataMember]
        public string imagename { get; set; }

        [DataMember]
        public int imageID { get; set; }
    }
}
