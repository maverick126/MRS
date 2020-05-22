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
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
using Metricon.Silverlight.MetriconRetailSystem.ViewModels;
using Metricon.Silverlight.MetriconRetailSystem.MRSService;
using System.Collections.ObjectModel;

namespace Metricon.Silverlight.MetriconRetailSystem.ChildWindows
{
    public partial class EnterMarginAttributes : ChildWindow
    {
        private int _revisionid = 0;
        private double _bpeCharge = 0.00;
        private DateTime _revisedpriceexpirydate = DateTime.Now;
        private EstimateViewModel _estmateViewModel = null;
        private ObservableCollection<HomePrice> _homePricesEffectiveDatesList = null;
        private bool _updateInBackground = false;
        private int _titledLand = 0;
        private int _basePriceExtensionDays = 0;
        private int _requiredBPEChargeType = 0;

        public EnterMarginAttributes(int revisionid, EstimateViewModel ev)
        {
            InitializeComponent();

            _revisionid = revisionid;
            PopulateDetails(ev);
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            BusyIndicator1.IsBusy = true;

            OKButton.Visibility = Visibility.Collapsed;
            CancelButton.Visibility = Visibility.Collapsed;

            if (!(bool)radBPECharge.IsChecked && !(bool)radBPEChargeRollback.IsChecked) // && !(bool)radBPEChargeTodayPrice.IsChecked)
            {
                MessageBoxResult confirm = MessageBox.Show("Please select either Required PBE charge (@5%) or Required PBE charge (Rollback) or Required PBE charge (Today's Price).", "", MessageBoxButton.OKCancel);
            }
            int basePriceExtensionDays = 0;
            int radBPEChargeType = 0;
            int newTitledLandDays = _estmateViewModel.TitledLandDays;
            int titledLand = 0;
            double requiredBPECharge = 0.00;

            int.TryParse(txttitledlanddays.Text.ToString(), out newTitledLandDays);
            int.TryParse(txtbasepriceextdays.Text, out basePriceExtensionDays);
            if (radBPECharge.IsChecked ?? false)
            {
                radBPEChargeType = 0;
                requiredBPECharge = _bpeCharge;
            }
            else if (radBPEChargeRollback.IsChecked ?? false)
            {
                radBPEChargeType = 1;
                double.TryParse(txtrequireBPErollbackprice.Text.ToString().Replace("$", ""), out requiredBPECharge);
            }
            //else if (radBPEChargeTodayPrice.IsChecked??false)
            //    radBPEChargeType = 2;
            if (radtitlelandofferyes.IsChecked ?? false)
                titledLand = 1;

            RetailSystemClient mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            mrsClient.MarginReport_SaveDetailsCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(mrsClient_MarginReport_SaveDetailsCompleted);
            mrsClient.MarginReport_SaveDetailsAsync(_revisionid, titledLand, newTitledLandDays, basePriceExtensionDays, _revisedpriceexpirydate, Math.Round(requiredBPECharge), radBPEChargeType, (App.Current as App).CurrentUserId);
        }

        void mrsClient_MarginReport_SaveDetailsCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                CreateLog();
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "MarginReport_SaveDetailsCompleted");

            OKButton.Visibility = Visibility.Visible;
            CancelButton.Visibility = Visibility.Visible;
            BusyIndicator1.IsBusy = false;
        }

        private void CreateLog()
        {
            string changes = string.Empty;
            int titledLandDays = 0;
            int basePriceExtensionDays = 0;
            int radBPEChargeType = 0;
            int newTitledLandDays = _estmateViewModel.TitledLandDays;
            int titledLand = 0;

            int.TryParse(txttitledlanddays.Text.ToString(), out newTitledLandDays);
            int.TryParse(txtbasepriceextdays.Text, out basePriceExtensionDays);
            if (radBPECharge.IsChecked ?? false)
                radBPEChargeType = 0;
            else if (radBPEChargeRollback.IsChecked ?? false)
                radBPEChargeType = 1;

            int.TryParse(txttitledlanddays.Text.ToString(), out titledLandDays);
             if (radtitlelandofferyes.IsChecked ?? false)
                titledLand = 1;
            if (_titledLand != titledLand)
            {
                changes += " TitledLand old: " + _titledLand + " new: " + titledLand.ToString();
            }
            if (_estmateViewModel.TitledLandDays != titledLandDays)
            {
                changes += " TitledLandDays old: " + _estmateViewModel.TitledLandDays + " new: " + titledLandDays.ToString();
            }
            if (_basePriceExtensionDays != basePriceExtensionDays)
            {
                changes += " BasePriceExtensionDays old: " + _basePriceExtensionDays + " new: " + basePriceExtensionDays.ToString();
            }
            if (_requiredBPEChargeType != radBPEChargeType)
            {
                changes += " BPEChargeType old: " + _requiredBPEChargeType + " new: " + radBPEChargeType.ToString();
            }

            if (txtrequireBPEcharge.Text.ToString() != txtbasepriceextcharge.Text.ToString())
            {
                changes += " Base Price Ext Charge old: " + txtrequireBPEcharge.Text.ToString() + " new: " + txtbasepriceextcharge.Text.ToString();
            }
            if (txtrequireBPECurrentHomePrice.Text.ToString() != txtrequireBPErollbackprice.Text.ToString())
            {
                changes += " Home Price old: " + txtrequireBPECurrentHomePrice.Text.ToString() + " new: " + txtrequireBPErollbackprice.Text.ToString();
            }

            string description = "Margin report settings have been changed to " + changes + " by user " + (App.Current as App).CurrentUserFullName;
            RetailSystemClient MRSclient = new RetailSystemClient();
            MRSclient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            MRSclient.CreateSalesEstimateLogCompleted += new EventHandler<CreateSalesEstimateLogCompletedEventArgs>(mrsClient_CreateSalesEstimateLogCompleted);
            MRSclient.CreateSalesEstimateLogAsync(
                (App.Current as App).CurrentUserLoginName,
                MRSLogAction.UpdateMarginReportSettings,
                _revisionid,
                description,
                0);
        }

        void mrsClient_CreateSalesEstimateLogCompleted(object sender, CreateSalesEstimateLogCompletedEventArgs e)
        {
            BusyIndicator1.IsBusy = false;

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

        private void PopulateDetails(EstimateViewModel ev)
        {
            RetailSystemClient mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            mrsClient.MarginReport_GetDetailsCompleted += new EventHandler<MarginReport_GetDetailsCompletedEventArgs>(mrsClient_MarginReport_GetDetailsCompleted);
            mrsClient.MarginReport_GetDetailsAsync(_revisionid);

            _estmateViewModel = ev;
        }

        void mrsClient_MarginReport_GetDetailsCompleted(object sender, MarginReport_GetDetailsCompletedEventArgs e)
        {
            BusyIndicator1.IsBusy = false;

            DateTime depositDate = DateTime.Parse(_estmateViewModel.DepositDate);
            DateTime expectedACCDate = depositDate;
            if (e.Error == null)
            {
                if (e.Result.BCForecastDate != null)
                {
                    expectedACCDate = DateTime.Parse(e.Result.BCForecastDate);
                }
                txtexpectedaccedate.Text = expectedACCDate.ToString("dd/MM/yyyy");
                _estmateViewModel.TitledLandDays = e.Result.TitledLandDays;
                if (e.Result.TitledLand)
                {
                    radtitlelandofferyes.IsChecked = true;
                    txttitledlanddays.Text = _estmateViewModel.TitledLandDays.ToString();
                    radrevisedpriceexpirydate.SelectedDate = depositDate.AddDays(e.Result.BasePriceExtensionDays + e.Result.TitledLandDays);
                    _titledLand = 1;
                }
                else
                {
                    radtitlelandofferno.IsChecked = true;
                    txttitledlanddays.Text = "0";
                    radrevisedpriceexpirydate.SelectedDate = depositDate.AddDays(e.Result.BasePriceExtensionDays + e.Result.DaysFrom);
                }
                txtstdpriceholddays.Text = e.Result.DaysFrom.ToString();
                _basePriceExtensionDays = e.Result.BasePriceExtensionDays;
                _estmateViewModel.BasePriceExtensionDays = e.Result.BasePriceExtensionDays;
                _estmateViewModel.BasePriceExtensionCharge = (decimal) e.Result.BPECharge;
                _requiredBPEChargeType = e.Result.RequiredBPEChargeType;
                if (_requiredBPEChargeType == 0)
                {
                    radBPECharge.IsChecked = true;
                    txtrequireBPErollbackprice.Text = _estmateViewModel.HomePrice;
                }
                else if (_requiredBPEChargeType == 1)
                {
                    radBPEChargeRollback.IsChecked = true;
                    txtrequireBPErollbackprice.Text = e.Result.RequiredBPECharge.ToString("c");
                }
                //else if (e.Result.RequiredBPECharge == 2)
                //    radBPEChargeTodayPrice.IsChecked = true;
                _estmateViewModel.RequiredPBETodaysPrice = (decimal) e.Result.TodaysPrice;

                _homePricesEffectiveDatesList = e.Result.PriceEffectiveDates;
                if (_homePricesEffectiveDatesList != null)
                {
                    _homePricesEffectiveDatesList.Insert(0, new HomePrice { PriceId = 0, EffectiveDateOptionName = "Select" });
                    cmbEffectiveDate.ItemsSource = _homePricesEffectiveDatesList;
                }

                cmbEffectiveDate.SelectedIndex = 0;
            }

            txteffectivedate.Text = _estmateViewModel.EffectiveDate;
            txttoday.Text = DateTime.Now.ToString("dd/MM/yyyy");
            txtdepositdate.Text = _estmateViewModel.DepositDate;
            txtbasepriceextcharge.Text = String.Format("${0:0.00}", _estmateViewModel.BasePriceExtensionCharge);
            // txtrevisedpriceholddays.Text = _estmateViewModel.RevisedPriceHoldDays.ToString();

            calculateRevisedPriceHoldDays();

            DateTime expdate = DateTime.Parse(_estmateViewModel.EffectiveDate);
            if (expdate < DateTime.Now)
            {
                decimal homePriceInEstimate = 0;
                decimal.TryParse(_estmateViewModel.HomePrice.Replace("$", ""), out homePriceInEstimate);
                double interestPerDay = 0.000164;

                _bpeCharge = interestPerDay * (double) homePriceInEstimate * (_estmateViewModel.BasePriceExtensionDays - 30); // first 30 days no charge
                txtrequireBPEcharge.Text = String.Format("${0:0.00}", Math.Round(_bpeCharge));
                txtrequireBPECurrentHomePrice.Text = _estmateViewModel.HomePrice;
                txtIncreaseInprice.Text = String.Format("${0:0.00}", _estmateViewModel.RequiredPBETodaysPrice - homePriceInEstimate);
                DateTime dtTemp = DateTime.Now;
                DateTime.TryParse(_estmateViewModel.EffectiveDate, out dtTemp);
                txtNewPriceEffectiveDate.Text = dtTemp.ToString("dd/MM/yyyy");
            }
            else
            {
                //txtrequireBPEcharge.IsEnabled = false;
                //txtrequireBPEchargerollback.IsEnabled = false;
                //charge.IsEnabled = false;
                //chargerollback.IsEnabled = false;
            }
            //Get Effective Date Options
            //PopulateEffectiveDates(_revisionid);
        }

        //public void PopulateEffectiveDates(int estimateRevisionId)
        //{
        //    RetailSystemClient mrsClient = new RetailSystemClient();

        //    mrsClient.GetHomePricesCompleted += new EventHandler<GetHomePricesCompletedEventArgs>(mrsClient_GetHomePricesCompleted);
        //    mrsClient.GetHomePricesAsync(estimateRevisionId);
        //}

        //void mrsClient_GetHomePricesCompleted(object sender, GetHomePricesCompletedEventArgs e)
        //{
        //    if (e.Error == null)
        //    {
        //        _homePricesEffectiveDatesList = e.Result;
        //        _homePricesEffectiveDatesList.Insert(0, new HomePrice { PriceId = 0, EffectiveDateOptionName = "Select" });
        //        cmbEffectiveDate.ItemsSource = _homePricesEffectiveDatesList;

        //        cmbEffectiveDate.SelectedIndex = 0;
        //    }
        //    else
        //        ExceptionHandler.PopUpErrorMessage(e.Error, "GetHomePricesCompleted");

        //    BusyIndicator1.IsBusy = false;
        //}

        private void txtaccedate_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangedEventArgs e)
        {

        }

        private void txtpriceexpirydate_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangedEventArgs e)
        {

        }

        private void radrevisedpriceexpirydate_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangedEventArgs e)
        {
            DateTime depositDate = DateTime.Parse(_estmateViewModel.DepositDate);

            if (radrevisedpriceexpirydate.SelectedDate != null)
            {
                txtbasepriceextdays.Text = (radrevisedpriceexpirydate.SelectedDate - depositDate).Value.TotalDays.ToString();
                if (!_updateInBackground)
                {
                    calculateRevisedPriceHoldDays();
                }
            }
        }

        private void calculateRevisedPriceHoldDays()
        {
            int titledLandDays = 0;
            DateTime depositDate = DateTime.Parse(_estmateViewModel.DepositDate);
            int stdPriceHoldDays = 0;
            int revisedPriceHolidays = (int)(radrevisedpriceexpirydate.SelectedDate - depositDate).Value.TotalDays;

            int.TryParse(txtstdpriceholddays.Text.ToString(), out stdPriceHoldDays);
            int.TryParse(txttitledlanddays.Text.ToString(), out titledLandDays);

            // std price hold days + base price extension days – titled land days
            if (radtitlelandofferyes.IsChecked ?? false)
                _estmateViewModel.BasePriceExtensionDays = revisedPriceHolidays - titledLandDays;
            else
                _estmateViewModel.BasePriceExtensionDays = revisedPriceHolidays - stdPriceHoldDays;

            if ((_estmateViewModel.BasePriceExtensionDays - 30) < 0)
            {
                _estmateViewModel.BasePriceExtensionDays = 0;
            }
             txtbasepriceextdays.Text = _estmateViewModel.BasePriceExtensionDays.ToString();
            _revisedpriceexpirydate = depositDate.AddDays(revisedPriceHolidays);

            decimal homePriceInEstimate = 0;
            decimal.TryParse(_estmateViewModel.HomePrice.Replace("$", ""), out homePriceInEstimate);
            double interestPerDay = 0.000164;

            if ((_estmateViewModel.BasePriceExtensionDays - 30) > 0)
            {
                _bpeCharge = interestPerDay * (double)homePriceInEstimate * (_estmateViewModel.BasePriceExtensionDays - 30); // first 30 days no charge
            }
            else
            {
                _bpeCharge = 0;
            }

            txtrequireBPEcharge.Text = String.Format("${0:0.00}", Math.Round(_bpeCharge));
        }

        private void radtitlelandofferyes_Checked(object sender, RoutedEventArgs e)
        {
            txttitledlanddays.IsReadOnly = false;
            txttitledlanddays.Text = _estmateViewModel.TitledLandDays.ToString();
            decimal homePriceInEstimate = 0;
            decimal.TryParse(_estmateViewModel.HomePrice.Replace("$", ""), out homePriceInEstimate);
            double interestPerDay = 0.000167;

            if (_estmateViewModel != null)
            {
                _bpeCharge = interestPerDay * (double)homePriceInEstimate * (_estmateViewModel.BasePriceExtensionDays - 30); // first 30 days no charge
                txtrequireBPEcharge.Text = String.Format("${0:0.00}", _bpeCharge);
            }
        }

        private void radtitlelandofferno_Checked(object sender, RoutedEventArgs e)
        {
            if (txttitledlanddays != null)
            {
                txttitledlanddays.IsReadOnly = true;
                txttitledlanddays.Text = "0";
            }

            if (_estmateViewModel != null)
            {
                decimal homePriceInEstimate = 0;
                decimal.TryParse(_estmateViewModel.HomePrice.Replace("$", ""), out homePriceInEstimate);
                double interestPerDay = 0.000167;

                _bpeCharge = interestPerDay * (double)homePriceInEstimate * (_estmateViewModel.BasePriceExtensionDays - 30); // first 30 days no charge
                txtrequireBPEcharge.Text = String.Format("${0:0.00}", _bpeCharge);
            }
        }

        private void txtstdpriceholddays_TextChanged(object sender, TextChangedEventArgs e)
        {
            calculateRevisedPriceHoldDays();
        }

        private void txtbasepriceextdays_TextChanged(object sender, TextChangedEventArgs e)
        {
            calculateRevisedPriceHoldDays();
        }

        private void txttitledlanddays_TextChanged(object sender, TextChangedEventArgs e)
        {
            calculateRevisedPriceHoldDays();
        }

        private void cmbEffectiveDate_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangedEventArgs e)
        {
            // decimal totalPriceExc = 0;
            decimal selectedPrice = 0;
            decimal homePriceInEstimate = 0;
            decimal.TryParse(_estmateViewModel.HomePrice.Replace("$", ""), out homePriceInEstimate);
            // decimal.TryParse(_estmateViewModel.TotalPrice.Replace("$", ""), out totalPriceExc);
            if (cmbEffectiveDate.SelectedIndex > 0)
            {
                selectedPrice = _homePricesEffectiveDatesList[cmbEffectiveDate.SelectedIndex].EffectivePrice;
                txtIncreaseInprice.Text = (selectedPrice - homePriceInEstimate).ToString("c");
                txtrequireBPErollbackprice.Text = _homePricesEffectiveDatesList[cmbEffectiveDate.SelectedIndex].EffectivePrice.ToString("c");
                txtNewPriceEffectiveDate.Text = _homePricesEffectiveDatesList[cmbEffectiveDate.SelectedIndex].EffectiveDate.ToString();
            }
            calculateRevisedPriceHoldDays();
        }
    }
}

