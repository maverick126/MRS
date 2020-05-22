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
using System.Collections.ObjectModel;

namespace Metricon.Silverlight.MetriconRetailSystem.ChildWindows
{
    public partial class SetCustomerDocument : ChildWindow
    {
        private int _estimateRevisionId;
        private int _userId;
        private int _customerDocumentId;
        private string _documentType;
        private bool _documentTypeAcive = false;
        private RetailSystemClient mrsClient;
        private string _notesToUser = "If the {0} is ready to issue to the customer tick 'Set as {0}', enter a Sent Date and save." + Environment.NewLine +
                                      "If changes are required to the {0} prior to the customer signing, untick “Set as {0}”, make the changes and then repeat step one." + Environment.NewLine +
                                      "Once {0} is signed by the customer enter the Counter Signed Date, Save and complete your revision.";

        public SetCustomerDocument(int estimateRevisionId, int userId)
        {
            InitializeComponent();

            _estimateRevisionId = estimateRevisionId;
            _userId = userId;

            mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            mrsClient.GetCustomerDocumentDetailsCompleted += new EventHandler<GetCustomerDocumentDetailsCompletedEventArgs>(mrsClient_GetCustomerDocumentDetailsCompleted);
            mrsClient.GetCustomerDocumentDetailsAsync(_estimateRevisionId);
        }

        void mrsClient_GetCustomerDocumentDetailsCompleted(object sender, GetCustomerDocumentDetailsCompletedEventArgs e)
        {
            if (e.Error == null && e.Result != null)
            {
                CustomerDocumentDetails document = e.Result;

                if (document.DocumentType == "ERROR")
                {
                    RadWindow.Alert(document.DocumentSummary);
                    CancelButton_Click(sender, null);
                }
                else
                {
                    if (document.DocumentType == "Contract" || document.DocumentType == "STM" || (this.Title.ToString().ToUpper().Contains("CONTRACT") && document.DocumentType == "PC")) // && !document.Active  ))
                    {
                        document.DocumentType = "Contract";
                        DocumentNumberTextBlock.Visibility = System.Windows.Visibility.Collapsed;
                        txtDocumentNumber.Visibility = System.Windows.Visibility.Collapsed;
                        ExtensionDaysTextBlock.Visibility = System.Windows.Visibility.Collapsed;
                        txtExtensionDays.Visibility = System.Windows.Visibility.Collapsed;
                    }
                    else if (document.DocumentType == "PC" || document.DocumentType == "PC-TP") // PC with Town Planning
                    {
                        document.DocumentType = "PC";
                        ExtensionDaysTextBlock.Visibility = System.Windows.Visibility.Collapsed;
                        txtExtensionDays.Visibility = System.Windows.Visibility.Collapsed;
                    }

                    _documentTypeAcive = document.Active;
                    if (_documentTypeAcive)
                        chkActive.IsChecked = true;
                    else
                        chkActive.IsChecked = false;

                    if (document.SentDate.HasValue)
                        dtSentDate.SelectedDate = document.SentDate;

                    if (document.ExtensionDays.HasValue)
                        txtExtensionDays.Text = document.ExtensionDays.Value.ToString();

                    if (document.DocumentNumber.HasValue)
                        txtDocumentNumber.Text = document.DocumentNumber.Value.ToString();

                    _customerDocumentId = document.CustomerDocumentID;
                    _documentType = document.DocumentType;

                    this.Title = "Set This Revision As " + document.DocumentType;
                    SetDocumentTypeTextBlock.Text = "Set As " + document.DocumentType;

                    if (document.DocumentType != null && document.DocumentType != "" && document.DocumentType.ToUpper().Contains("VARIATION"))
                    {
                        summaryblock.Visibility = Visibility.Visible;
                        txtsummary.Visibility = Visibility.Visible;
                        txtsummary.Text = document.DocumentSummary;
                    }

                    if (document.AcceptedDate.HasValue)
                    {
                        dtAcceptedDate.SelectedDate = document.AcceptedDate;
                        //If Sent Date has been specified, do not allow it to be changed and do not allow un check 'Set AS' checkbox 
                        if (document.AcceptedDate != null)
                        {
                            dtSentDate.IsEnabled = false;
                            dtAcceptedDate.IsEnabled = false;
                            chkActive.IsEnabled = false;
                        }
                    }

                    txtnotes.Text = string.Format(_notesToUser, _documentType);
                }
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetCustomerDocumentDetailsCompleted");

            BusyIndicator1.IsBusy = false;             
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            CustomerDocumentDetails document = new CustomerDocumentDetails();

            if ((!chkActive.IsChecked.HasValue || !chkActive.IsChecked.Value) && _customerDocumentId == 0)
            {
                DialogParameters param = new DialogParameters();
                param.Header = "Data Not Saved";
                param.Content = "Data will not be saved because the document is not set.\r\nPlease click Cancel to close this window.";
                RadWindow.Alert(param);
            }
            else if (dtAcceptedDate.SelectedDate.HasValue && dtSentDate.SelectedDate.HasValue && dtAcceptedDate.SelectedDate < dtSentDate.SelectedDate)
            {
                DialogParameters param = new DialogParameters();
                param.Header = "Invalid Counter Signed Date";
                param.Content = "The Counter Signed Date cannot be earlier than the Sent Date";
                RadWindow.Alert(param);
            }
            else
            {
                BusyIndicator1.IsBusy = true;

                document.CustomerDocumentID = _customerDocumentId;

                if (dtSentDate.SelectedDate != null)
                    document.SentDate = dtSentDate.SelectedDate;

                if (dtAcceptedDate.SelectedDate != null)
                    document.AcceptedDate = dtAcceptedDate.SelectedDate;

                if (chkActive.IsChecked.HasValue && chkActive.IsChecked.Value)
                    document.Active = true;
                else
                    document.Active = false;

                if (!string.IsNullOrEmpty(txtDocumentNumber.Text))
                    document.DocumentNumber = Convert.ToInt32(txtDocumentNumber.Text);

                if (!string.IsNullOrEmpty(txtExtensionDays.Text))
                    document.ExtensionDays = Convert.ToInt32(txtExtensionDays.Text);

                if (txtsummary.Visibility == Visibility.Visible)
                {
                    document.DocumentSummary = txtsummary.Text;
                }

                document.DocumentType = _documentType;
                document.EstimateRevisionID = _estimateRevisionId;
                document.UserId = _userId;

                GetNextRevision();
            }

            RadWindow window = this.ParentOfType<RadWindow>();
            if (window != null)
            {
                window.DialogResult = document.Active;
            }
        }

        private void GetNextRevision()
        {
            mrsClient.GetNextEstimateRevisionCompleted += new EventHandler<GetNextEstimateRevisionCompletedEventArgs>(_mrsClient_GetNextEstimateRevisionCompleted);
            mrsClient.GetNextEstimateRevisionAsync(_estimateRevisionId, 2);
        }

        void _mrsClient_GetNextEstimateRevisionCompleted(object sender, GetNextEstimateRevisionCompletedEventArgs e)
        {
            RadWindow window = this.ParentOfType<RadWindow>();

            if (e.Error == null)
            {
                //NextRevision nextRevision = e.Result;
                //if (nextRevision != null)
                //{
                //    if (nextRevision.Name != null)
                //        txtNotes.Text = "Next Revision: " + nextRevision.Name;
                //    else
                //        txtNotes.Text += "Next Revision: N/A";
                //    if (nextRevision.Notes != null)
                //        txtNotes.Text += "\r\n" + nextRevision.Notes;
                //}

                List<NextRevision> revisions = e.Result.ToList();
                // if no TBA available due to NSR then don't allow to set as pc, contract or variation
                if (chkActive.IsChecked.HasValue && chkActive.IsChecked.Value &&
                    revisions.Where(w => w.RevisionTypeId == 0).FirstOrDefault() == null)
                {
                    DialogParameters param = new DialogParameters();
                    param.Header = "Unable to Set as " + _documentType;
                    param.Content = "Unable to Set as " + _documentType + " as estimate contains items not accepted by the Sales Estimator.\r\nPlease send job to Sales Estimator to review items not accepted.";                     
                    RadWindow.Alert(param);
                    if (window != null)
                    {
                        window.DialogResult = false;
                        window.Close();
                    }
                }
                else
                {
                    UpdateCustomerDocumentDetails();
                    if (window != null)
                    {
                        window.Close();
                    }
                }
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetNextEstimateRevisionCompleted");

            BusyIndicator1.IsBusy = false;
        }

        void UpdateCustomerDocumentDetails()
        {
            CustomerDocumentDetails document = new CustomerDocumentDetails();

            document.CustomerDocumentID = _customerDocumentId;

            if (dtSentDate.SelectedDate != null)
                document.SentDate = dtSentDate.SelectedDate;

            if (dtAcceptedDate.SelectedDate != null)
                document.AcceptedDate = dtAcceptedDate.SelectedDate;

            if (chkActive.IsChecked.HasValue && chkActive.IsChecked.Value)
                document.Active = true;
            else
                document.Active = false;

            if (!string.IsNullOrEmpty(txtDocumentNumber.Text))
                document.DocumentNumber = Convert.ToInt32(txtDocumentNumber.Text);

            if (!string.IsNullOrEmpty(txtExtensionDays.Text))
                document.ExtensionDays = Convert.ToInt32(txtExtensionDays.Text);

            if (txtsummary.Visibility == Visibility.Visible)
            {
                document.DocumentSummary = txtsummary.Text;
            }

            document.DocumentType = _documentType;
            document.EstimateRevisionID = _estimateRevisionId;
            document.UserId = _userId;

            mrsClient.UpdateCustomerDocumentDetailsCompleted += new EventHandler<UpdateCustomerDocumentDetailsCompletedEventArgs>(mrsClient_UpdateCustomerDocumentDetailsCompleted);
            mrsClient.UpdateCustomerDocumentDetailsAsync(document);

            CreateLog(document.Active);
        }

        void mrsClient_UpdateCustomerDocumentDetailsCompleted(object sender, UpdateCustomerDocumentDetailsCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                _customerDocumentId = e.Result;

                RadWindow window = this.ParentOfType<RadWindow>();
                if (window != null)
                {
                    window.DialogResult = chkActive.IsChecked.HasValue && chkActive.IsChecked.Value ? true : false;
                    window.Close();
                }
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "UpdateCustomerDocumentDetailsCompleted");

            BusyIndicator1.IsBusy = false;  
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            RadWindow window = this.ParentOfType<RadWindow>();
            if (window != null)
            {
                window.DialogResult = _documentTypeAcive ? true : false;
                window.Close();
            }
        }

        private void CreateLog(bool setAsTicked)
        {
            string description = (setAsTicked ? "Set" : "Unset") + " this revision as " + _documentType + " by user " + (App.Current as App).CurrentUserLoginName;

            RetailSystemClient mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());
            mrsClient.CreateSalesEstimateLogAsync(
                (App.Current as App).CurrentUserLoginName,
                MRSLogAction.CurrentMilestone,
                _estimateRevisionId,
                description,
                0);
        }

        private void dtSentDate_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangedEventArgs e)
        {
            //if (_documentType != null && _documentType != "" && !_documentType.ToUpper().Contains("VARIATION"))
            //    chkActive.IsChecked = dtSentDate.SelectedDate.HasValue;
        }

        private void chkActive_Click(object sender, RoutedEventArgs e)
        {
            if (_documentType != null && _documentType != "" && !_documentType.ToUpper().Contains("VARIATION") && dtAcceptedDate.SelectedDate.HasValue)
                chkActive.IsChecked = true;

            if (!(bool)chkActive.IsChecked)
            {
                dtSentDate.SelectedDate = null;
                dtAcceptedDate.SelectedDate = null;
            }
        }

        private void dtAcceptedDate_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (_documentType != null && _documentType != "" && !_documentType.ToUpper().Contains("VARIATION"))
                chkActive.IsChecked = dtAcceptedDate.SelectedDate.HasValue;
        }
    }
}

