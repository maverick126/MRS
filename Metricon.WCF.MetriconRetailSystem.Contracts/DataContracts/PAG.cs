using System;
using System.Runtime.Serialization;

namespace Metricon.WCF.MetriconRetailSystem.Contracts
{
    [DataContract]
    public class PAG
    {
        private string _areaname;
        private string _groupname;
        private string _productname;
        private decimal _quantity;
        private decimal _sellprice;
        private int _optionid;
        private string _extradesc;
        private string _internaldesc;
        private bool _promotionproduct;
        private bool _standardoption;
        private bool _issiteworkitem; 
        private bool _ismasterpromotion;


        public PAG(string pareaname, string pgroupname, string pproductname, decimal pqty, decimal psellprice, int optionid, string pextradesc, string pinternaldesc, bool ppromtionproduct, bool pstandardoption, bool issiteworkitem, bool ismasterpromotion)
        {
            _areaname = pareaname;
            _groupname = pgroupname;
            _productname = pproductname;
            _quantity = pqty;
            _sellprice = psellprice;
            _optionid = optionid;
            _extradesc = pextradesc;
            _internaldesc = pinternaldesc;
            _promotionproduct = ppromtionproduct;
            _standardoption = pstandardoption;
            _issiteworkitem = issiteworkitem;
            _ismasterpromotion = ismasterpromotion;
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
        [DataMember]
        public string ProductName
        {
            get { return this._productname; }
            set
            {
                if (value != this._productname)
                {
                    this._productname = value;
                }
            }
        }
        [DataMember]
        public decimal Quantity
        {
            get { return this._quantity; }
            set
            {
                if (value != this._quantity)
                {
                    this._quantity = value;
                }
            }
        }
        [DataMember]
        public decimal SellPrice
        {
            get { return this._sellprice; }
            set
            {
                if (value != this._sellprice)
                {
                    this._sellprice = value;
                }
            }
        }

        [DataMember]
        public int HomeDisplayOptionID
        {
            get { return this._optionid; }
            set
            {
                if (value != this._optionid)
                {
                    this._optionid = value;
                }
            }
        }

        [DataMember]
        public string ExtraDescription
        {
            get { return this._extradesc; }
            set
            {
                if (value != this._extradesc)
                {
                    this._extradesc = value;
                }
            }
        }
        [DataMember]
        public string InternalDescription
        {
            get { return this._internaldesc; }
            set
            {
                if (value != this._internaldesc)
                {
                    this._internaldesc = value;
                }
            }
        }

        [DataMember]
        public bool PromotionProduct
        {
            get { return this._promotionproduct; }
            set
            {
                if (value != this._promotionproduct)
                {
                    this._promotionproduct = value;
                }
            }
        }

        [DataMember]
        public bool StandardOption
        {
            get { return this._standardoption; }
            set
            {
                if (value != this._standardoption)
                {
                    this._standardoption = value;
                }
            }
        }

        [DataMember]
        public bool IsSiteworkItem
        {
            get { return this._issiteworkitem; }
            set
            {
                if (value != this._issiteworkitem)
                {
                    this._issiteworkitem = value;
                }
            }
        }

        [DataMember]
        public bool IsMasterPromotion
        {
            get { return this._ismasterpromotion; }
            set
            {
                if (value != this._ismasterpromotion)
                {
                    this._ismasterpromotion = value;
                }
            }
        }
    }
}
