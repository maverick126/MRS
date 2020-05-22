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
using Telerik.Windows.Controls;

namespace Metricon.Silverlight.MetriconRetailSystem.ChildWindows
{
    public partial class CompareEstimates : ChildWindow
    {
        public CompareEstimates(object dataContext)
        {
            InitializeComponent();

            this.Width = App.Current.Host.Content.ActualWidth-10;
            this.Height = App.Current.Host.Content.ActualHeight-30;

            CompareGrid.MaxHeight = App.Current.Host.Content.ActualHeight - 350;

            EstimateGridItem item = (EstimateGridItem)dataContext;

            this.Title = String.Format(this.Title.ToString(), item.EstimateId.ToString());

            BusyIndicator1.IsBusy = true; 

            RetailSystemClient mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            mrsClient.GetEstimatesRevisionsCompleted += new EventHandler<GetEstimatesRevisionsCompletedEventArgs>(mrsClient_GetEstimatesRevisionsCompleted);
            mrsClient.GetEstimatesRevisionsAsync(item.EstimateId);
        }

        void mrsClient_GetEstimatesRevisionsCompleted(object sender, GetEstimatesRevisionsCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                System.Collections.ObjectModel.ObservableCollection<EstimateHeader> headers = e.Result;
                System.Collections.ObjectModel.ObservableCollection<EstimateGridItem> comboItems = new System.Collections.ObjectModel.ObservableCollection<EstimateGridItem>();

                foreach (EstimateHeader header in headers)
                {
                    EstimateGridItem item = new EstimateGridItem();
                    item.RecordId = header.RecordId;
                    item.RevisionDetails = "Revision " + header.RevisionNumber.ToString() + " " + header.RevisionTypeCode ;
                    comboItems.Add(item);
                }

                cmbRevision1.ItemsSource = comboItems;
                cmbRevision2.ItemsSource = comboItems;

                if (cmbRevision1.Items.Count > 0)
                    cmbRevision1.SelectedIndex = cmbRevision1.Items.Count - 1;
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetEstimatesRevisionsCompleted");

            BusyIndicator1.IsBusy = false;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            RadWindow window = this.ParentOfType<RadWindow>();
            if (window != null)
                window.Close();
        }

        private void btnCompare_Click(object sender, RoutedEventArgs e)
        {
            if (cmbRevision1.SelectedItem != null && cmbRevision2.SelectedItem != null)
            {
                BusyIndicator1.IsBusy = true;

                HeaderGrid.Columns[1].Header = ((EstimateGridItem)cmbRevision1.SelectedItem).RevisionDetails;
                HeaderGrid.Columns[2].Header = ((EstimateGridItem)cmbRevision2.SelectedItem).RevisionDetails;

                HeaderCompareGrid.Columns[1].Header = HeaderGrid.Columns[1].Header;
                HeaderCompareGrid.Columns[2].Header = HeaderGrid.Columns[2].Header;

                RetailSystemClient mrsClient = new RetailSystemClient();
                mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

                mrsClient.CompareEstimateHeaderCompleted += new EventHandler<CompareEstimateHeaderCompletedEventArgs>(mrsClient_CompareEstimateHeaderCompleted);
                mrsClient.CompareEstimateHeaderAsync(Convert.ToInt32(cmbRevision1.SelectedValue), Convert.ToInt32(cmbRevision2.SelectedValue));
            }
            else
            {
                DialogParameters param = new DialogParameters();
                param.Header = "Estimate Revisions Required";
                param.Content = "Please specify both Source and Destination revisions to compare.";
                RadWindow.Alert(param);
            }
        }

        void mrsClient_CompareEstimateDetailsCompleted(object sender, CompareEstimateDetailsCompletedEventArgs e)
        {
            if (e.Error == null)
            {
               
                CompareGrid.ItemsSource = e.Result;
                CompareGrid.ExpandAllGroups();

                btnPrint.IsEnabled = true;
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "CompareEstimateDetailsCompleted");

            BusyIndicator1.IsBusy = false;   
        }

        void mrsClient_CompareEstimateHeaderCompleted(object sender, CompareEstimateHeaderCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                HeaderCompareGrid.ItemsSource = e.Result;

                RetailSystemClient mrsClient = new RetailSystemClient();
                mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

                mrsClient.CompareEstimateDetailsCompleted += new EventHandler<CompareEstimateDetailsCompletedEventArgs>(mrsClient_CompareEstimateDetailsCompleted);
                mrsClient.CompareEstimateDetailsAsync(Convert.ToInt32(cmbRevision1.SelectedValue), Convert.ToInt32(cmbRevision2.SelectedValue));
            }
            else
            {
                ExceptionHandler.PopUpErrorMessage(e.Error, "CompareEstimateHeaderCompleted");
                BusyIndicator1.IsBusy = false;
            }
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            if (cmbRevision1.SelectedItem != null && cmbRevision2.SelectedItem != null)
            {
                string sourceId = ((EstimateGridItem)cmbRevision1.SelectedItem).RecordId.ToString();
                string destinationId = ((EstimateGridItem)cmbRevision2.SelectedItem).RecordId.ToString();
                //List<Filter> filterlist= GetAllFilters();
                string filter = GetAllGridData();
                RadWindow win = new RadWindow();
                ComparisonPrint printDlg = new ComparisonPrint(sourceId, destinationId, filter);
                win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                win.Header = "Print Estimate Comparison";
                win.Content = printDlg;
                win.ShowDialog();
            }
            else
            {
                DialogParameters param = new DialogParameters();
                param.Header = "Estimate Revisions Required";
                param.Content = "Please specify both Source and Destination revisions to compare.";
                RadWindow.Alert(param);
            }            
        }
        private string GetAllGridData()
        {
            string result = "";

            foreach(var itm in  CompareGrid.Items)
            {
                if (result == "")
                {
                    result = (itm as EstimateDetailsComparison).PagID;
                }
                else
                {
                    result = result+","+(itm as EstimateDetailsComparison).PagID;
                }
            }
            return result;
        }
        private List<Filter> GetAllFilters()
        {
            IList<FilterSetting> settings = new List<FilterSetting>();
            List<Filter> result = new List<Filter>();
            foreach (Telerik.Windows.Data.IFilterDescriptor filter in CompareGrid.FilterDescriptors)
            {
                Telerik.Windows.Controls.GridView.IColumnFilterDescriptor columnFilter = filter as Telerik.Windows.Controls.GridView.IColumnFilterDescriptor;
                if (columnFilter != null)
                {
                    FilterSetting setting = new FilterSetting();

                    setting.ColumnUniqueName = columnFilter.Column.UniqueName;

                    setting.SelectedDistinctValues.AddRange(columnFilter.DistinctFilter.DistinctValues);

                    if (columnFilter.FieldFilter.Filter1.IsActive)
                    {
                        setting.Filter1 = new FilterDescriptorProxy();
                        setting.Filter1.Operator = columnFilter.FieldFilter.Filter1.Operator;
                        setting.Filter1.Value = columnFilter.FieldFilter.Filter1.Value;
                        setting.Filter1.IsCaseSensitive = columnFilter.FieldFilter.Filter1.IsCaseSensitive;
                    }

                    setting.FieldFilterLogicalOperator = columnFilter.FieldFilter.LogicalOperator;

                    if (columnFilter.FieldFilter.Filter2.IsActive)
                    {
                        setting.Filter2 = new FilterDescriptorProxy();
                        setting.Filter2.Operator = columnFilter.FieldFilter.Filter2.Operator;
                        setting.Filter2.Value = columnFilter.FieldFilter.Filter2.Value;
                        setting.Filter2.IsCaseSensitive = columnFilter.FieldFilter.Filter2.IsCaseSensitive;
                    }

                    settings.Add(setting);
                }
            }
            foreach(FilterSetting s1 in settings)
            {
                foreach(string value in s1.SelectedDistinctValues)
                {
                    Filter f = new Filter();
                    f.ColumnUniqueName = s1.ColumnUniqueName;
                    f.FilterValue = value;
                    result.Add(f);
                }
                
            }

            return result;
        }

        private void cmbRevisions_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangedEventArgs e)
        {
            btnPrint.IsEnabled = false;
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            if (cmbRevision1.Items.Count > 0)
                cmbRevision1.SelectedIndex = cmbRevision1.Items.Count - 1;

            cmbRevision2.SelectedIndex = -1;

            HeaderCompareGrid.ItemsSource=null;
            CompareGrid.ItemsSource = null;

        }
    }

    public class FilterDescriptorProxy
    {
        public Telerik.Windows.Data.FilterOperator Operator { get; set; }
        public object Value { get; set; }
        public bool IsCaseSensitive { get; set; }
    }

    public class FilterSetting
    {
        public string ColumnUniqueName { get; set; }
        public List<object> SelectedDistinctValues = new List<object>();
        public FilterDescriptorProxy Filter1 { get; set; }
        public Telerik.Windows.Data.FilterCompositionLogicalOperator FieldFilterLogicalOperator { get; set; }
        public FilterDescriptorProxy Filter2 { get; set; }
    }
    public class Filter
    {
        public string ColumnUniqueName { get; set; }
        public string FilterValue { get; set; }

    }
}

