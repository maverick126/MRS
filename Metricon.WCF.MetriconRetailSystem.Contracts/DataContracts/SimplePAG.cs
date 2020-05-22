using System;
using System.Runtime.Serialization;


namespace Metricon.WCF.MetriconRetailSystem.Contracts
{
    [DataContract]
    public class SimplePAG
    {
        private string _areaname;
        private string _groupname;
        private string _productid;
        private int _areaid;
        private int _groupid;
        private int _productareagroupid;
        private int _standardinclusionsid;
        private int _optionid;
        private bool _selected;
        private string _productname;
        private bool _isSiteWork;
        private bool _isStandardOption;
        private string _displayat;
        private bool _derivedcost;
        private decimal _costbtpexcgst;
        private decimal _costdbcexcgst;
        private string _productdescription;
        private string _additionalinformation;
        private string _extradescription;
        private string _internaldescription;
        private string _uom;
        private decimal _quantity;
        private decimal _price;
        private decimal _totalprice;
        private string _pricedisplaycode;
        private int _pricedisplaycodeid;
        private bool _itemallowtochangedisplaycode;
        private bool _itemallowtochangequantity;
        private bool _itemallowtochangeprice;
        private bool _itemallowtochangedescription;
        private string _margin;
        private bool _isAccepted;
        private bool _isInLieuExist;
        private bool _isInLieuExistStandard;
        private bool _isInLieuExistPromo;
        private decimal _priceStandard;
        private decimal _pricePromo;
        private decimal _costStandardExcGST;
        private decimal _costPromoExcGST;
        private bool _priceStandardSelected;
        private bool _pricePromoSelected;

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

        [DataMember]
        public string ProductID
        {
            get { return this._productid; }
            set
            {
                if (value != this._productid)
                {
                    this._productid = value;
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
        public int ProductAreaGroupID
        {
            get { return this._productareagroupid; }
            set
            {
                if (value != this._productareagroupid)
                {
                    this._productareagroupid = value;
                }
            }
        }

        [DataMember]
        public int StandardInclusionsID
        {
            get { return this._standardinclusionsid; }
            set
            {
                if (value != this._standardinclusionsid)
                {
                    this._standardinclusionsid = value;
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

        [DataMember]
        public bool IsSiteWork
        {
            get { return this._isSiteWork; }
            set
            {
                if (value != this._isSiteWork)
                {
                    this._isSiteWork = value;
                }
            }
        }

        [DataMember]
        public bool IsStandardOption
        {
            get { return this._isStandardOption; }
            set
            {
                if (value != this._isStandardOption)
                {
                    this._isStandardOption = value;
                }
            }
        }

        [DataMember]
        public string DisplayAt
        {
            get { return this._displayat; }
            set
            {
                if (value != this._displayat)
                {
                    this._displayat = value;
                }
            }
        }

        [DataMember]
        public bool DerivedCost
        {
            get { return this._derivedcost; }
            set
            {
                if (value != this._derivedcost)
                {
                    this._derivedcost = value;
                }
            }
        }

        [DataMember]
        public decimal CostBTPExcGST
        {
            get { return this._costbtpexcgst; }
            set
            {
                if (value != this._costbtpexcgst)
                {
                    this._costbtpexcgst = value;
                }
            }
        }

        [DataMember]
        public decimal CostDBCExcGST
        {
            get { return this._costdbcexcgst; }
            set
            {
                if (value != this._costdbcexcgst)
                {
                    this._costdbcexcgst = value;
                }
            }
        }
        [DataMember]
        public string ProductDescription
        {
            get { return this._productdescription; }
            set
            {
                if (value != this._productdescription)
                {
                    this._productdescription = value;
                }
            }
        }

        [DataMember]
        public string AdditionalNotes
        {
            get { return this._additionalinformation; }
            set
            {
                if (value != this._additionalinformation)
                {
                    this._additionalinformation = value;
                }
            }
        }

        [DataMember]
        public string ExtraDescription
        {
            get { return this._extradescription; }
            set
            {
                if (value != this._extradescription)
                {
                    this._extradescription = value;
                }
            }
        }

        [DataMember]
        public string InternalDescription
        {
            get { return this._internaldescription; }
            set
            {
                if (value != this._internaldescription)
                {
                    this._internaldescription = value;
                }
            }
        }

        [DataMember]
        public string Uom
        {
            get { return this._uom; }
            set
            {
                if (value != this._uom)
                {
                    this._uom = value;
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
        public decimal Price
        {
            get { return this._price; }
            set
            {
                if (value != this._price)
                {
                    this._price = value;
                }
            }
        }

        [DataMember]
        public decimal TotalPrice
        {
            get { return this._totalprice; }
            set
            {
                if (value != this._totalprice)
                {
                    this._totalprice = value;
                }
            }
        }

        [DataMember]
        public string PriceDisplayCodeDesc
        {
            get { return this._pricedisplaycode; }
            set
            {
                if (value != this._pricedisplaycode)
                {
                    this._pricedisplaycode = value;
                }
            }
        }

        [DataMember]
        public int PriceDisplayCodeId
        {
            get { return this._pricedisplaycodeid; }
            set
            {
                if (value != this._pricedisplaycodeid)
                {
                    this._pricedisplaycodeid = value;
                }
            }
        }

        [DataMember]
        public bool ItemAllowToChangeDisplayCode
        {
            get { return this._itemallowtochangedisplaycode; }
            set
            {
                if (value != this._itemallowtochangedisplaycode)
                {
                    this._itemallowtochangedisplaycode = value;
                }
            }
        }

        [DataMember]
        public bool ItemAllowToChangeQuantity 
        {
            get { return this._itemallowtochangequantity; }
            set
            {
                if (value != this._itemallowtochangequantity)
                {
                    this._itemallowtochangequantity = value;
                }
            }
        }

        [DataMember]
        public bool ItemAllowToChangePrice 
        {
            get { return this._itemallowtochangeprice; }
            set
            {
                if (value != this._itemallowtochangeprice)
                {
                    this._itemallowtochangeprice = value;
                }
            }
        }

        [DataMember]
        public bool ItemAllowToChangeDescription 
        {
            get { return this._itemallowtochangedescription; }
            set
            {
                if (value != this._itemallowtochangedescription)
                {
                    this._itemallowtochangedescription = value;
                }
            }
        }

        [DataMember]
        public string Margin
        {
            get { return this._margin; }
            set
            {
                if (value != this._margin)
                {
                    this._margin = value;
                }
            }
        }

        [DataMember]
        public bool IsAccepted
        {
            get { return this._isAccepted; }
            set
            {
                if (value != this._isAccepted)
                {
                    this._isAccepted = value;
                }
            }
        }


        [DataMember]
        public bool IsInLieuExist
        {
            get { return this._isInLieuExist; }
            set
            {
                if (value != this._isInLieuExist)
                {
                    this._isInLieuExist = value;
                }
            }
        }

        [DataMember]
        public bool IsInLieuExistStandard
        {
            get { return this._isInLieuExistStandard; }
            set
            {
                if (value != this._isInLieuExistStandard)
                {
                    this._isInLieuExistStandard = value;
                }
            }
        }

        [DataMember]
        public bool IsInLieuExistPromo
        {
            get { return this._isInLieuExistPromo; }
            set
            {
                if (value != this._isInLieuExistPromo)
                {
                    this._isInLieuExistPromo = value;
                }
            }
        }

        [DataMember]
        public decimal PriceStandard
        {
            get { return this._priceStandard; }
            set
            {
                if (value != this._priceStandard)
                {
                    this._priceStandard = value;
                }
            }
        }

        [DataMember]
        public decimal PricePromo
        {
            get { return this._pricePromo; }
            set
            {
                if (value != this._pricePromo)
                {
                    this._pricePromo = value;
                }
            }
        }

        [DataMember]
        public decimal CostStandardExcGST
        {
            get { return this._costStandardExcGST; }
            set
            {
                if (value != this._costStandardExcGST)
                {
                    this._costStandardExcGST = value;
                }
            }
        }

        [DataMember]
        public decimal CostPromoExcGST
        {
            get { return this._costPromoExcGST; }
            set
            {
                if (value != this._costPromoExcGST)
                {
                    this._costPromoExcGST = value;
                }
            }
        }

        [DataMember]
        public bool PriceStandardSelected
        {
            get { return this._priceStandardSelected; }
            set
            {
                if (value != this._priceStandardSelected)
                {
                    this._priceStandardSelected = value;
                }
            }
        }

        [DataMember]
        public bool PricePromoSelected
        {
            get { return this._pricePromoSelected; }
            set
            {
                if (value != this._pricePromoSelected)
                {
                    this._pricePromoSelected = value;
                }
            }
        }
    }
}
