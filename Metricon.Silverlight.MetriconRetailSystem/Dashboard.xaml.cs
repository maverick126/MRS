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
using System.Windows.Navigation;

using System.Reflection;

using Metricon.Silverlight.MetriconRetailSystem.MRSService;
using Metricon.Silverlight.MetriconRetailSystem.Internal;

namespace Metricon.Silverlight.MetriconRetailSystem
{
    public partial class Dashboard : Page
    {
        private App _currentApp;
        private string revisionTypeIds; //Revision Types that a Role allow users to access
       
        public Dashboard()
        {
            InitializeComponent();

            BusyIndicator1.IsBusy = true;
            BusyIndicator2.IsBusy = true;

            _currentApp = (App)App.Current;
            _currentApp.SelectedTab = 0;

            txtVersion.Text = GetAssemblyVersion();

            GetCurrentUserLoginName();

            if (_currentApp.CurrentUserLoginName != string.Empty)
            {
                GetUser();
            }
            else
            {
                UserValidationError();
            }
        }

        // Executes when the user navigates to this page.
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            
        }

        private string GetAssemblyVersion()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            return assembly.FullName.Split(',')[1].Replace('=',' ');
        }

        private void UserValidationError()
        {
            _currentApp.UserValidationFailed = true;

            BusyIndicator1.IsBusy = false;
        }

        /// <summary>
        /// Remove domain name from user's login name
        /// </summary>
        private void GetCurrentUserLoginName()
        {
            if (_currentApp.Resources["username"] != null)
            {
                string domainLoginName = _currentApp.Resources["username"].ToString();

                if (domainLoginName.ToLower().IndexOf("methomes") >= 0)
                {
                    _currentApp.CurrentUserLoginName = domainLoginName.Substring(9);
                }
                else
                    _currentApp.CurrentUserLoginName = string.Empty;
            }
            else
                _currentApp.CurrentUserLoginName = string.Empty;

        }

        /// <summary>
        /// Get User's details from the database
        /// </summary>
        private void GetUser()
        {
            RetailSystemClient mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            mrsClient.GetCurrentUserCompleted += new EventHandler<GetCurrentUserCompletedEventArgs>(mrsClient_GetCurrentUserCompleted);
            mrsClient.GetCurrentUserAsync(_currentApp.CurrentUserLoginName);


        }

        /// <summary>
        /// Populate User's details
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void mrsClient_GetCurrentUserCompleted(object sender, GetCurrentUserCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                User currentUser = e.Result;
                if (currentUser != null)
                {
                    _currentApp.CurrentUserFullName = currentUser.FullName;
                    _currentApp.CurrentUserId = currentUser.UserId;
                    _currentApp.CurrentRegionId = currentUser.RegionId;
                    _currentApp.LoginPriceRegionId = currentUser.LoginPriceRegionId;
                    _currentApp.CurrentUserStateID = currentUser.StateId.ToString();
                    _currentApp.CurrentUserPrimaryRoleId = currentUser.PrimaryRoleId;



                    txtFullName.Text = String.Format("Welcome {0} from {1}", currentUser.FullName, currentUser.RegionName);

                    RetailSystemClient mrsClient = new RetailSystemClient();
                    mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

                    mrsClient.GetUserRolesCompleted += new EventHandler<GetUserRolesCompletedEventArgs>(mrsClient_GetUserRolesCompleted);
                    mrsClient.GetUserRolesAsync(currentUser.UserId);


                    mrsClient.GetRoleAccessModuleCompleted += new EventHandler<GetRoleAccessModuleCompletedEventArgs>(mrsClient_GetRoleAccessModuleCompleted);
                    mrsClient.GetRoleAccessModuleAsync(currentUser.PrimaryRoleId);

                }
                else
                    UserValidationError();
            }
            else
            {
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetCurrentUserCompleted");
                UserValidationError();
            }

            BusyIndicator1.IsBusy = false;
        }

        void mrsClient_GetRoleAccessModuleCompleted(object sender, GetRoleAccessModuleCompletedEventArgs e)
        {
            if (e.Error == null)
            {   
              _currentApp.CurrentRoleAccessModule = e.Result;
            }
            else
            {
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetRoleAccessModuleCompleted");
                UserValidationError();
            }
        }


        void mrsClient_GetUserRolesCompleted(object sender, GetUserRolesCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                cmbUserRole.ItemsSource = e.Result;

                if (cmbUserRole.Items.Count > 0)
                {
                    //Remove SelectionChanged EventHandler before modifying value
                    //cmbUserRole.SelectionChanged -= new Telerik.Windows.Controls.SelectionChangedEventHandler(cmbUserRole_SelectionChanged);
                    //Modify value
                    if (_currentApp.CurrentUserRoleId > 0)
                        cmbUserRole.SelectedValue = _currentApp.CurrentUserRoleId;
                    else if (_currentApp.CurrentUserPrimaryRoleId > 0)
                        cmbUserRole.SelectedValue = _currentApp.CurrentUserPrimaryRoleId;

                    if (cmbUserRole.SelectedItem == null)
                        cmbUserRole.SelectedIndex = 0;
                    //Add SelectionChanged EventHandler
                    //cmbUserRole.SelectionChanged += new Telerik.Windows.Controls.SelectionChangedEventHandler(cmbUserRole_SelectionChanged);

                    UserRole selectedRole = (UserRole)cmbUserRole.SelectedItem;

                    _currentApp.CurrentUserRoleId = selectedRole.RoleId;

                    //if (System.Windows.Browser.HtmlPage.Document.QueryString.ContainsKey("ref"))
                    //    if (System.Windows.Browser.HtmlPage.Document.QueryString["ref"] == "SQS")
                    //        this.NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                }
                else
                    UserValidationError();
            }
            else
            {
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetUserRolesCompleted");
                UserValidationError();
            }
        }

        private void cmbUserRole_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (!BusyIndicator2.IsBusy)
                BusyIndicator2.IsBusy = true;

            UserRole selectedRole = (UserRole)cmbUserRole.SelectedItem;

            _currentApp.CurrentUserRoleId = selectedRole.RoleId;
            _currentApp.IsManager = selectedRole.IsManager;

            //Get Alert Message
            RetailSystemClient mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            mrsClient.GetEstimateCountCompleted += new EventHandler<GetEstimateCountCompletedEventArgs>(mrsClient_GetEstimateCountCompleted);
            mrsClient.GetEstimateCountAsync(_currentApp.CurrentUserId, _currentApp.CurrentUserRoleId);

            mrsClient.GetRoleAccessModuleCompleted += new EventHandler<GetRoleAccessModuleCompletedEventArgs>(mrsClient_GetRoleAccessModuleCompleted);
            mrsClient.GetRoleAccessModuleAsync(_currentApp.CurrentUserRoleId);
        }

        void mrsClient_GetEstimateCountCompleted(object sender, GetEstimateCountCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                System.Collections.ObjectModel.ObservableCollection<int> estimateCount = e.Result;

                HlnkNewJobs.Content = estimateCount[0].ToString() + " New Job";
                if (estimateCount[0] > 1)
                    HlnkNewJobs.Content += "s";

                HlnkWorkspace.Content = estimateCount[1].ToString() + " Work In Progress Job";
                if (estimateCount[1] > 1)
                    HlnkWorkspace.Content += "s";

                HlnkAccepted.Content = estimateCount[2].ToString() + " Completed Job";
                if (estimateCount[2] > 1)
                    HlnkAccepted.Content += "s";

                HlnkRejected.Content = estimateCount[3].ToString() + " Rejected Job";
                if (estimateCount[3] > 1)
                    HlnkRejected.Content += "s";

                HlnkOnHold.Content = estimateCount[4].ToString() + " On Hold Job";
                if (estimateCount[4] > 1)
                    HlnkOnHold.Content += "s";

                HlnkCancelled.Content = estimateCount[5].ToString() + " Cancelled Job";
                if (estimateCount[5] > 1)
                    HlnkCancelled.Content += "s";

                HlnkAppointment.Content = estimateCount[6].ToString() + " Appointment";
                if (estimateCount[6] > 1)
                    HlnkAppointment.Content += "s";
                HlnkAppointment.Content += " for today";

                RetailSystemClient mrsClient = new RetailSystemClient();
                mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

                mrsClient.GetRevisionTypeAccessCompleted += new EventHandler<GetRevisionTypeAccessCompletedEventArgs>(mrsClient_GetRevisionTypeAccessCompleted);
                mrsClient.GetRevisionTypeAccessAsync(_currentApp.CurrentUserRoleId);
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetEstimateCountCompleted");
        }

        void mrsClient_GetRevisionTypeAccessCompleted(object sender, GetRevisionTypeAccessCompletedEventArgs e)
        {
            revisionTypeIds = string.Empty;
            if (e.Error == null)
            {
                foreach (var item in e.Result)
                {
                    if (revisionTypeIds == string.Empty)
                        revisionTypeIds = "#" + item.RevisionTypeId.ToString() + "#";
                    else
                        revisionTypeIds += ",#" + item.RevisionTypeId.ToString() + "#";
                }

                WorkspacePanel.Visibility = System.Windows.Visibility.Visible;

                if (revisionTypeIds == "#1#") // If the selected Role allows access to Sales Consultant revision only
                {
                    NewJobsPanel.Visibility = System.Windows.Visibility.Collapsed;
                    AcceptedPanel.Visibility = System.Windows.Visibility.Collapsed;
                    RejectedPanel.Visibility = System.Windows.Visibility.Collapsed;
                    OnHoldPanel.Visibility = System.Windows.Visibility.Collapsed;
                    CancelledPanel.Visibility = System.Windows.Visibility.Collapsed;
                    AppointmentPanel.Visibility = System.Windows.Visibility.Collapsed;
                }
                
                else
                {
                    NewJobsPanel.Visibility = System.Windows.Visibility.Visible;
                    AcceptedPanel.Visibility = System.Windows.Visibility.Visible;              
                    OnHoldPanel.Visibility = System.Windows.Visibility.Visible;
                    CancelledPanel.Visibility = System.Windows.Visibility.Visible;

                    if (revisionTypeIds.IndexOf("#2#") >= 0) // Only check resubmitted jobs when user have access to Sales Accept revision
                    {
                        RejectedPanel.Visibility = System.Windows.Visibility.Visible;

                        RetailSystemClient mrsClient = new RetailSystemClient();
                        mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

                        mrsClient.GetResubmittedEstimateCountCompleted += new EventHandler<GetResubmittedEstimateCountCompletedEventArgs>(mrsClient_GetResubmittedEstimateCountCompleted);
                        if (_currentApp.IsManager)
                            mrsClient.GetResubmittedEstimateCountAsync(0, _currentApp.CurrentRegionId);
                        else
                            mrsClient.GetResubmittedEstimateCountAsync(_currentApp.CurrentUserId, 0);
                    }
                    else
                    {
                        RejectedPanel.Visibility = System.Windows.Visibility.Collapsed;
                        AlertPanel.Visibility = System.Windows.Visibility.Collapsed;
                    }

                    // Only display appointment to Studio M roles
                    if (revisionTypeIds.IndexOf("#7#") >= 0 ||
                        revisionTypeIds.IndexOf("#8#") >= 0 ||
                        revisionTypeIds.IndexOf("#9#") >= 0 ||
                        revisionTypeIds.IndexOf("#10#") >= 0 ||
                        revisionTypeIds.IndexOf("#11#") >= 0 ||
                        revisionTypeIds.IndexOf("#12#") >= 0) 
                    {
                        AppointmentPanel.Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {
                        AppointmentPanel.Visibility = System.Windows.Visibility.Collapsed;
                    }
                }

                //RetailSystemClient mrsClient = new RetailSystemClient();
                //mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

                //mrsClient.GetOnHoldEstimateCountCompleted += new EventHandler<GetOnHoldEstimateCountCompletedEventArgs>(mrsClient_GetOnHoldEstimateCountCompleted);
                //if (_currentApp.IsManager)
                //    mrsClient.GetOnHoldEstimateCountAsync(revisionTypeIds, 0, _currentApp.CurrentRegionId);
                //else
                //    mrsClient.GetOnHoldEstimateCountAsync(revisionTypeIds, _currentApp.CurrentUserId, 0);


            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetRevisionTypeAccessCompleted");

            if (BusyIndicator2.IsBusy)
                BusyIndicator2.IsBusy = false;
        }

        //void mrsClient_GetOnHoldEstimateCountCompleted(object sender, GetOnHoldEstimateCountCompletedEventArgs e)
        //{
        //    txtAlert.Text = string.Empty;

        //    if (e.Error == null)
        //    {
        //        int onholdCount = e.Result;

        //        if (onholdCount > 0)
        //        {
        //            if (onholdCount == 1)
        //                txtAlert.Text = "You have 1 On Hold job."; 
        //            else
        //                txtAlert.Text = "You have " + onholdCount.ToString() + " On Hold jobs.";
        //        }

        //        if (revisionTypeIds.IndexOf("2") >= 0) // Only check resubmitted jobs when user have access to Sales Accept revision
        //        {
        //            RetailSystemClient mrsClient = new RetailSystemClient();
        //            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

        //            mrsClient.GetResubmittedEstimateCountCompleted += new EventHandler<GetResubmittedEstimateCountCompletedEventArgs>(mrsClient_GetResubmittedEstimateCountCompleted);
        //            if (_currentApp.IsManager)
        //                mrsClient.GetResubmittedEstimateCountAsync(0, _currentApp.CurrentRegionId);
        //            else
        //                mrsClient.GetResubmittedEstimateCountAsync(_currentApp.CurrentUserId, 0);
        //        }
        //        else
        //        {
        //            if (txtAlert.Text == string.Empty)
        //                AlertPanel.Visibility = System.Windows.Visibility.Collapsed;
        //            else
        //                AlertPanel.Visibility = System.Windows.Visibility.Visible;
        //        }
        //    }
        //    else
        //        ExceptionHandler.PopUpErrorMessage(e.Error, "GetOnHoldEstimateCountCompleted");
        //}

        void mrsClient_GetResubmittedEstimateCountCompleted(object sender, GetResubmittedEstimateCountCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                int resubmitCount = e.Result;

                if (resubmitCount > 0)
                {
                    if (txtAlert.Text != string.Empty)
                        txtAlert.Text += "\r\n";

                    if (resubmitCount == 1)
                        txtAlert.Text += "You have 1 re-submitted job in Work In Progress list.";
                    else
                        txtAlert.Text += "You have " + resubmitCount.ToString() + " re-submitted jobs in Work In Progress list.";
                }
                else
                    AlertPanel.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetResubmittedEstimateCountCompleted");
        }

        private void ShortcutsLink_Click(object sender, RoutedEventArgs e)
        {
            HyperlinkButton btn = (HyperlinkButton)sender;
            switch (btn.Name)
            {
                case "HlnkNewJobs":
                    (App.Current as App).SelectedStatusId = (int)EstimateRevisionStatus.WorkInProgress;
                    _currentApp.SelectedTab = 0;
                    break;
                case "HlnkWorkspace":
                    (App.Current as App).SelectedStatusId = (int)EstimateRevisionStatus.WorkInProgress;
                    _currentApp.SelectedTab = 1;
                    break;
                case "HlnkAppointment":
                    // If user is either Sales Manager or Sales Consultant (They can only see 1 tab)
                    if (_currentApp.CurrentUserRoleId == 2 || _currentApp.CurrentUserRoleId == 3)
                        _currentApp.SelectedTab = 0;
                    else
                        _currentApp.SelectedTab = 1;
                    break;
                case "HlnkAccepted":
                    //_currentApp.SelectedTab = 2;
                    (App.Current as App).SelectedStatusId = (int)EstimateRevisionStatus.Accepted;
                    _currentApp.SelectedTab = 1;
                    break;
                case "HlnkRejected":
                    //_currentApp.SelectedTab = 3;
                    (App.Current as App).SelectedStatusId = (int)EstimateRevisionStatus.Rejected;
                    _currentApp.SelectedTab = 1;
                    break;
                case "HlnkOnHold":
                    //_currentApp.SelectedTab = 4;
                    (App.Current as App).SelectedStatusId = (int)EstimateRevisionStatus.OnHold;
                    _currentApp.SelectedTab = 1;
                    break;
                case "HlnkCancelled":
                    //_currentApp.SelectedTab = 5;
                    (App.Current as App).SelectedStatusId = (int)EstimateRevisionStatus.Cancelled;
                    _currentApp.SelectedTab = 1;
                    break; 
                default:
                    _currentApp.SelectedTab = 0;
                    break; 
            }

            this.NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
        }

        private void LinkCloseAlert_Click(object sender, RoutedEventArgs e)
        {
            AlertPanel.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void buttonOnlinePriceBook_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/OnlinePriceBook.xaml", UriKind.Relative));
        }
    }
}
