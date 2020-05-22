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

using System.Collections.ObjectModel;
using Telerik.Windows.Controls;
using Metricon.Silverlight.MetriconRetailSystem.MRSService;

namespace Metricon.Silverlight.MetriconRetailSystem.ChildWindows
{
    public partial class SetEffectiveDate : ChildWindow
    {
        private int _recordId;
        private RetailSystemClient _mrsClient;
        private DateTime _originalEffectiveDate;
        private Decimal _originalHomePrice;

        public SetEffectiveDate(object dataContext)
        {
            InitializeComponent();

            EstimateGridItem item = (EstimateGridItem)dataContext;

            _recordId = item.RecordId;
            
            this.Title = String.Format(this.Title.ToString(), item.EstimateId.ToString());

            BusyIndicator1.IsBusy = true;

            _mrsClient = new RetailSystemClient();
            _mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            //Get Estimate Header (for Original Effective Date)
            _mrsClient.GetEstimateHeaderCompleted += new EventHandler<GetEstimateHeaderCompletedEventArgs>(mrsClient_GetEstimateHeaderCompleted);
            _mrsClient.GetEstimateHeaderAsync(_recordId);
        }

        void mrsClient_GetEstimateHeaderCompleted(object sender, GetEstimateHeaderCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                EstimateHeader header = (EstimateHeader)e.Result;

                _originalEffectiveDate = header.EffectiveDate;
                _originalHomePrice = header.HomePrice;

                //Get Effective Date Options
                PopulateEffectiveDates(_recordId);
            }
            else
            {
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetEstimateHeaderCompleted");
                BusyIndicator1.IsBusy = false;
            }
        }

        public void PopulateEffectiveDates(int estimateRevisionId)
        {
            _mrsClient.GetHomePricesCompleted += new EventHandler<GetHomePricesCompletedEventArgs>(mrsClient_GetHomePricesCompleted);
            _mrsClient.GetHomePricesAsync(estimateRevisionId);
        }

        void mrsClient_GetHomePricesCompleted(object sender, GetHomePricesCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                ObservableCollection<HomePrice> prices = e.Result;
                prices.Insert(0, new HomePrice { PriceId = 0, EffectiveDateOptionName = "Select" });
                cmbEffectiveDate.ItemsSource = prices;

                cmbEffectiveDate.SelectedIndex = 0;
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetHomePricesCompleted");

            BusyIndicator1.IsBusy = false;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (cmbEffectiveDate.SelectedIndex > 0)
            {
                BusyIndicator1.IsBusy = true;
                BusyIndicator1.BusyContent = "Saving Estimate...";

                _mrsClient.UpdateEstimateEffectiveDateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(mrsClient_UpdateEstimateEffectiveDateCompleted);
                _mrsClient.UpdateEstimateEffectiveDateAsync(_recordId,
                    Convert.ToInt32(cmbEffectiveDate.SelectedValue),
                    (App.Current as App).CurrentUserId);
            }
            else
            {
                DialogParameters param = new DialogParameters();
                param.Header = "Price Effective Date is required";
                param.Content = "Please specify the Price Effective Date";
                RadWindow.Alert(param);
            }
        }

        void mrsClient_UpdateEstimateEffectiveDateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                HomePrice selectedItem = (HomePrice)cmbEffectiveDate.SelectedItem;

                string extraDescription = string.Format("Price Effective Date changed from {0} ({1}) to {2}",
                    _originalEffectiveDate.ToString("dd/MM/yyyy"),
                    _originalHomePrice.ToString("c"),
                    selectedItem.EffectiveDateOptionName);

                _mrsClient.CreateSalesEstimateLogCompleted += new EventHandler<CreateSalesEstimateLogCompletedEventArgs>(mrsClient_CreateSalesEstimateLogCompleted);
                _mrsClient.CreateSalesEstimateLogAsync(
                    (App.Current as App).CurrentUserLoginName,
                    MRSLogAction.ModifyEffectiveDate,
                    _recordId,
                    extraDescription,
                    0);
            }
            else
            {
                ExceptionHandler.PopUpErrorMessage(e.Error, "UpdateEstimateEffectiveDateCompleted");
                BusyIndicator1.IsBusy = false;
            }
        }

        void mrsClient_CreateSalesEstimateLogCompleted(object sender, CreateSalesEstimateLogCompletedEventArgs e)
        {
            BusyIndicator1.IsBusy = false;

            if (e.Error != null)
                ExceptionHandler.PopUpErrorMessage(e.Error, "CreateSalesEstimateLogCompleted");

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

