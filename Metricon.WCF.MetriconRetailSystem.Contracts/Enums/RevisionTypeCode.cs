using System.Runtime.Serialization;

namespace Metricon.WCF.MetriconRetailSystem.Contracts
{
    [DataContract]
    public enum RevisionTypeCode
    {
        [EnumMember]
        SC = 1,
        [EnumMember]
        SA = 2,
        [EnumMember]
        DF = 3,
        [EnumMember]
        SE = 4,
        [EnumMember]
        CSC = 5

    }
}
