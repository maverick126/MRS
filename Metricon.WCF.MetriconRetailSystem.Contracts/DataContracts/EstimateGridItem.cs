using System;
using System.Runtime.Serialization;

namespace Metricon.WCF.MetriconRetailSystem.Contracts
{
    [DataContract]
    public class EstimateGridItem : EstimateBase
    {
        [DataMember]
        public string RevisionDetails { get; set; }

        [DataMember]
        public int PreviousRevisionId { get; set; }

        [DataMember]
        public Boolean PreviewVisible { get; set; }

        [DataMember]
        public Boolean HistoryVisible { get; set; }

        [DataMember]
        public Boolean AssignToMeVisible { get; set; }

        [DataMember]
        public Boolean AssignVisible { get; set; }

        [DataMember]
        public Boolean EditVisible { get; set; }

        [DataMember]
        public Boolean AcceptVisible { get; set; }

        [DataMember]
        public Boolean RejectVisible { get; set; }

        [DataMember]
        public Boolean EffectiveDateVisible { get; set; }

        [DataMember]
        public Boolean DueDateVisible { get; set; }

        [DataMember]
        public Boolean DifficultyVisible { get; set; }

        [DataMember]
        public Boolean OnHoldVisible { get; set; }

        [DataMember]
        public Boolean CancelVisible { get; set; }

        [DataMember]
        public Boolean ActivateVisible { get; set; }

        [DataMember]
        public Boolean CommentsVisible { get; set; }

        [DataMember]
        public Boolean AllowToResetCurrentMilestone { get; set; }

        [DataMember]
        public Boolean AllowUndoSetContract { get; set; }

        [DataMember]
        public Boolean AllowUndoCurrentRevision { get; set; }
    }
}
