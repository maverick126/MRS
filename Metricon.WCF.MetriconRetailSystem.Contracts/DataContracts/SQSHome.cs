using System;
using System.Runtime.Serialization;

namespace Metricon.WCF.MetriconRetailSystem.Contracts
{
    [DataContract]
    public class SQSHome
    {
        private int _homeid;
        private string _homename;
        private bool _display;

        [DataMember]
        public int HomeID
        {
            get { return this._homeid; }
            set
            {
                if (value != this._homeid)
                {
                    this._homeid = value;
                }
            }
        }
        [DataMember]
        public string HomeName
        {
            get { return this._homename; }
            set
            {
                if (value != this._homename)
                {
                    this._homename = value;
                }
            }
        }

        [DataMember]
        public bool Display
        {
            get { return this._display; }
            set
            {
                if (value != this._display)
                {
                    this._display = value;
                }
            }
        }
    }
}
