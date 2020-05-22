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
using Metricon.Silverlight.MetriconRetailSystem.ChildWindows;
using Telerik.Windows.Controls;

namespace Metricon.Silverlight.MetriconRetailSystem
{
    public partial class EstimateListGridViewDetails : UserControl
    {
        public EstimateListGridViewDetails()
        {

            InitializeComponent();

            
        }

        public void PopulateContent()
        {
            BusyIndicator1.IsBusy = true;

            RetailSystemClient mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            EstimateGridItem item = this.DataContext as EstimateGridItem;
            if (item.RecordType == "EstimateHeader" )
            {
                mrsClient.GetEstimateHeaderCompleted += new EventHandler<GetEstimateHeaderCompletedEventArgs>(mrsClient_GetEstimateHeaderCompleted);
                mrsClient.GetEstimateHeaderAsync(item.RecordId);
            }
            else if (item.RecordType == "Queue")
            {
                mrsClient.GetLatestEstimateRevisionIdCompleted += new EventHandler<GetLatestEstimateRevisionIdCompletedEventArgs>(mrsClient_GetLatestEstimateRevisionIdCompleted);
                mrsClient.GetLatestEstimateRevisionIdAsync(item.EstimateId);
            }
            else
            {
                MessageBox.Show("SQS Estimates cannot be viewed here");
                BusyIndicator1.IsBusy = false;
            }

        }

        void mrsClient_GetLatestEstimateRevisionIdCompleted(object sender, GetLatestEstimateRevisionIdCompletedEventArgs e)
        {
            int estimateRevisionId = (int)e.Result;          
            RetailSystemClient mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            mrsClient.GetEstimateHeaderCompleted += new EventHandler<GetEstimateHeaderCompletedEventArgs>(mrsClient_GetEstimateHeaderCompleted);
            mrsClient.GetEstimateHeaderAsync(estimateRevisionId);
        }

        void mrsClient_GetEstimateHeaderCompleted(object sender, GetEstimateHeaderCompletedEventArgs e)
        {
            EstimateHeader header = (EstimateHeader)e.Result;
            txtEffectiveDate.Text = header.EffectiveDate.ToString("dd/MM/yyyy");
            txtHomePrice.Text = header.HomePrice.ToString("c");
            //txtPromotion.Text = header.PromotionName;
            txtPromotionValue.Text = header.PromotionValue.ToString("c");
            txtRegion.Text = header.Region;
            txtTotalPrice.Text = header.TotalPrice.ToString("c");
            txtUpgradeValue.Text = header.UpgradeValue.ToString("c");
            txtComments.Text = header.Comments;
            txtFullSiteAddress.Text = GetLotAddress(header.LotNumber, header.StreetNumber, header.StreetAddress, header.Suburb, header.State, header.PostCode);
            txtHomeName.Text = header.HomeName;
            txtMargin.Text = header.MarginString;
            //txtTotalCost.Text = header.TotalCostBTP.ToString("c");
            txtTotalCost.Text = header.TotalCostDBC.ToString("c");
            txtTotalRetail.Text =header.TotalPriceExc.ToString("c");
            txtTotalMargin.Text = header.TotalMargin.ToString("c");
            txtEstimateNumber.Text=header.EstimateId.ToString();
            txtDifficulty.Text = header.DifficultyRating;
            if (header.DueDate == DateTime.MinValue)
                txtDueDate.Text = "";
            else
                txtDueDate.Text = header.DueDate.ToString("dd/MM/yyyy");            
            if (header.AppointmentDate == DateTime.MinValue)
                txtAppointment.Text = "";
            else
                txtAppointment.Text = header.AppointmentDate.ToString("dd/MM/yyyy");            
            if (header.DepositDate == DateTime.MinValue)
                txtDepositDate.Text = "";
            else
                txtDepositDate.Text = header.DepositDate.ToString("dd/MM/yyyy");
            txtHomeAndLandPackage.Text = header.HomeAndLandPackage;

            if (!((App)App.Current).CurrentRoleAccessModule.AccessMarginModule)
            {
                txtTotalRetail.Visibility = Visibility.Collapsed;
                lblTotalretailexc.Visibility = Visibility.Collapsed;

                txtTotalCost.Visibility = Visibility.Collapsed;
                lbltotalcost.Visibility = Visibility.Collapsed;
                
                txtMargin.Visibility = Visibility.Collapsed;
                lblTotalMarginpercentage.Visibility = Visibility.Collapsed;

                txtTotalMargin.Visibility = Visibility.Collapsed;
                lblTotalMargin.Visibility = Visibility.Collapsed;
            } 

            BusyIndicator1.IsBusy = false;
        }

        private void btnHistory_Click(object sender, RoutedEventArgs e)
        {
            RadWindow win = new RadWindow();
            EstimateHistory historyDlg = new EstimateHistory(this.DataContext);
            win.Header = "Estimate History";
            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
            win.Content = historyDlg;
            win.ShowDialog();
        }

        private void btnCompare_Click(object sender, RoutedEventArgs e)
        {
            RadWindow win = new RadWindow();
            CompareEstimates compareDlg = new CompareEstimates(this.DataContext);
            win.Header = "Compare Estimates";
            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
            win.Content = compareDlg;
            win.ResizeMode = ResizeMode.NoResize;
            win.ShowDialog();
        }

        private string GetLotAddress(string lotNumber, string streetNumber, string streetAddress, string suburb, string state, string postCode)
        {
            string lotAddress = "";

            if (!string.IsNullOrWhiteSpace(lotNumber))
                lotAddress = "Lot " + lotNumber;
            if (!string.IsNullOrWhiteSpace(streetNumber))
            {
                if (!string.IsNullOrWhiteSpace(lotAddress))
                    lotAddress += ", ";
                lotAddress += streetNumber + " ";
            }
            if (!string.IsNullOrWhiteSpace(streetAddress))
            {
                if (!string.IsNullOrWhiteSpace(streetNumber))
                    lotAddress += " ";
                else if (!string.IsNullOrWhiteSpace(lotAddress))
                    lotAddress += ", ";
                lotAddress += streetAddress;
            }
            if (!string.IsNullOrWhiteSpace(suburb))
            {
                if (!string.IsNullOrWhiteSpace(lotAddress))
                    lotAddress += ", ";
                lotAddress += suburb;
            }
            if (!string.IsNullOrWhiteSpace(state))
            {
                if (!string.IsNullOrWhiteSpace(lotAddress))
                    lotAddress += ", ";
                lotAddress += state;
            }
            if (!string.IsNullOrWhiteSpace(postCode))
            {
                if (!string.IsNullOrWhiteSpace(lotAddress))
                    lotAddress += ", ";
                lotAddress += postCode;
            }
            return lotAddress.ToUpper();
        }

    }
}
