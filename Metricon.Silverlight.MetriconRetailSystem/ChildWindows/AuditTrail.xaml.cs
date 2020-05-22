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
    public partial class AuditTrail : ChildWindow
    {
        public AuditTrail(object dataContext)
        {
            InitializeComponent();

            BusyIndicator1.IsBusy = true;

            EstimateGridItem item = (EstimateGridItem)dataContext;

            txtCustomerName.Text = item.CustomerName;
            txtContractNumber.Text = item.ContractNumber.ToString();
            txtCustomerNumber.Text = item.CustomerNumber.ToString();
            txtEstimateNumber.Text = item.EstimateId.ToString();
            txtJobFlow.Text = item.JobFlowType;
            txtContractType.Text = item.ContractType;

            this.Title = String.Format(this.Title.ToString(), item.EstimateId.ToString());

            RetailSystemClient mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            mrsClient.GetAuditTrailCompleted += new EventHandler<GetAuditTrailCompletedEventArgs>(mrsClient_GetAuditTrailCompleted);
            mrsClient.GetAuditTrailAsync(item.EstimateId);
        }

        void mrsClient_GetAuditTrailCompleted(object sender, GetAuditTrailCompletedEventArgs e)
        {
            if (e.Error == null)
                AuditTrailGrid.ItemsSource = e.Result;
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetAuditTrailCompleted");

            BusyIndicator1.IsBusy = false;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            RadWindow window = this.ParentOfType<RadWindow>();
            if (window != null)
                window.Close();
        }
    }
}

