using System;
using System.Runtime.Serialization;

namespace Metricon.WCF.MetriconRetailSystem.Contracts
{
    [DataContract]
    public class SharepointDocumentType
    {
        [DataMember]
        public int DocumentTypeID { get; set; }

        [DataMember]
        public string DocumentType { get; set; }

        [DataMember]
        public string DocumentCategory { get; set; }
    }
}
