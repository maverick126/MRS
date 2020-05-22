using System;
using System.Runtime.Serialization;

namespace Metricon.WCF.MetriconRetailSystem.Contracts
{
    [DataContract]
    public class SharepointDoc 
    {
        [DataMember]
        public string FileID { get; set; }

        [DataMember]
        public string DocumentName { get; set; }

        [DataMember]
        public string DocumentCategory { get; set; }

        [DataMember]
        public string DocumentType { get; set; }

        [DataMember]
        public string DocumentGroup { get; set; }

        [DataMember]
        public string FileURI { get; set; }


    }
}
