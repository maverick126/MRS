using System.Runtime.Serialization;

namespace Metricon.WCF.MetriconRetailSystem.Contracts
{
    [DataContract]    
    public enum EstimateRevisionStatus
    {
        [EnumMember]
        WorkInProgress = 1,
        [EnumMember]
        Accepted = 2,
        [EnumMember]
        Rejected = 3,
        [EnumMember]
        OnHold = 4,
        [EnumMember]
        Cancelled = 5
    }
}
