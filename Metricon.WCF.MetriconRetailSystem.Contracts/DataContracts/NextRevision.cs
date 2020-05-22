using System;
using System.Runtime.Serialization;

namespace Metricon.WCF.MetriconRetailSystem.Contracts
{
    [DataContract]
    public class NextRevision
    {
        [DataMember]
        public int RevisionTypeId { get; set; }
      
        [DataMember]
        public string RevisionTypeName { get; set; }

        [DataMember]
        public int OwnerId { get; set; }

        [DataMember]
        public string Notes { get; set; }
    }
}