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

using Metricon.Silverlight.MetriconRetailSystem.MRSService;
using Telerik.Windows;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace Metricon.Silverlight.MetriconRetailSystem.ChildWindows
{
    public partial class DeletedItemList : ChildWindow
    {
        private int estimateRevisionId;
        private bool estimateChanged = false;
        private bool allDeletedItemsLoaded = false;

        public DeletedItemList(int revisionId)
        {
            InitializeComponent();

            BusyIndicator1.IsBusy = true;
            HasCloseButton = false;

            estimateRevisionId = revisionId;

            RetailSystemClient mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            mrsClient.GetDeletedItemsCompleted += new EventHandler<GetDeletedItemsCompletedEventArgs>(mrsClient_GetDeletedItemsCompleted);
            mrsClient.GetDeletedItemsAsync(estimateRevisionId, RESULT_TYPE.CURRENT);
        }

        void mrsClient_GetDeletedItemsCompleted(object sender, GetDeletedItemsCompletedEventArgs e)
        {
            if (e.Error == null)
            { 
                DeletedItemGrid.ItemsSource = e.Result;
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetDeletedItemsCompleted");

            BusyIndicator1.IsBusy = false;           
        }

        void mrsClient_GetDeletedItemsAllCompleted(object sender, GetDeletedItemsCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                DeletedItemsAllGrid.ItemsSource = e.Result;
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetDeletedItemsCompleted");

            BusyIndicator1.IsBusy = false;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            RadWindow window = this.ParentOfType<RadWindow>();
            if (window != null)
            {
                window.DialogResult = estimateChanged;
                window.Close();
            }
        }

        private void btnReAdd_Click(object sender, RoutedEventArgs e)
        {
            BusyIndicator1.IsBusy = true;
            DeletedItem delItem = ((GridViewCell)((HyperlinkButton)e.OriginalSource).Parent).ParentRow.DataContext as DeletedItem;
            RetailSystemClient mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            if (delItem.IsMasterPromotion)
            {
                mrsClient.ReAddDeletedMasterPromotionEstimateItemCompleted += delegate(object o, ReAddDeletedMasterPromotionEstimateItemCompletedEventArgs es)
                {
                    if (es.Error == null)
                    {
                        if ((bool)es.Result)
                        {
                            mrsClient.GetDeletedItemsCompleted += new EventHandler<GetDeletedItemsCompletedEventArgs>(mrsClient_GetDeletedItemsCompleted);
                            mrsClient.GetDeletedItemsAsync(estimateRevisionId, RESULT_TYPE.CURRENT);
                            estimateChanged = true;
                        }
                        else
                        {
                            RadWindow.Alert("Re add deleted estimate item have failed");
                            return;
                        }
                    }
                    BusyIndicator1.IsBusy = false;
                };
                mrsClient.ReAddDeletedMasterPromotionEstimateItemAsync(estimateRevisionId, delItem.HomeDisplayOptionId, (App.Current as App).CurrentUserId);
            }
            else
            {
                mrsClient.ReAddDeletedEstimateItemCompleted += delegate(object o, ReAddDeletedEstimateItemCompletedEventArgs es)
                {
                    if (es.Error == null)
                    {
                        if ((bool)es.Result)
                        {
                            mrsClient.GetDeletedItemsCompleted += new EventHandler<GetDeletedItemsCompletedEventArgs>(mrsClient_GetDeletedItemsCompleted);
                            mrsClient.GetDeletedItemsAsync(estimateRevisionId, RESULT_TYPE.CURRENT);
                            estimateChanged = true;
                        }
                        else
                        {
                            RadWindow.Alert("Re add deleted estimate item have failed");
                            return;
                        }
                    }
                    BusyIndicator1.IsBusy = false;
                };
                mrsClient.ReAddDeletedEstimateItemAsync(delItem.RevisionId, estimateRevisionId, delItem.HomeDisplayOptionId, (App.Current as App).CurrentUserId);
            }
        }

        private void DeletedItemGrid_RowLoaded(object sender, RowLoadedEventArgs e)
        {
            HyperlinkButton hl;
            Image img;
            GridViewRow row = e.Row as GridViewRow;
            if (row != null)
            {
                DeletedItem rm = row.DataContext as DeletedItem;
                if (row != null && rm != null)
                {
                        if (((App.Current as App).CurrentAction != "EDIT") || EstimateList.revisiontypepermission.ReadOnly) 
                        {
                            foreach (GridViewCell Cell in row.Cells)
                            {
                                hl = Cell.FindChildByType<HyperlinkButton>();
                                if (hl != null && hl.Name == "btnReAdd")
                                {
                                    hl.IsEnabled = false;
                                    img = Cell.FindChildByType<Image>();
                                    img.Opacity = 0.3;
                                }
                            }
                        }
                    }
                }
        }

        private void TopPaneGroup_SelectionChanged(object sender, RadSelectionChangedEventArgs e)
        {
            if (!allDeletedItemsLoaded) {
                RetailSystemClient mrsClient = new RetailSystemClient();

                mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());
                mrsClient.GetDeletedItemsCompleted += new EventHandler<GetDeletedItemsCompletedEventArgs>(mrsClient_GetDeletedItemsAllCompleted);
                mrsClient.GetDeletedItemsAsync(estimateRevisionId, RESULT_TYPE.ALL);
                allDeletedItemsLoaded = true;
            }
        }
    }
}

