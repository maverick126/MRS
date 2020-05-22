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
using Metricon.Silverlight.MetriconRetailSystem.MRSService;

namespace Metricon.Silverlight.MetriconRetailSystem.ChildWindows
{
    public partial class CreateRevisionConfirmation : ChildWindow
    {
        private int _revisionTypeId;
        private int _estimateRevisionId;
        private RetailSystemClient _mrsClient;

        public CreateRevisionConfirmation(int estimateRevisionId, int revisionTypeId)
        {
            InitializeComponent();

            _mrsClient = new RetailSystemClient();
            _mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            _estimateRevisionId = estimateRevisionId;
            _revisionTypeId = revisionTypeId;

            //if (_revisionTypeId == 6) //Contract Draft
            //    txtMessage.Text = "Do you really want to create a Contract Draft?";
            //else if (_revisionTypeId == 13) //Final Contract
            //{
            //    txtMessage.Text = "Validating Estimate...";
            //    OKButton.Visibility = System.Windows.Visibility.Collapsed;
            //    CancelButton.Visibility = System.Windows.Visibility.Collapsed;

            //    _mrsClient.ValidateFinalContractCompleted += new EventHandler<ValidateFinalContractCompletedEventArgs>(_mrsClient_ValidateFinalContractCompleted);
            //    _mrsClient.ValidateFinalContractAsync(estimateRevisionId);
            //}
            if (_revisionTypeId == 14 || _revisionTypeId == 24)
                txtMessage.Text = "Do you really want to create a Pre Site Variation?";
            else if (_revisionTypeId == 18)
                txtMessage.Text = "Do you really want to create a Building Variation?";
            else if (_revisionTypeId == 5)
                txtMessage.Text = "Do you really want to create a CSC Revision?";
            else if (_revisionTypeId == 0)
                txtMessage.Text = "Do you really want to skip Single Studio M Revision?";
            else if (_revisionTypeId == 23)
            {
                txtMessage.Text = "Validating Studio M Answers...";
                OKButton.Visibility = System.Windows.Visibility.Collapsed;
                CancelButton.Visibility = System.Windows.Visibility.Collapsed;

                _mrsClient.ValidateStudioMRevisionsCompleted += new EventHandler<ValidateStudioMRevisionsCompletedEventArgs>(_mrsClient_ValidateStudioMRevisionsCompleted);
                _mrsClient.ValidateStudioMRevisionsAsync(_estimateRevisionId);
            }
                
        }

        void _mrsClient_ValidateStudioMRevisionsCompleted(object sender, ValidateStudioMRevisionsCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result.Count > 0)
                {
                    List<ValidationErrorMessage> result = new List<ValidationErrorMessage>();
                    foreach (ValidationErrorMessage s in e.Result)
                    {
                        result.Add(s);
                    }

                    RadWindow win = new RadWindow();
                    ShowValidationMessage messageDlg = new ShowValidationMessage(result, false, _estimateRevisionId);
                    win.Header = "Validation Error Message";
                    win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                    win.Content = messageDlg;
                    win.ShowDialog();

                    RadWindow window = this.ParentOfType<RadWindow>();
                    if (window != null)
                    {
                        window.DialogResult = true;
                        window.Close();
                    }
                }
                else
                {
                    txtMessage.Text = "Do you really want to merge all Studio M revisions?";
                    OKButton.Visibility = System.Windows.Visibility.Visible;
                    CancelButton.Visibility = System.Windows.Visibility.Visible;
                }

            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "ValidateStudioMRevisionsCompleted");
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            //if (_revisionTypeId == 6) //Contract Draft
            //{
            //    BusyIndicator1.IsBusy = true;
            //    BusyIndicator1.BusyContent = "Creating Contract Draft...";

            //    _mrsClient.CreateContractDraftCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(mrsClient_CreateContractDraftCompleted);
            //    _mrsClient.CreateContractDraftAsync(_estimateRevisionId, (App.Current as App).CurrentUserId);
            //}
            //else if (_revisionTypeId == 13) //Final Contract
            //{
            //    BusyIndicator1.IsBusy = true;
            //    BusyIndicator1.BusyContent = "Creating Final Contract...";

            //    _mrsClient.CreateFinalContractCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(mrsClient_CreateFinalContractCompleted);
            //    _mrsClient.CreateFinalContractAsync(_estimateRevisionId, (App.Current as App).CurrentUserId);
            //}
            if (_revisionTypeId == 14)
            {
                BusyIndicator1.IsBusy = true;
                BusyIndicator1.BusyContent = "Creating Pre Site Variation...";

                _mrsClient.CreateVariationCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(_mrsClient_CreateVariationCompleted);
                _mrsClient.CreateVariationAsync(_estimateRevisionId, _revisionTypeId, (App.Current as App).CurrentUserId);
            }
            else if (_revisionTypeId == 18)
            {
                BusyIndicator1.IsBusy = true;
                BusyIndicator1.BusyContent = "Creating Building Variation...";

                _mrsClient.CreateVariationCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(_mrsClient_CreateVariationCompleted);
                _mrsClient.CreateVariationAsync(_estimateRevisionId, _revisionTypeId, (App.Current as App).CurrentUserId);
            }
            else if (_revisionTypeId == 5)
            {
                BusyIndicator1.IsBusy = true;
                BusyIndicator1.BusyContent = "Creating Customer Support Coordinator Revision...";

                _mrsClient.CreateCscVariationCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(_mrsClient_CreateCscVariationCompleted);
                _mrsClient.CreateCscVariationAsync(_estimateRevisionId, (App.Current as App).CurrentUserId);
            }
            else if (_revisionTypeId == 24)
            {
                BusyIndicator1.IsBusy = true;
                BusyIndicator1.BusyContent = "Creating Pre Site Variation...";

                _mrsClient.CreateVariationCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(_mrsClient_CreateVariationCompleted);
                _mrsClient.CreateVariationAsync(_estimateRevisionId, _revisionTypeId, (App.Current as App).CurrentUserId);
            }
            else if (_revisionTypeId == 0) //Skip Studio M revisions
            {
                BusyIndicator1.IsBusy = true;
                BusyIndicator1.BusyContent = "Creating After Studio M Variation...";

                _mrsClient.CreateVariationCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(_mrsClient_CreateVariationCompleted);
                _mrsClient.CreateVariationAsync(_estimateRevisionId, 23, (App.Current as App).CurrentUserId);
            }
            else if (_revisionTypeId == 23)
            {
                BusyIndicator1.IsBusy = true;
                BusyIndicator1.BusyContent = "Merging Studio M Revisions...";

                _mrsClient.MergeStudioMRevisionsCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(_mrsClient_MergeStudioMRevisionsCompleted);
                _mrsClient.MergeStudioMRevisionsAsync(_estimateRevisionId, (App.Current as App).CurrentUserId);
            }

        }

        void _mrsClient_CreateCscVariationCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            BusyIndicator1.IsBusy = false;

            if (e.Error == null)
            {
                (App.Current as App).SelectedRevisionTypeId = _revisionTypeId;
                (App.Current as App).SelectedStatusId = 1; //In Progress

                RadWindow window = this.ParentOfType<RadWindow>();
                if (window != null)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "CreateCscVariationCompleted");
        }

        void _mrsClient_MergeStudioMRevisionsCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            BusyIndicator1.IsBusy = false;

            if (e.Error == null)
            {
                (App.Current as App).SelectedRevisionTypeId = _revisionTypeId;
                (App.Current as App).SelectedStatusId = 1; //In Progress

                RadWindow window = this.ParentOfType<RadWindow>();
                if (window != null)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "MergeStudioMRevisionsCompleted");            
        }

        void _mrsClient_CreateVariationCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                (App.Current as App).SelectedRevisionTypeId = _revisionTypeId;
                (App.Current as App).SelectedStatusId = 1; //In Progress

                CreateLog();
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "CreateVariationCompleted");
        }

        private void CreateLog()
        {

            string description = "Skip RSTM by user " + (App.Current as App).CurrentUserFullName;
            RetailSystemClient MRSclient = new RetailSystemClient();
            MRSclient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            MRSclient.CreateSalesEstimateLogCompleted += new EventHandler<CreateSalesEstimateLogCompletedEventArgs>(mrsClient_CreateSalesEstimateLogCompleted);
            MRSclient.CreateSalesEstimateLogAsync(
                (App.Current as App).CurrentUserLoginName,
                MRSLogAction.SkipRSTM,
                _estimateRevisionId,
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
        //void mrsClient_CreateFinalContractCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        //{
        //    BusyIndicator1.IsBusy = false;

        //    if (e.Error == null)
        //    {
        //        (App.Current as App).SelectedRevisionTypeId = 13; //Final Contract
        //        (App.Current as App).SelectedStatusId = 2; //Completed

        //        RadWindow window = this.ParentOfType<RadWindow>();
        //        if (window != null)
        //        {
        //            window.DialogResult = true;
        //            window.Close();
        //        }
        //    }
        //    else
        //        ExceptionHandler.PopUpErrorMessage(e.Error, "CreateFinalContractCompleted");
        //}

        //void mrsClient_CreateContractDraftCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        //{
        //    BusyIndicator1.IsBusy = false;

        //    if (e.Error == null)
        //    {
        //        (App.Current as App).SelectedRevisionTypeId = 6; //Contract Draft
        //        (App.Current as App).SelectedStatusId = 2; //Completed

        //        RadWindow window = this.ParentOfType<RadWindow>();
        //        if (window != null)
        //        {
        //            window.DialogResult = true;
        //            window.Close();
        //        }
        //    }
        //    else
        //        ExceptionHandler.PopUpErrorMessage(e.Error, "CreateContractDraftCompleted");
        //}

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

