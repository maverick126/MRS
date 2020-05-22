using System.Runtime.Serialization;

namespace Metricon.WCF.MetriconRetailSystem.Contracts
{
    [DataContract]
    public enum Priority
    {
        [EnumMember]
        Low = 1,
        [EnumMember]
        Medium = 2,
        [EnumMember]
        High = 3,
        [EnumMember]
        Urgent = 4
    }
}
