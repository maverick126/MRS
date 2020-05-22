using System;
using System.Runtime.Serialization;

namespace Metricon.WCF.MetriconRetailSystem.Contracts
{
    [DataContract]
    public class PromotionPAG
    {
       private string _areaname;
        private string _groupname;
        private string _productname;
        private decimal _quantity;
        private decimal _sellprice;
        private bool _issiteworkitem; 
        private bool _isinmultiplepromotion;
        private string _multiplepromotionname;
        private string _iconimage;
        private int _revisiondetailsid;
        private bool _selected; 

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
        public int RevisionDetailsID
        {
            get { return this._revisiondetailsid; }
            set
            {
                if (value != this._revisiondetailsid)
                {
                    this._revisiondetailsid = value;
                }
            }
        }
        [DataMember]
        public bool IsInMultiplePromotion
        {
            get { return this._isinmultiplepromotion; }
            set
            {
                if (value != this._isinmultiplepromotion)
                {
                    this._isinmultiplepromotion = value;
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
        public string MultiplePromotionName
        {
            get { return this._multiplepromotionname; }
            set
            {
                if (value != this._multiplepromotionname)
                {
                    this._multiplepromotionname = value;
                }
            }
        }
        [DataMember]
        public string IconImage
        {
            get { return this._iconimage; }
            set
            {
                if (value != this._iconimage)
                {
                    this._iconimage = value;
                }
            }
        }
        [DataMember]
        public bool Selected
        {
            get { return this._selected; }
            set
            {
                if (value != this._selected)
                {
                    this._selected = value;
                }
            }
        }
    }
}
