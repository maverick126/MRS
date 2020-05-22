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
using System.Globalization;


using System.Collections.ObjectModel;
using Telerik.Windows.Controls;
using Metricon.Silverlight.MetriconRetailSystem.MRSService;

namespace Metricon.Silverlight.MetriconRetailSystem.ChildWindows
{
    public partial class SetEstimateStatus : ChildWindow
    {       
        private int _statusId;
        private int _estimateRevisionId;
        private string _comments;
        private string _summary;
        private string _revisionnumner;
        private string _estimatenumber;
        private int _ownerId;
        private int _nextRevisionTypeId;
        private string _notes;
        private MRSLogAction _logAction;
        private RetailSystemClient _mrsClient;
        private string _customerdocumenttype = "";
        private string _customerdocumentnumber = "0";
        private bool _createNewRevision; // Whether a new revision will be created as a result of status change
        private EstimateHeader estimate;
        private double _totalvalue = 0;

        public SetEstimateStatus(int estimateRevisionId, int statusId)
        {
            InitializeComponent();

            _mrsClient = new RetailSystemClient();
            _mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            _estimateRevisionId = estimateRevisionId;
            _statusId = statusId;
            
            BusyIndicator1.IsBusy = true;

            if (statusId == (int)EstimateRevisionStatus.WorkInProgress || // Reactivate contract
                statusId == (int)EstimateRevisionStatus.OnHold || // Put contract On Hold
                statusId == (int)EstimateRevisionStatus.Cancelled) // Cancelled contract
                _createNewRevision = false;
            else
                _createNewRevision = true;

            GetEstimateHeader();
        }

        private void PopulateStatusReasons(int statusId, int revisionTypeId)
        {
            _mrsClient.GetStatusReasonsCompleted += new EventHandler<GetStatusReasonsCompletedEventArgs>(mrsClient_GetStatusReasonsCompleted);
            _mrsClient.GetStatusReasonsAsync(statusId, revisionTypeId);
        }

        void mrsClient_GetStatusReasonsCompleted(object sender, GetStatusReasonsCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                ObservableCollection<StatusReason> reasons = e.Result;

                reasons.Insert(0, new StatusReason { StatusReasonName = "Select", StatusReasonId = 0 });
                cmbReason.ItemsSource = reasons;

                if (reasons.Count == 2)
                {
                    cmbReason.SelectedIndex = 1;
                }
                else
                {
                    cmbReason.SelectedIndex = 0;
                }

                if (reasons.Count == 1)
                {
                    cmbReason.Visibility = System.Windows.Visibility.Collapsed;
                }

                GetNextRevision();
            }
            else
            {
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetStatusReasonsCompleted");
                BusyIndicator1.IsBusy = false;
            }
        }

        private void GetNextRevision()
        {
            _mrsClient.GetNextEstimateRevisionCompleted += new EventHandler<GetNextEstimateRevisionCompletedEventArgs>(_mrsClient_GetNextEstimateRevisionCompleted);
            _mrsClient.GetNextEstimateRevisionAsync(_estimateRevisionId, _statusId);
        }

        void _mrsClient_GetNextEstimateRevisionCompleted(object sender, GetNextEstimateRevisionCompletedEventArgs e)
        {
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

                ObservableCollection<NextRevision> revisions = e.Result;

                if (revisions.Count > 1)
                {
                    revisions.Insert(0, new NextRevision { RevisionTypeName = "Select", RevisionTypeId = -1, OwnerId = 0, Notes = "Please select the next Revision Type."});
                }

                cmbRevision.ItemsSource = revisions;

                cmbRevision.SelectedIndex = 0;
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetNextEstimateRevisionCompleted");

            BusyIndicator1.IsBusy = false;           
        }

        private void GetEstimateHeader()
        {
            _mrsClient.GetEstimateHeaderCompleted += new EventHandler<GetEstimateHeaderCompletedEventArgs>(mrsClient_GetEstimateHeaderCompleted);
            _mrsClient.GetEstimateHeaderAsync(_estimateRevisionId);
        }

        void mrsClient_GetEstimateHeaderCompleted(object sender, GetEstimateHeaderCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                estimate = e.Result;

                _revisionnumner = estimate.RevisionNumber.ToString();
                _estimatenumber = estimate.EstimateId.ToString();
                if (estimate.CustomerDocumentName != null && estimate.CustomerDocumentName != "")
                {
                    if (estimate.CustomerDocumentName.ToUpper().Contains("CONTRACT"))
                    {
                        _totalvalue = double.Parse(estimate.TotalPrice.ToString());
                    }
                    else if (estimate.CustomerDocumentName.ToUpper().Contains("VARIATION"))
                    {
                        _totalvalue = double.Parse(estimate.TotalVariation.ToString());
                    }

                    if (estimate.CustomerDocumentName.ToUpper().Contains("VARIATION") || estimate.CustomerDocumentName.ToUpper().Contains("CONTRACT"))
                    {
                        string[] temp = estimate.CustomerDocumentName.Split(' ');
                        if (estimate.CustomerDocumentName.ToUpper().Contains("VARIATION"))
                        {
                            if (temp.Length > 0)
                            {
                                _customerdocumenttype = temp[0].ToUpper();
                            }
                            if (temp.Length > 1)
                            {
                                _customerdocumentnumber = temp[1];
                            }
                        }
                        else
                        {
                            _customerdocumenttype = estimate.CustomerDocumentName;
                        }
                        if (_statusId == 2)
                        {
                            if (estimate.CustomerDocumentName.ToUpper().Contains("VARIATION"))
                            {
                                lblvariation.Visibility = Visibility.Visible;
                                txtvariationsummary.Visibility = Visibility.Visible;
                                txtvariationsummary.Text = estimate.CustomerDocumentDesc;
                                _summary = estimate.CustomerDocumentDesc;
                            }

                        }
                    }
                }
                else
                {
                    lblTotal.Visibility = Visibility.Collapsed;
                    txtSignedTotal.Visibility = Visibility.Collapsed;
                }

                if (_createNewRevision)
                {
                    // Populate existing comments to allow user to append the comments
                    _comments = estimate.Comments != null ? estimate.Comments : string.Empty;
                    txtComments.Text = _comments;
                }

                switch (_statusId)
                {
                    case 1:
                        txtReason.Text = "Activating Estimate " + estimate.EstimateId.ToString();
                        this.Title = "Activating Estimate " + estimate.EstimateId.ToString();
                        txtRevision.Visibility = Visibility.Visible;
                        cmbRevision.Visibility = Visibility.Visible;
                        break;

                    case 2:
                        txtReason.Text = "Type of Acceptance ";
                        this.Title = "Completing Estimate " + estimate.EstimateId.ToString();
                        txtRevision.Visibility = Visibility.Visible;
                        cmbRevision.Visibility = Visibility.Visible;

                        if (estimate.RevisionTypeId == 2) // STS revision type
                            chkCustomerNotification.Visibility = System.Windows.Visibility.Visible;
                        break;

                    case 3:
                        txtReason.Text = "Rejection Reason ";
                        this.Title = "Rejecting Estimate " + estimate.EstimateId.ToString();
                        txtRevision.Visibility = Visibility.Visible;
                        cmbRevision.Visibility = Visibility.Visible;
                        break;

                    case 4:
                        txtReason.Text = "On Hold Reason ";
                        this.Title = "Holding Estimate " + estimate.EstimateId.ToString();

                        break;

                    case 5:
                        txtReason.Text = "Cancel Reason ";
                        this.Title = "Cancelling Estimate " + estimate.EstimateId.ToString();

                        break;

                    default:
                        break;
                }

                PopulateStatusReasons(_statusId, estimate.RevisionTypeId);
            }
            else
            {
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetEstimateHeaderCompleted");
                BusyIndicator1.IsBusy = false;
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            double totalsignedvalue;
            string genericmessage = "Customer signed total is mandatory.";
            if (cmbRevision.SelectedIndex == 0 && cmbRevision.Items.Count > 1)
            {
                DialogParameters param = new DialogParameters();
                param.Header = "Next Revision Type is required";
                param.Content = "Please specify Next Revision Type";
                RadWindow.Alert(param);
                return;
            }

            if (!(cmbReason.SelectedIndex > 0 || cmbReason.Items.Count == 1))
            {
                DialogParameters param = new DialogParameters();
                param.Header = txtReason.Text + "is required";
                param.Content = "Please specify " + txtReason.Text;
                RadWindow.Alert(param);
                return;
            }

            if (txtSignedTotal.Visibility == Visibility.Visible)
            {
                if (txtSignedTotal.Text.Trim() == "")
                {
                    DialogParameters param = new DialogParameters();
                    param.Header = "Alert";
                    param.Content = genericmessage;
                    RadWindow.Alert(param);
                    return;
                }
                else
                {
                    try
                    {
                        totalsignedvalue = double.Parse(txtSignedTotal.Text);
                        if (totalsignedvalue == _totalvalue)
                        {
                            BusyIndicator1.IsBusy = true;
                            BusyIndicator1.BusyContent = "Saving Estimate...";

                            if (_statusId != 3) // Complete, cancel, on hold, activate
                            {
                                _mrsClient.ValidateSetEstimateStatusCompleted += new EventHandler<ValidateSetEstimateStatusCompletedEventArgs>(_mrsClient_ValidateSetEstimateStatusCompleted);
                                _mrsClient.ValidateSetEstimateStatusAsync(_estimateRevisionId, Convert.ToInt32(cmbRevision.SelectedValue));
                            }
                            else // Reject
                            {
                                _mrsClient.RejectVariationCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(_mrsClient_RejectVariationCompleted);
                                _mrsClient.RejectVariationAsync(_estimateRevisionId, (App.Current as App).CurrentUserId);
                            }
                        }
                        else
                        {
                            DialogParameters param = new DialogParameters();
                            param.Header = "Alert";
                            if (_customerdocumenttype.ToUpper().Contains("VARIATION"))
                            {
                                param.Content = "Customer signed total " + totalsignedvalue.ToString("C", CultureInfo.CurrentCulture) + " does not match variation value " + _totalvalue.ToString("C", CultureInfo.CurrentCulture) + "." + Environment.NewLine + "Please rectify.";
                            }
                            else
                            {
                                param.Content = "Customer signed total " + totalsignedvalue.ToString("C", CultureInfo.CurrentCulture) + " does not match contract value " + _totalvalue.ToString("C", CultureInfo.CurrentCulture) + "." + Environment.NewLine + "Please rectify.";
                            }

                            //RadWindow.Alert(param);
                            RadWindow.Alert(new TextBlock() { Text = param.Content.ToString(), TextWrapping = TextWrapping.Wrap, Width = 250 });
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        DialogParameters param = new DialogParameters();
                        param.Header = "Alert";
                        param.Content = genericmessage;
                        RadWindow.Alert(param);
                        return;
                    }

                }

            }
            else
            {
                // when TBA selected for pc, contract, variation then accepted date must be set, raise error otherwise.
                // this is an incorrect logic, this is already taken care on the stored procedure validate SP
                //if (Convert.ToInt32(cmbRevision.SelectedValue) == 0)
                //{
                //    DialogParameters param = new DialogParameters();
                //    param.Header = "Accepted date is required";
                //    param.Content = "Please specify Accepted Date before completing this revision.";
                //    RadWindow.Alert(param);
                //    return;
                //}

                BusyIndicator1.IsBusy = true;
                BusyIndicator1.BusyContent = "Saving Estimate...";

                if (_statusId != 3) // Complete, cancel, on hold, activate
                {
                    _mrsClient.ValidateSetEstimateStatusCompleted += new EventHandler<ValidateSetEstimateStatusCompletedEventArgs>(_mrsClient_ValidateSetEstimateStatusCompleted);
                    _mrsClient.ValidateSetEstimateStatusAsync(_estimateRevisionId, Convert.ToInt32(cmbRevision.SelectedValue));
                }
                else // Reject
                {
                    _mrsClient.RejectVariationCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(_mrsClient_RejectVariationCompleted);
                    _mrsClient.RejectVariationAsync(_estimateRevisionId, (App.Current as App).CurrentUserId);
                }
            }
        }

        void _mrsClient_RejectVariationCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            _mrsClient.RejectVariationCompleted -= new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(_mrsClient_RejectVariationCompleted);

            if (e.Error == null)
            {
                // If the Comments has been modified
                if (_comments != txtComments.Text && _createNewRevision)
                {
                    _mrsClient.UpdateCommentCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(mrsClient_UpdateCommentCompleted);
                    _mrsClient.UpdateCommentAsync(_estimateRevisionId, txtComments.Text, (App.Current as App).CurrentUserId, int.Parse(_customerdocumentnumber), txtvariationsummary.Text);
                }
                else
                {
                    CreateLog();
                }
            }
            else
            {
                ExceptionHandler.PopUpErrorMessage(e.Error, "RejectVariationCompleted");
                BusyIndicator1.IsBusy = false;
            }
        }

        void _mrsClient_ValidateSetEstimateStatusCompleted(object sender, ValidateSetEstimateStatusCompletedEventArgs e)
        {
            _mrsClient.ValidateSetEstimateStatusCompleted -= new EventHandler<ValidateSetEstimateStatusCompletedEventArgs>(_mrsClient_ValidateSetEstimateStatusCompleted);

            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    BusyIndicator1.IsBusy = false;

                    DialogParameters param = new DialogParameters();
                    param.Header = "Validation Error";
                    param.Content = e.Result;
                    RadWindow.Alert(param);                    
                }
                else
                {
                    // If the Comments has been modified
                    if ((_comments != txtComments.Text || _summary!= txtvariationsummary.Text) && _createNewRevision)
                    {
                        _mrsClient.UpdateCommentCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(mrsClient_UpdateCommentCompleted);
                        _mrsClient.UpdateCommentAsync(_estimateRevisionId, txtComments.Text, (App.Current as App).CurrentUserId, int.Parse(_customerdocumentnumber), txtvariationsummary.Text);
                    }
                    else
                    {
                        CreateLog();
                    }
                }
            }
            else
            {
                ExceptionHandler.PopUpErrorMessage(e.Error, "ValidateSetEstimateStatusCompleted");
                BusyIndicator1.IsBusy = false;
            }
        }

        void mrsClient_UpdateCommentCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            _mrsClient.UpdateCommentCompleted -= new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(mrsClient_UpdateCommentCompleted);

            if (e.Error == null)
                CreateLog();
            else
            {
                ExceptionHandler.PopUpErrorMessage(e.Error, "UpdateCommentCompleted");
                BusyIndicator1.IsBusy = false;
            }
        }

        void mrsClient_SetContractStatusCompleted(object sender, SetContractStatusCompletedEventArgs e)
        {
            BusyIndicator1.IsBusy = false;
            if (e.Error == null)
            {
                if (!e.Result)
                {
                    DialogParameters param = new DialogParameters();
                    param.Header = "Update failed";
                    param.Content = "Update failed!\r\nPlease contact the System Administrator.";
                    RadWindow.Alert(param);
                }
                else
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
                ExceptionHandler.PopUpErrorMessage(e.Error, "SetContractStatusCompleted");

            
        }

        void mrsClient_CompleteEstimateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            _mrsClient.CompleteEstimateCompleted -= new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(mrsClient_CompleteEstimateCompleted);

            BusyIndicator1.IsBusy = false;

            if (e.Error == null)
            {
                RadWindow window = this.ParentOfType<RadWindow>();
                if (window != null)
                {
                    window.DialogResult = true;
                    window.Close();
                }

                /* Commented on 20130722 - It causes error intermittently when complete/reject estimates 
                _mrsClient.SynchroniseCustomerDetailsCompleted += delegate(object o, SynchroniseCustomerDetailsCompletedEventArgs es)
                {
                    BusyIndicator1.IsBusy = false;

                    if (es.Error == null)
                    {
                        RadWindow window = this.ParentOfType<RadWindow>();
                        if (window != null)
                        {
                            window.DialogResult = true;
                            window.Close();
                        }
                    }
                    else
                        ExceptionHandler.PopUpErrorMessage(es.Error, "SynchronizeCustomerDetails");

                };

                _mrsClient.SynchroniseCustomerDetailsAsync(Convert.ToInt32(_estimatenumber));
                */ 
            }
            else
            {    
                //ExceptionHandler.PopUpErrorMessage(e.Error, "CompleteEstimateCompleted");
                //BusyIndicator1.IsBusy = false;

            }
        }

        void mrsClient_CreateTaskForContractCompleted(object sender, CreateTaskForContractCompletedEventArgs e)
        {
            _mrsClient.CreateTaskForContractCompleted -= new EventHandler<CreateTaskForContractCompletedEventArgs>(mrsClient_CreateTaskForContractCompleted);

            if (e.Error == null)
            {

                if (!e.Result)
                {
                    DialogParameters param = new DialogParameters();
                    param.Header = "Create Task Failed";
                    param.Content = "Create Task failed!\r\nPlease contact the System Administrator.";
                    RadWindow.Alert(param);
                    BusyIndicator1.IsBusy = false;
                }
                else 
                    CompleteEstimate();
            }
            else
            {
                ExceptionHandler.PopUpErrorMessage(e.Error, "CreateTaskForContractCompleted");
                BusyIndicator1.IsBusy = false;
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

        private void CreateLog()
        {
            int reasoncode = 0;
            string comments = string.Empty;
            string nextuser = "";
            if (cmbNextUser.Visibility == Visibility.Visible)
            {
                nextuser = cmbNextUser.Text;
                _ownerId =    int.Parse(cmbNextUser.SelectedValue.ToString());
            }


            if (!_createNewRevision)
            {
                reasoncode = Convert.ToInt32(cmbReason.SelectedValue);
                comments = txtComments.Text;
            }

            if (nextuser.Trim() != "")
            {
                comments = comments + " Completed and assigned to " + nextuser;
            }

            switch (_statusId)
            {
                // Completed
                case 2:
                    _logAction = MRSLogAction.Accept;
                    break;

                // Rejected
                case 3:
                    _logAction = MRSLogAction.Reject;
                    break;

                // Reactivate (Work In Progress)
                case 1:
                    _logAction = MRSLogAction.Reactivate;
                    break;

                // On Hold
                case 4:
                    _logAction = MRSLogAction.OnHold;
                    break;

                // Cancel
                case 5:
                    _logAction = MRSLogAction.Cancel;
                    break;

                default:
                    break;
            }

            _mrsClient.CreateSalesEstimateLogCompleted += new EventHandler<CreateSalesEstimateLogCompletedEventArgs>(mrsClient_CreateSalesEstimateLogCompleted);
            _mrsClient.CreateSalesEstimateLogAsync(
                (App.Current as App).CurrentUserLoginName,
                _logAction,
                _estimateRevisionId,
                comments,
                reasoncode);
        }

        void win_ClosedAndChangeFilter(object sender, WindowClosedEventArgs e)
        {
            RadWindow dlg = (RadWindow)sender;
            bool? result = dlg.DialogResult;
            if (result.HasValue && result.Value)
            {
                Frame parentFrame = (Frame)this.Parent.ParentOfType<Frame>();
                if (parentFrame != null)
                {
                    parentFrame.Refresh();
                }
            }
            BusyIndicator1.IsBusy = false;
        }

        private void CompleteEstimate()
        {
            switch (_statusId)
            {
                // Completed
                case 2:
                    //if (Convert.ToInt32(cmbRevision.SelectedValue) == 6)
                    //{
                    //    // Ready for Split Studio M revisions
                    //    CreateStudioMSplit draftDlg = new CreateStudioMSplit(_estimateRevisionId, 6);
                    //    RadWindow win = new RadWindow();
                    //    win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                    //    win.Header = "Ready for Studio M Split Revisions";
                    //    win.Content = draftDlg;
                    //    win.Closed += new EventHandler<WindowClosedEventArgs>(win_ClosedAndChangeFilter);
                    //    win.ShowDialog();
                    //    BusyIndicator1.IsBusy = false;
                    //    RadWindow window = this.ParentOfType<RadWindow>();
                    //    if (window != null)
                    //    {
                    //        window.DialogResult = false;
                    //        window.Close();
                    //    }
                    //}
                    //else
                    //{ 
                        _mrsClient.CompleteEstimateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(mrsClient_CompleteEstimateCompleted);
                        _mrsClient.CompleteEstimateAsync(_estimateRevisionId, (App.Current as App).CurrentUserId, _statusId, Convert.ToInt32(cmbReason.SelectedValue), Convert.ToInt32(cmbRevision.SelectedValue), _ownerId);
                    //}
                    break;

                // Rejected
                case 3:
                    _mrsClient.CompleteEstimateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(mrsClient_CompleteEstimateCompleted);
                    _mrsClient.CompleteEstimateAsync(_estimateRevisionId, (App.Current as App).CurrentUserId, _statusId, Convert.ToInt32(cmbReason.SelectedValue), Convert.ToInt32(cmbRevision.SelectedValue), _ownerId);
                    break;

                // Reactivate (Work In Progress)
                case 1:
                    _mrsClient.SetContractStatusCompleted += new EventHandler<SetContractStatusCompletedEventArgs>(mrsClient_SetContractStatusCompleted);
                    _mrsClient.SetContractStatusAsync((App.Current as App).CurrentUserLoginName, _estimateRevisionId, ContractStatus.WorkInProgress);
                    break;

                // On Hold
                case 4:
                    _mrsClient.SetContractStatusCompleted += new EventHandler<SetContractStatusCompletedEventArgs>(mrsClient_SetContractStatusCompleted);
                    _mrsClient.SetContractStatusAsync((App.Current as App).CurrentUserLoginName, _estimateRevisionId, ContractStatus.OnHold);
                    break;

                // Cancel
                case 5:
                    _mrsClient.SetContractStatusCompleted += new EventHandler<SetContractStatusCompletedEventArgs>(mrsClient_SetContractStatusCompleted);
                    _mrsClient.SetContractStatusAsync((App.Current as App).CurrentUserLoginName, _estimateRevisionId, ContractStatus.Cancelled);
                    break;

                default:
                    break;
            }
        }

        void mrsClient_CreateSalesEstimateLogCompleted(object sender, CreateSalesEstimateLogCompletedEventArgs e)
        {
            _mrsClient.CreateSalesEstimateLogCompleted -= new EventHandler<CreateSalesEstimateLogCompletedEventArgs>(mrsClient_CreateSalesEstimateLogCompleted);

            if (e.Error == null)
            {
                string subject = string.Empty;
                string desc = "";
                if (_logAction == MRSLogAction.Reject && _nextRevisionTypeId == 1 && EstimateList.SelectedContractid!="0") //Rejects to Sales Consultant
                {
                    subject = @"Estimate " + _estimatenumber + " V" + _revisionnumner + " has been rejected!";
                    desc = (App.Current as App).CurrentUserFullName + @" has rejected estimate " + _estimatenumber + " (Revision " + _revisionnumner + @") and have requested you rectify as per comments below.<br>
                         <b>Details:</b><br>
                         Customer Name: " + estimate.CustomerName + @"<br>
                         Contract No: " + estimate.ContractNumber + @"<br>
                         Estimate No: " + _estimatenumber + @"<br>
                         Revision No: " + _revisionnumner + @"<br>
                         Rejected By: " + (App.Current as App).CurrentUserFullName + @"<br>
                         Comments: " + txtComments.Text+ @"<br><br>
                         <b>Action:</b><br>
                         Please communicate above comments to customer.<br><br>
                         <b>Note:</b><br>
                         You can always view completed estimate in system by visiting http://metriconretail<br><br>
                         Kind Regards,<br><br>
                         Metricon Homes";

                    _mrsClient.CreateTaskForContractCompleted += new EventHandler<CreateTaskForContractCompletedEventArgs>(mrsClient_CreateTaskForContractCompleted);
                    _mrsClient.CreateTaskForContractAsync(EstimateList.SelectedContractid, EstimateList.SelectedEstimateRevisionId, subject, DateTime.Now.AddDays(2), "Rejected", desc);

                    // send email to SC to notify rejection
                    //_mrsClient.SendNotificationEmailCompleted += delegate(object o,  SendNotificationEmailCompletedEventArgs es)
                    //{

                    //    if (es.Error == null)
                    //    {

                    //    }
                    //    else
                    //        ExceptionHandler.PopUpErrorMessage(es.Error, "SendNotificationEmail");

                    //};

                    //_mrsClient.SendNotificationEmailAsync(EstimateList.SelectedContractid,estimate.SalesAcceptor, estimate.SalesConsultantName, estimate.ContractNumber.ToString(), "new_contract", "REJECTED", _estimatenumber, _revisionnumner, txtComments.Text);
                }
                else if (_logAction == MRSLogAction.Accept && chkCustomerNotification.IsChecked == true && _nextRevisionTypeId == 1 && EstimateList.SelectedContractid != "0")
                {
                    subject = @"Customer Notification is required for Estimate " + _estimatenumber + " V" + _revisionnumner;
                    desc = (App.Current as App).CurrentUserFullName + @"  has completed estimate " + _estimatenumber + " (Revision " + _revisionnumner + @") and have requested you  to notify customer of following. Completed estimate is also attached for your reference.<br>
                         <b>Details:</b><br>
                         Customer Name: " + estimate.CustomerName + @"<br>
                         Contract No: " + estimate.ContractNumber + @"<br>
                         Estimate No: " + _estimatenumber + @"<br>
                         Revision No: " + _revisionnumner + @"<br>
                         Completed By: " + (App.Current as App).CurrentUserFullName + @"<br>
                         Comments: " + txtComments.Text + @"<br><br>
                         <b>Action:</b><br>
                         Please communicate above comments to customer.<br><br>
                         <b>Note:</b><br>
                         You can always view completed estimate in system by visiting http://metriconretail<br><br>
                         Kind Regards,<br><br>
                         Metricon Homes";

                    _mrsClient.CreateTaskForContractCompleted += new EventHandler<CreateTaskForContractCompletedEventArgs>(mrsClient_CreateTaskForContractCompleted);
                    _mrsClient.CreateTaskForContractAsync(EstimateList.SelectedContractid, EstimateList.SelectedEstimateRevisionId, subject, DateTime.Now.AddDays(1), "Completed", desc);

                    // send email to SC to notify rejection
                    //_mrsClient.SendNotificationEmailCompleted += delegate(object o, SendNotificationEmailCompletedEventArgs es)
                    //{

                    //    if (es.Error == null)
                    //    {

                    //    }
                    //    else
                    //        ExceptionHandler.PopUpErrorMessage(es.Error, "SendNotificationEmail");

                    //};

                    //_mrsClient.SendNotificationEmailAsync(EstimateList.SelectedContractid, estimate.SalesAcceptor, estimate.SalesConsultantName, estimate.ContractNumber.ToString(), "new_contract", "COMPLETED", _estimatenumber, _revisionnumner, txtComments.Text);
                }
                else if (_logAction == MRSLogAction.Reject && _nextRevisionTypeId == 5 && _ownerId > 0 && EstimateList.SelectedContractid != "0") //Rejects to CSC
                {
                    subject = @"Estimate " + _estimatenumber + " V" + _revisionnumner + " has been rejected!";
                    desc = (App.Current as App).CurrentUserFullName + @" has rejected estimate " + _estimatenumber + " (Revision " + _revisionnumner + @") and have requested you rectify as per comments below.<br>
                         <b>Details:</b><br>
                         Customer Name: " + estimate.CustomerName + @"<br>
                         Contract No: " + estimate.ContractNumber + @"<br>
                         Estimate No: " + _estimatenumber + @"<br>
                         Revision No: " + _revisionnumner + @"<br>
                         Rejected By: " + (App.Current as App).CurrentUserFullName + @"<br>
                         Comments: " + txtComments.Text + @"<br><br>
                         <b>Action:</b><br>
                         Please communicate above comments to customer.<br><br>
                         <b>Note:</b><br>
                         You can always view completed estimate in system by visiting http://metriconretail<br><br>
                         Kind Regards,<br><br>
                         Metricon Homes";

                    _mrsClient.SendCrmEmailCompleted += new EventHandler<SendCrmEmailCompletedEventArgs>(_mrsClient_SendCrmEmailCompleted);
                    _mrsClient.SendCrmEmailAsync(new Guid(EstimateList.SelectedContractid), _ownerId, subject, desc);
                }
                else if (_logAction == MRSLogAction.Accept && _ownerId > 0 && EstimateList.SelectedContractid != "0") //Assign automatically to existing user
                {
                    subject = @"Estimate " + _estimatenumber + " V" + _revisionnumner + " has been assigned to you.";
                    desc = (App.Current as App).CurrentUserFullName + @" has completed estimate " + _estimatenumber + " (Revision " + _revisionnumner + @") and the MRS automatically assigned it to you.<br>
                         <b>Details:</b><br>
                         Customer Name: " + estimate.CustomerName + @"<br>
                         Contract No: " + estimate.ContractNumber + @"<br>
                         Estimate No: " + _estimatenumber + @"<br>
                         Revision No: " + _revisionnumner + @"<br>
                         Completed By: " + (App.Current as App).CurrentUserFullName + @"<br>
                         Comments: " + txtComments.Text + @"<br><br>
                         <b>Action:</b><br>
                         Please work on the assigned estimate.<br><br>
                         <b>Note:</b><br>
                         You can always view completed estimate in system by visiting http://metriconretail<br><br>
                         Kind Regards,<br><br>
                         Metricon Homes";

                    _mrsClient.SendCrmEmailCompleted += new EventHandler<SendCrmEmailCompletedEventArgs>(_mrsClient_SendCrmEmailCompleted);
                    _mrsClient.SendCrmEmailAsync(new Guid(EstimateList.SelectedContractid), _ownerId, subject, desc);
                }
                else
                    CompleteEstimate();
            }
            else
            {
                BusyIndicator1.IsBusy = false;
                ExceptionHandler.PopUpErrorMessage(e.Error, "CreateSalesEstimateLogCompleted");
            }

        }

        void _mrsClient_SendCrmEmailCompleted(object sender, SendCrmEmailCompletedEventArgs e)
        {
            _mrsClient.SendCrmEmailCompleted -= new EventHandler<SendCrmEmailCompletedEventArgs>(_mrsClient_SendCrmEmailCompleted);
            if (e.Error == null)
            {

                if (!e.Result)
                {
                    DialogParameters param = new DialogParameters();
                    param.Header = "Send Email Failed";
                    param.Content = "Send Email failed!\r\nPlease contact the System Administrator.";
                    RadWindow.Alert(param);
                    BusyIndicator1.IsBusy = false;
                }
                else
                    CompleteEstimate();
            }
            else
            {
                ExceptionHandler.PopUpErrorMessage(e.Error, "SendCrmEmailCompleted");
                BusyIndicator1.IsBusy = false;
            }
        }

        private void cmbRevision_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangedEventArgs e)
        {
            _nextRevisionTypeId = ((sender as RadComboBox).SelectedItem as NextRevision).RevisionTypeId;
            //_ownerId = ((sender as RadComboBox).SelectedItem as NextRevision).OwnerId;

            string notes = ((sender as RadComboBox).SelectedItem as NextRevision).Notes;
            if (!string.IsNullOrEmpty(notes))
                txtNotes.Text = notes;
            else
                txtNotes.Text = string.Empty;


            if (_nextRevisionTypeId > 0)
            {
                
                txtAssignto.Visibility = Visibility.Visible;
                cmbNextUser.Visibility = Visibility.Visible;

                lblTotal.Visibility = Visibility.Collapsed;
                txtSignedTotal.Visibility = Visibility.Collapsed;

                BusyIndicator1.IsBusy = true;
                _mrsClient.GetUsersByRegionAndRevisionTypeCompleted += new EventHandler<GetUsersByRegionAndRevisionTypeCompletedEventArgs>(mrsClient_GetUsersByRegionAndRevisionTypeCompleted);
                _mrsClient.GetUsersByRegionAndRevisionTypeAsync((App.Current as App).CurrentRegionId, int.Parse(cmbRevision.SelectedValue.ToString()));
            }
            else
            {
                txtAssignto.Visibility = Visibility.Collapsed;
                cmbNextUser.Visibility = Visibility.Collapsed;

                if (_nextRevisionTypeId == 0 && _customerdocumenttype.Trim() != "")
                {
                    lblTotal.Visibility = Visibility.Visible;
                    txtSignedTotal.Visibility = Visibility.Visible;
                }
                else
                {
                    lblTotal.Visibility = Visibility.Collapsed;
                    txtSignedTotal.Visibility = Visibility.Collapsed;
                }
            }
        }

        void mrsClient_GetUsersByRegionAndRevisionTypeCompleted(object sender, GetUsersByRegionAndRevisionTypeCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                User defualtuser=  new User { FullName = "Select", RegionId = 0, UserId = 0 };
                ObservableCollection<User> owners = e.Result;
                bool exists = cmbNextUser.Items.Any(item => (item as User).UserId == 0);
                if (!exists)
                {
                    owners.Insert(0, defualtuser);
                }
                cmbNextUser.ItemsSource = owners;
                cmbNextUser.SelectedIndex = 0;
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetUsersByRegionAndRevisionTypeCompleted");

            BusyIndicator1.IsBusy = false;
        }
    }
}

