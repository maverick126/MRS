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
    public partial class AddProductInLieuOfStandardPromotion : ChildWindow
    {
        private int estimateRevisionId;
        private int originateoptionid;

        public AddProductInLieuOfStandardPromotion(int revisionId)
        {
            InitializeComponent();

            BusyIndicator1.IsBusy = true;
            HasCloseButton = false;

            estimateRevisionId = revisionId;

            RetailSystemClient mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            mrsClient.GetInLieuStandardPromotionItemsCompleted += new EventHandler<GetInLieuStandardPromotionItemsCompletedEventArgs>(mrsClient_GetInLieuStandardPromotionItemsCompleted);
            mrsClient.GetInLieuStandardPromotionItemsAsync(estimateRevisionId, originateoptionid);
        }

        void mrsClient_GetInLieuStandardPromotionItemsCompleted(object sender, GetInLieuStandardPromotionItemsCompletedEventArgs e)
        {
            if (e.Error == null)
                InLieuStandardPromotionItemGrid.ItemsSource = e.Result;
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetInLieuStandardPromotionItemsCompleted");

            BusyIndicator1.IsBusy = false;           
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            RadWindow window = this.ParentOfType<RadWindow>();
            if (window != null)
            {
                window.DialogResult = true;
                window.Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            RadWindow window = this.ParentOfType<RadWindow>();
            if (window != null)
            {
                window.DialogResult = false;
                window.Close();
            }
        }

    }
}

