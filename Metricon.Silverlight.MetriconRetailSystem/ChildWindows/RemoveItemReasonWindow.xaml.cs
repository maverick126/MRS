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

using System.Collections.ObjectModel;

namespace Metricon.Silverlight.MetriconRetailSystem.ChildWindows
{
    public partial class RemoveItemReasonWindow : ChildWindow
    {
        RetailSystemClient _mrsClient;
        List<EstimateDetails> pag;
        public RemoveItemReasonWindow(List<EstimateDetails> listEstimateDetails, string itemsSelected)
        {
            InitializeComponent();
            pag = listEstimateDetails;

            _mrsClient = new RetailSystemClient();
            _mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            _mrsClient.GetItemRemoveReasonCompleted += new EventHandler<GetItemRemoveReasonCompletedEventArgs>(mrsClient_GetItemRemoveReasonCompleted);
            _mrsClient.GetItemRemoveReasonAsync();

            textBoxSelectedItems.Text = itemsSelected;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            RadWindow window = this.ParentOfType<RadWindow>();
            if (window != null)
                window.Close();
        }

        void mrsClient_GetItemRemoveReasonCompleted(object sender, GetItemRemoveReasonCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                ObservableCollection<ItemRemoveReason> reason = e.Result;
                cmbReason.ItemsSource = reason;
                cmbReason.SelectedIndex = 0;
 
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetItemRemoveReasonCompleted");
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            string reason = cmbReason.SelectedValue == null ? "" : cmbReason.SelectedValue.ToString();
            if (reason == "0")
            {
                DialogParameters param = new DialogParameters();
                param.Header = "Reason to Remove is required";
                param.Content = "Please select a Reason to Remove the item.";
                RadWindow.Alert(param);
            }

            else if (reason == "1" && String.IsNullOrEmpty(txtReason.Text))
            {
                DialogParameters param = new DialogParameters();
                param.Header = "Deletion Comments Required";
                param.Content = "Please enter Deletion Comments.";
                RadWindow.Alert(param);
            }
            else if (!string.IsNullOrWhiteSpace(reason))
            {
                _mrsClient.DeleteProductCompleted += delegate(object o, DeleteProductCompletedEventArgs es)
                {
                    if (es.Error == null)
                    {
                        if (es.Result != null)
                        {
                            RadWindow window = this.ParentOfType<RadWindow>();
                            if (window != null)
                            {
                                window.DialogResult = true;
                                window.Close();
                            }
                        }
                    }
                    else
                        ExceptionHandler.PopUpErrorMessage(es.Error, "DeleteProductCompleted");
                };

                foreach (EstimateDetails ed in pag)
                    _mrsClient.DeleteProductAsync(ed.EstimateRevisionDetailsId, txtReason.Text, int.Parse(cmbReason.SelectedValue.ToString()), (App.Current as App).CurrentUserId);
            }          
        }

        //void mrsClient_CompleteEstimateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        //{


        //    if (e.Error == null)
        //    {
        //        _mrsClient.SynchroniseCustomerDetailsCompleted += delegate(object o, SynchroniseCustomerDetailsCompletedEventArgs es)
        //        {

        //            if (es.Error == null)
        //            {
        //                RadWindow window = this.ParentOfType<RadWindow>();
        //                if (window != null)
        //                {
        //                    window.DialogResult = true;
        //                    window.Close();
        //                }
        //            }
        //            else
        //                ExceptionHandler.PopUpErrorMessage(es.Error, "SynchronizeCustomerDetails");

        //        };

        //       // _mrsClient.SynchroniseCustomerDetailsAsync(Convert.ToInt32(_estimatenumber));

        //    }
        //    else
        //    {
        //        ExceptionHandler.PopUpErrorMessage(e.Error, "CompleteEstimateCompleted");

        //    }
        //}
    }
}

