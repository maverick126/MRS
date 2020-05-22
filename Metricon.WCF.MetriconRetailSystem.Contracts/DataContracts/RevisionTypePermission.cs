using System;
using System.Runtime.Serialization;

namespace Metricon.WCF.MetriconRetailSystem.Contracts
{
    [DataContract]
    public class RevisionTypePermission
    {
        [DataMember]
        public bool AllowToAddNSR { get; set; }

        [DataMember]
        public bool AllowToAcceptItem { get; set; }

        [DataMember]
        public bool ValidateAccept { get; set; }

        [DataMember]
        public bool ValidateStandardInclusion { get; set; }

        [DataMember]
        public bool ReadOnly { get; set; }

        [DataMember]
        public bool AllowToViewStudioMTab { get; set; } 
    }
}
