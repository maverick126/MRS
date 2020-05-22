using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using Telerik.Windows;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
using System.Collections.ObjectModel;
using Telerik.Windows.Controls;

using Metricon.Silverlight.MetriconRetailSystem.ViewModels;
using Metricon.Silverlight.MetriconRetailSystem.ChildWindows;
using Metricon.Silverlight.MetriconRetailSystem.MRSService;
using System.Xml.Linq;


namespace Metricon.Silverlight.MetriconRetailSystem.ChildWindows
{
    public partial class RemoveMasterPromotionItemWindow : ChildWindow
    {
        RetailSystemClient _mrsClient;
        EstimateDetails pag;

        public RemoveMasterPromotionItemWindow(EstimateDetails ed)
        {

            InitializeComponent();

            BusyIndicator1.IsBusy = true;

            pag = ed;
            
            _mrsClient = new RetailSystemClient();
            _mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            _mrsClient.GetExistingPromotionProductByMasterPromotionRevisionDetailsIDCompleted += new EventHandler<GetExistingPromotionProductByMasterPromotionRevisionDetailsIDCompletedEventArgs>(mrsClient_GetExistingPromotionProductByMasterPromotionRevisionDetailsIDCompleted);
            _mrsClient.GetExistingPromotionProductByMasterPromotionRevisionDetailsIDAsync(ed.EstimateRevisionDetailsId.ToString());
        }
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            RadWindow window = this.ParentOfType<RadWindow>();
            if (window != null)
                window.Close();
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedidstring = "";
            List<PromotionPAG> promotionpagneedupdate=new List<PromotionPAG>();
            foreach (var item in (ObservableCollection<PromotionPAG>)PAGGrid.ItemsSource)
            {
                if (item.Selected) // here reuse this column to hold the selection of check box
                {
                    if (selectedidstring == "")
                    {
                        selectedidstring = item.RevisionDetailsID.ToString();
                    }
                    else
                    {
                        selectedidstring = selectedidstring + "," + item.RevisionDetailsID.ToString();
                    }
                }
                else
                {
                    if (!item.IsInMultiplePromotion)
                    {
                        promotionpagneedupdate.Add(item);
                    }
                }
            }
            RemoveParameterClass p = new RemoveParameterClass();
            p.LeftPAG = promotionpagneedupdate;
            if (selectedidstring != "")// if select any product, then get the list first then remove from screen data source
            {
                _mrsClient.GetEstimateDetailsByIDStringCompleted += delegate(object o, GetEstimateDetailsByIDStringCompletedEventArgs es2)
                {
                    if (es2.Error == null)
                    {
                        if (es2.Result != null)
                        {
                            p.RemovedPAG = es2.Result.ToList();
                            _mrsClient.DeleteMasterPromotionItemCompleted += delegate(object o2, DeleteMasterPromotionItemCompletedEventArgs es)
                            {
                                if (es.Error == null)
                                {
                                    RadWindow window = this.ParentOfType<RadWindow>();
                                    window.DataContext = p;
                                    if (window != null)
                                    {
                                        window.DialogResult = true;
                                        window.Close();
                                    }
                                }
                                else
                                    ExceptionHandler.PopUpErrorMessage(es.Error, "DeleteMasterPromotionItemCompleted");
                            };

                            _mrsClient.DeleteMasterPromotionItemAsync(pag.EstimateRevisionDetailsId.ToString(), selectedidstring, (App.Current as App).CurrentUserId);

                        }
                    }
                    else
                        ExceptionHandler.PopUpErrorMessage(es2.Error, "GetEstimateDetailsByIDStringCompleted");
                };

                _mrsClient.GetEstimateDetailsByIDStringAsync(selectedidstring);
            }
            else
            {
                p.RemovedPAG = new List<EstimateDetails>();
                _mrsClient.DeleteMasterPromotionItemCompleted += delegate(object o2, DeleteMasterPromotionItemCompletedEventArgs es)
                {
                    if (es.Error == null)
                    {
                        RadWindow window = this.ParentOfType<RadWindow>();
                        window.DataContext = p;
                        if (window != null)
                        {
                            window.DialogResult = true;
                            window.Close();
                        }
                    }
                    else
                        ExceptionHandler.PopUpErrorMessage(es.Error, "DeleteMasterPromotionItemCompleted");
                };

                _mrsClient.DeleteMasterPromotionItemAsync(pag.EstimateRevisionDetailsId.ToString(), selectedidstring, (App.Current as App).CurrentUserId);

            }
        }

        void mrsClient_GetExistingPromotionProductByMasterPromotionRevisionDetailsIDCompleted(object sender, GetExistingPromotionProductByMasterPromotionRevisionDetailsIDCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                ObservableCollection<PromotionPAG> reason = e.Result;
                PAGGrid.ItemsSource = reason;
                txtpromowarning.Text = txtpromowarning.Text.Replace("$token$", e.Result.Count.ToString());
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetExistingPromotionProductByMasterPromotionRevisionDetailsIDCompleted");

            BusyIndicator1.IsBusy = false;
        }
    }

    public class RemoveParameterClass
    {
        public List<EstimateDetails> RemovedPAG { get; set; }
        public List<PromotionPAG> LeftPAG { get; set; }
    }
}

