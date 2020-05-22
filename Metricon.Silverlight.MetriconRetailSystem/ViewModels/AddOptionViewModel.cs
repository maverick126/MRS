using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Telerik.Windows.Controls;
using Metricon.Silverlight.MetriconRetailSystem.Command;
using Metricon.Silverlight.MetriconRetailSystem.MRSService;
using Metricon.Silverlight.MetriconRetailSystem.ChildWindows;

namespace Metricon.Silverlight.MetriconRetailSystem.ViewModels
{
    public class AddOptionViewModel : ViewModelBase
    {
        private ObservableCollection<SimplePAG> _list = new ObservableCollection<SimplePAG>();
        private string opid;
        private string revid;
        //private string stdinclid;
        private int selectedTab;
        public ObservableCollection<NonStandardGroup> _estimatenonstandardgroup = new ObservableCollection<NonStandardGroup>();
        public ObservableCollection<PriceDisplayCode> EstimateNonStandardPriceDisplayCode { get; set; }
        private RetailSystemClient mrsClient;

        public AddOptionViewModel(string optionid, string revisionid, int currentTab)
        {
            opid = optionid;
            revid = revisionid;
            //stdinclid = standardinclusionid;
            selectedTab = currentTab;


            if (!IsDesignTime)
            {
                mrsClient = new RetailSystemClient();
                mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

                GetItemList();

                GetPriceDisplayCodes();
            }

        }

        public void GetItemList()
        {

            if (_list.Count == 0)
            {
                mrsClient.GetRelevantPAGFromOnePAGCompleted += new EventHandler<GetRelevantPAGFromOnePAGCompletedEventArgs>(mrsClient_GetRelevantPAGFromOnePAGCompleted);
                mrsClient.GetRelevantPAGFromOnePAGAsync(opid, revid);
            }
            else
            {
                mrsClient_GetRelevantPAGFromOnePAGCompleted(null, null);
            }

        }
        
        void mrsClient_GetRelevantPAGFromOnePAGCompleted(object sender, GetRelevantPAGFromOnePAGCompletedEventArgs e)
        {
            if (e != null)
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        foreach (var p in e.Result)
                        {
                            p.TotalPrice = p.Quantity * p.Price;
                            //Only display options that are available in the same tab
                            switch (selectedTab)
                            {
                                case 0:
                                    if (p.IsSiteWork && p.IsStandardOption)
                                    {
                                        _list.Add(p);
                                    }
                                    break;
                                case 1:
                                    if (p.IsStandardOption && !p.IsSiteWork && !p.AreaName.ToUpper().Contains("NON STANDARD REQUEST"))
                                    {
                                        _list.Add(p);
                                    }
                                    break;
                                case 2:
                                    if (p.IsStandardOption && p.AreaName.ToUpper().Contains("NON STANDARD REQUEST"))
                                    {
                                        _list.Add(p);
                                    }
                                    break;
                                //case 3:
                                //    if (!p.IsStandardOption)
                                //    {
                                //        _list.Add(p);
                                //    }
                                //    break;
                                case 3:
                                    _list.Add(p);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
                else
                    ExceptionHandler.PopUpErrorMessage(e.Error, "GetRelevantPAGFromOnePAGCompleted");
            }
        }

        public void GetPriceDisplayCodes()
        {
            mrsClient.GetPriceDisplayCodesCompleted += new EventHandler<GetPriceDisplayCodesCompletedEventArgs>(mrsClient_GetPriceDisplayCodesCompleted);
            mrsClient.GetPriceDisplayCodesAsync();
        }

        void mrsClient_GetPriceDisplayCodesCompleted(object sender, GetPriceDisplayCodesCompletedEventArgs e)
        {
            EstimateNonStandardPriceDisplayCode = new ObservableCollection<PriceDisplayCode>();
            if (e.Error == null)
            {
                this.EstimateNonStandardPriceDisplayCode = e.Result;
                mrsClient.GetPriceDisplayCodesCompleted -= new EventHandler<GetPriceDisplayCodesCompletedEventArgs>(mrsClient_GetPriceDisplayCodesCompleted);
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetPriceDisplayCodesCompleted");
        }

        public ObservableCollection<SimplePAG> OptionList
        {
            get
            {
                return _list;
            }

            set
            {
                if (_list != value)
                {
                    _list = value;
                    OnPropertyChanged("OptionList");
                }
            }
        }

        public ObservableCollection<NonStandardGroup> EstimateNonStandardGroup
        {
            get
            {
                return _estimatenonstandardgroup;
            }

            set
            {
                if (_estimatenonstandardgroup != value)
                {
                    _estimatenonstandardgroup = value;
                    OnPropertyChanged("EstimateNonStandardGroup");
                }
            }
        }

    }
}
