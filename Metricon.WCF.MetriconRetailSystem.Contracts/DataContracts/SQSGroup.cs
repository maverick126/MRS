using System;
using System.Runtime.Serialization;

namespace Metricon.WCF.MetriconRetailSystem.Contracts
{
    [DataContract]
    public class SQSGroup
    {
        private int _groupid;
        private string _groupname;

        [DataMember]
        public int GroupID
        {
            get { return this._groupid; }
            set
            {
                if (value != this._groupid)
                {
                    this._groupid = value;
                }
            }
        }
        [DataMember]
        public string GroupName
        {
            get { return this._groupname; }
            set
            {
                if (value != this._groupname)
                {
                    this._groupname = value;
                }
            }
        }

    }
}
