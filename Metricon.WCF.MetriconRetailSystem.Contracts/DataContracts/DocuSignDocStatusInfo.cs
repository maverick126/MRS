using System;
using System.Runtime.Serialization;

namespace Metricon.WCF.MetriconRetailSystem.Contracts
{
    [DataContract]
    public class DocuSignDocStatusInfo
    {
        [DataMember]
        public int integrationid { get; set; }
        [DataMember]
        public string status { get; set; }
        [DataMember]
        public string recipients { get; set; }
        [DataMember]
        public string envelopeId { get; set; }
        [DataMember]
        public string estimateid { get; set; }
        [DataMember]
        public string documenttype { get; set; }

        [DataMember]
        public string documentnumber { get; set; }

        [DataMember]
        public string versiontype { get; set; }
        [DataMember]
        public string printtype { get; set; }
        [DataMember]
        public string revisionnumber { get; set; }
        [DataMember]
        public string statusChangedDateTime { get; set; }
        [DataMember]
        public string deleted { get; set; }

        [DataMember]
        public string accountid { get; set; }

        [DataMember]
        public bool EnableSendViaDocuSign { get; set; }

        [DataMember]
        public bool EnableSignInPerson { get; set; }

        [DataMember]
        public bool EnableVoid { get; set; }

        [DataMember]
        public bool Selected { get; set; }
    }
}
