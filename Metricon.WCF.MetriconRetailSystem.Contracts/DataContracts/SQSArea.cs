using System;
using System.Runtime.Serialization;

namespace Metricon.WCF.MetriconRetailSystem.Contracts
{
    [DataContract]
    public class SQSArea
    {
        private int _areaid;
        private string _areaname;

        [DataMember]
        public int AreaID
        {
            get { return this._areaid; }
            set
            {
                if (value != this._areaid)
                {
                    this._areaid = value;
                }
            }
        }
        [DataMember]
        public string AreaName
        {
            get { return this._areaname; }
            set
            {
                if (value != this._areaname)
                {
                    this._areaname = value;
                }
            }
        }

    }
}
