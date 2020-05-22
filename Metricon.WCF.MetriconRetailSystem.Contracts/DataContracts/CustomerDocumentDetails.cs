using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Metricon.WCF.MetriconRetailSystem.Contracts
{
    [DataContract]
    public class CustomerDocumentDetails
    {
        [DataMember]
        public int CustomerDocumentID { get; set; }

        [DataMember]
        public int EstimateRevisionID { get; set; }

        [DataMember]
        public string DocumentType { get; set; }

        [DataMember]
        public int? DocumentNumber { get; set; }

        [DataMember]
        public int? ExtensionDays { get; set; }

        [DataMember]
        public DateTime? SentDate { get; set; }

        [DataMember]
        public DateTime? AcceptedDate { get; set; }

        [DataMember]
        public int UserId { get; set; }

        [DataMember]
        public Boolean Active { get; set; }

        [DataMember]
        public string DocumentSummary { get; set; }
    }
}
