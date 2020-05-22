using System;
using System.Runtime.Serialization;

namespace Metricon.WCF.MetriconRetailSystem.Contracts
{
    [DataContract]
    public class RevisionType
    {
        [DataMember]
        public int RevisionTypeId { get; set; }

        [DataMember]
        public string RevisionTypeName { get; set; }
    }
}

