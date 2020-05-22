using System;
using System.Runtime.Serialization;

namespace Metricon.WCF.MetriconRetailSystem.Contracts
{
    [DataContract]
    public class EstimateBase
    {
        [DataMember]
        public int EstimateId { get; set; }

        [DataMember]
        public int RecordId { get; set; } // Can be either EstimateRevisionId or QueueId

        [DataMember]
        public string RecordType { get; set; } // Can be either EstimateHeader or Queue

        [DataMember]
        public string HomeName { get; set; }

        [DataMember]
        public string CustomerName { get; set; }

        [DataMember]
        public int CustomerNumber { get; set; }

        [DataMember]
        public int ContractNumber { get; set; }

        [DataMember]
        public string SalesConsultantName { get; set; }

        [DataMember]
        public DateTime CreatedOn { get; set; }

        [DataMember]
        public DateTime DueDate { get; set; }

        [DataMember]
        public DateTime AppointmentDate { get; set; }

        [DataMember]
        public string DifficultyRating { get; set; }

        [DataMember]
        public int DifficultyRatingId { get; set; }

        [DataMember]
        public int RevisionNumber { get; set; }

        [DataMember]
        public int RevisionTypeId { get; set; }

        [DataMember]
        public string RevisionTypeCode { get; set; }

        [DataMember]
        public string OwnerName { get; set; }

        [DataMember]
        public int OwnerId { get; set; }

        [DataMember]
        public string StatusName { get; set; }

        [DataMember]
        public int StatusId { get; set; }

        [DataMember]
        public string ContractStatusName { get; set; }

        [DataMember]
        public string BusinessUnit { get; set; }

        [DataMember]
        public string Opportunityid { get; set; } // this is oppid for each estimate. will be used when reject/accept with minor issues to create activity in CRM 

        [DataMember]
        public string Accountid { get; set; }

        [DataMember]
        public string Phone { get; set; } // this is mobile or landline phone for each customer. will be used when reject/accept with minor issues to create activity in CRM 

        [DataMember]
        public string ContractID { get; set; } // this is new_contractid for each estimate. will be used when reject/accept with minor issues to create task in CRM 

        [DataMember]
        public bool AllowToAddNSR { get; set; }

        [DataMember]
        public bool ValidateAcceptedFlag { get; set; }

        [DataMember]
        public bool ValidateStandardInclusion { get; set; }

        [DataMember]
        public bool ReadOnly { get; set; }

        [DataMember]
        public bool AllowToAcceptItem { get; set; }

        [DataMember]
        public bool AllowToViewStudioMTab { get; set; }

        [DataMember]
        public bool AllowToViewStudioMDocuSign { get; set; }

        [DataMember]
        public string ContractType { get; set; }

        [DataMember]
        public string JobFlowType { get; set; }

        [DataMember]
        public string MRSGroup { get; set; }

        [DataMember]
        public int RegionID { get; set; }

        [DataMember]
        public string JobLocation { get; set; }

        [DataMember]
        public int EditEstimateUserID { get; set; }

        [DataMember]
        public string EditEstimateUserName { get; set; }
    }
}
