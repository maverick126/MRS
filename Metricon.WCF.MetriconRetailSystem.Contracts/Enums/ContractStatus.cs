using System.Runtime.Serialization;

namespace Metricon.WCF.MetriconRetailSystem.Contracts
{
    [DataContract]
    public enum ContractStatus
    {
        [EnumMember]
        Pending = 1,
        [EnumMember]
        WorkInProgress = 2,
        [EnumMember]
        OnHold = 3,
        [EnumMember]
        Cancelled = 4
    }
}
