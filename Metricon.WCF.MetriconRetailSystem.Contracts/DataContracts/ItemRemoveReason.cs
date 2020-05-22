using System;
using System.Runtime.Serialization;


namespace Metricon.WCF.MetriconRetailSystem.Contracts
{
    [DataContract]
    public class ItemRemoveReason
    {
        [DataMember]
        public int RemoveReasonID { get; set; }

        [DataMember]
        public string RemoveReason { get; set; }
    }
}
