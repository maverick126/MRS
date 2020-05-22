using System;
using System.Runtime.Serialization;

namespace Metricon.WCF.MetriconRetailSystem.Contracts
{
    [DataContract]
    public class EstimateComments
    {
        private string _comments;

        public EstimateComments(string pcomments)
        {
            _comments = pcomments;

        }
        [DataMember]
        public string Comments
        {
            get { return this._comments; }
            set
            {
                if (value != this._comments)
                {
                    this._comments = value;
                }
            }
        }
    }
}
