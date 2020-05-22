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

using Metricon.Silverlight.MetriconRetailSystem.MRSService;
using Metricon.Silverlight.MetriconRetailSystem.ViewModels;

namespace Metricon.Silverlight.MetriconRetailSystem
{
    public partial class MainPage : UserControl
    {       
        private App _currentApp;

        public MainPage()
        {
            InitializeComponent();

            _currentApp = (App)App.Current;

            GetCurrentUserLoginName();

            if (_currentApp.CurrentUserLoginName != string.Empty)
            {
                GetUser();
                GetStatuses();
            }
            else
            {
                UserValidationError();
            }
        }

        private void UserValidationError()
        {
            _currentApp.UserValidationFailed = true;
            txtFullName.Text = "User not found";

            //If UserValidation page is loaded, refresh it
            if (MainFrame.Source != null)
                MainFrame.Refresh();
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
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            mrsClient.GetCurrentUserCompleted += new EventHandler<GetCurrentUserCompletedEventArgs>(mrsClient_GetCurrentUserCompleted);
            mrsClient.GetCurrentUserAsync(_currentApp.CurrentUserLoginName);
        }


        private void GetStatuses()
        {
            RetailSystemClient mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            mrsClient.GetEstimateStatusesCompleted += new EventHandler<GetEstimateStatusesCompletedEventArgs>(mrsClient_GetEstimateStatusesCompleted);
            mrsClient.GetEstimateStatusesAsync();
        }

        void mrsClient_GetEstimateStatusesCompleted(object sender, GetEstimateStatusesCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                cmbStatus.ItemsSource = e.Result;

                //Remove SelectionChanged EventHandler before modifying value
                cmbStatus.SelectionChanged -= new Telerik.Windows.Controls.SelectionChangedEventHandler(cmbStatus_SelectionChanged);

                if (_currentApp.SelectedStatusId > 0)
                    cmbStatus.SelectedValue = _currentApp.SelectedStatusId;
                else
                {
                    cmbStatus.SelectedValue = (int)EstimateRevisionStatus.WorkInProgress;
                    _currentApp.SelectedStatusId = (int)EstimateRevisionStatus.WorkInProgress;
                }
                //Add SelectionChanged EventHandler
                cmbStatus.SelectionChanged += new Telerik.Windows.Controls.SelectionChangedEventHandler(cmbStatus_SelectionChanged);
            }
            else
            {
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetEstimateStatusesCompleted");
            }
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
                    txtFullName.Text = currentUser.FullName;
                    txtRegion.Text = currentUser.RegionName;

                    _currentApp.CurrentUserFullName = currentUser.FullName;
                    _currentApp.CurrentUserId = currentUser.UserId;
                    _currentApp.CurrentRegionId = currentUser.RegionId;
                    _currentApp.CurrentUserStateID = currentUser.StateId.ToString();
                    _currentApp.CurrentUserPrimaryRoleId = currentUser.PrimaryRoleId;

                    RetailSystemClient mrsClient = new RetailSystemClient();
                    mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

                    mrsClient.GetUserRolesCompleted += new EventHandler<GetUserRolesCompletedEventArgs>(mrsClient_GetUserRolesCompleted);
                    mrsClient.GetUserRolesAsync(currentUser.UserId);
                }
                else
                    UserValidationError();
            }
            else
            {
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetCurrentUserCompleted");
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
                    cmbUserRole.SelectionChanged -= new Telerik.Windows.Controls.SelectionChangedEventHandler(cmbUserRole_SelectionChanged);
                    //Modify value
                    if (_currentApp.CurrentUserRoleId > 0)
                        cmbUserRole.SelectedValue = _currentApp.CurrentUserRoleId;
                    else if (_currentApp.CurrentUserPrimaryRoleId > 0)
                        cmbUserRole.SelectedValue = _currentApp.CurrentUserPrimaryRoleId;

                    if (cmbUserRole.SelectedItem == null)
                        cmbUserRole.SelectedIndex = 0;


                    //Add SelectionChanged EventHandler
                    cmbUserRole.SelectionChanged += new Telerik.Windows.Controls.SelectionChangedEventHandler(cmbUserRole_SelectionChanged);

                    UserRole selectedRole = (UserRole)cmbUserRole.SelectedItem;

                    _currentApp.CurrentUserRoleId = selectedRole.RoleId;
                    _currentApp.IsManager = selectedRole.IsManager;



                    RetailSystemClient mrsClient = new RetailSystemClient();
                    mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

                    mrsClient.GetRevisionTypeAccessCompleted += new EventHandler<GetRevisionTypeAccessCompletedEventArgs>(mrsClient_GetRevisionTypeAccessCompleted);
                    mrsClient.GetRevisionTypeAccessAsync(selectedRole.RoleId);

                    mrsClient.GetRoleAccessModuleCompleted += new EventHandler<GetRoleAccessModuleCompletedEventArgs>(mrsClient_GetRoleAccessModuleCompleted);
                    mrsClient.GetRoleAccessModuleAsync(_currentApp.CurrentUserRoleId);
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
        void mrsClient_GetRevisionTypeAccessCompleted(object sender, GetRevisionTypeAccessCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                e.Result.Insert(0, new RevisionType() { RevisionTypeName = "All", RevisionTypeId = 0 });
                
                if (e.Result.Count > 1)
                {
                    //Remove SelectionChanged EventHandler before modifying value
                    cmbRevisionType.SelectionChanged -= new Telerik.Windows.Controls.SelectionChangedEventHandler(cmbRevisionType_SelectionChanged);

                    cmbRevisionType.ItemsSource = e.Result;
                    if (_currentApp.SelectedRevisionTypeId > 0)
                    {
                        bool revisionTypeFound = false;

                        foreach (RevisionType revisionType in e.Result)
                        {
                            if (revisionType.RevisionTypeId == _currentApp.SelectedRevisionTypeId)
                            {
                                cmbRevisionType.SelectedValue = _currentApp.SelectedRevisionTypeId;
                                revisionTypeFound = true;
                                break;
                            }
                        }

                        if (!revisionTypeFound)
                        {
                            cmbRevisionType.SelectedIndex = 0;
                            RevisionType selectedRevisionType = (RevisionType)cmbRevisionType.SelectedItem;
                            _currentApp.SelectedRevisionTypeId = selectedRevisionType.RevisionTypeId;
                        }
                    }
                    else
                    {
                        cmbRevisionType.SelectedIndex = 0;
                        RevisionType selectedRevisionType = (RevisionType)cmbRevisionType.SelectedItem;
                        _currentApp.SelectedRevisionTypeId = selectedRevisionType.RevisionTypeId;
                    }
                    //Add SelectionChanged EventHandler
                    cmbRevisionType.SelectionChanged += new Telerik.Windows.Controls.SelectionChangedEventHandler(cmbRevisionType_SelectionChanged);

                    if (MainFrame.Source != null && MainFrame.Source.OriginalString == "/EstimateList.xaml")
                        MainFrame.Refresh();
                    else
                        MainFrame.Navigate(new Uri("/EstimateList.xaml", UriKind.Relative));
                }
                else
                    UserValidationError();
            }
            else
            {
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetRevisionTypeAccessCompleted");
                UserValidationError();
            }
        }

        private void cmbUserRole_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangedEventArgs e)
        {
            MessageBoxResult confirm = MessageBox.Show("Do you really want to change your role?\r\nAll unsaved jobs will be reset.", "", MessageBoxButton.OKCancel);
            if (confirm == MessageBoxResult.OK)
            {
                UserRole selectedRole = (UserRole)cmbUserRole.SelectedItem;

                _currentApp.CurrentUserRoleId = selectedRole.RoleId;
                _currentApp.IsManager = selectedRole.IsManager;

                ResetEditEstimateUserID();

                RetailSystemClient mrsClient = new RetailSystemClient();
                mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

                mrsClient.GetRevisionTypeAccessCompleted += new EventHandler<GetRevisionTypeAccessCompletedEventArgs>(mrsClient_GetRevisionTypeAccessCompleted);
                mrsClient.GetRevisionTypeAccessAsync(selectedRole.RoleId);

                mrsClient.GetRoleAccessModuleCompleted += new EventHandler<GetRoleAccessModuleCompletedEventArgs>(mrsClient_GetRoleAccessModuleCompleted);
                mrsClient.GetRoleAccessModuleAsync(_currentApp.CurrentUserRoleId);
            }
            else
            {
                //Remove SelectionChanged EventHandler before modifying value
                cmbUserRole.SelectionChanged -= new Telerik.Windows.Controls.SelectionChangedEventHandler(cmbUserRole_SelectionChanged);
                //Modify value
                cmbUserRole.SelectedValue = _currentApp.CurrentUserRoleId;
                //Add SelectionChanged EventHandler
                cmbUserRole.SelectionChanged += new Telerik.Windows.Controls.SelectionChangedEventHandler(cmbUserRole_SelectionChanged);
            }
        }

        private void homebtn_Click(object sender, RoutedEventArgs e)
        {
            //if (MainFrame.Source != null && MainFrame.Source.OriginalString == "/EstimateList.xaml")
            //    MainFrame.Refresh();
            //else
            //    MainFrame.Navigate(new Uri("/EstimateList.xaml", UriKind.Relative));
            ResetSearchFilters();
            ResetEditEstimateUserID();

            Frame parentFrame = (Frame)this.Parent;
            parentFrame.Navigate(new Uri("/Dashboard.xaml", UriKind.Relative));
        }

        private void cmbRevisionType_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangedEventArgs e)
        {
            MessageBoxResult confirm = MessageBox.Show("Do you really want to change Role Type filter?\r\nAll unsaved jobs will be reset.", "", MessageBoxButton.OKCancel);
            if (confirm == MessageBoxResult.OK)
            {
                RevisionType selectedRevisionType = (RevisionType)cmbRevisionType.SelectedItem;

                _currentApp.SelectedRevisionTypeId = selectedRevisionType.RevisionTypeId;

                ResetEditEstimateUserID();

                if (MainFrame.Source != null && MainFrame.Source.OriginalString == "/EstimateList.xaml")
                    MainFrame.Refresh();
                else
                    MainFrame.Navigate(new Uri("/EstimateList.xaml", UriKind.Relative));
            }
            else
            {   //Change Revision Type to what it was
                //Remove SelectionChanged EventHandler before modifying value
                cmbRevisionType.SelectionChanged -= new Telerik.Windows.Controls.SelectionChangedEventHandler(cmbRevisionType_SelectionChanged);
                //Modify value
                cmbRevisionType.SelectedValue = _currentApp.SelectedRevisionTypeId;
                //Add SelectionChanged EventHandler
                cmbRevisionType.SelectionChanged += new Telerik.Windows.Controls.SelectionChangedEventHandler(cmbRevisionType_SelectionChanged);
            }
        }

        private void cmbStatus_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangedEventArgs e)
        {
            MessageBoxResult confirm = MessageBox.Show("Do you really want to change Status filter?\r\nAll unsaved jobs will be reset.", "", MessageBoxButton.OKCancel);
            if (confirm == MessageBoxResult.OK)
            {
                EstimateStatus selectedEstimateStatus = (EstimateStatus)cmbStatus.SelectedItem;

                _currentApp.SelectedStatusId = selectedEstimateStatus.StatusId;

                ResetEditEstimateUserID();

                if (MainFrame.Source != null && MainFrame.Source.OriginalString == "/EstimateList.xaml")
                {
                    MainFrame.Refresh();
                }
                else
                    MainFrame.Navigate(new Uri("/EstimateList.xaml", UriKind.Relative));
            }
            else
            {   //Change Status to what it was
                //Remove SelectionChanged EventHandler before modifying value
                cmbStatus.SelectionChanged -= new Telerik.Windows.Controls.SelectionChangedEventHandler(cmbStatus_SelectionChanged);
                //Modify value
                cmbStatus.SelectedValue = _currentApp.SelectedStatusId;
                //Add SelectionChanged EventHandler
                cmbStatus.SelectionChanged += new Telerik.Windows.Controls.SelectionChangedEventHandler(cmbStatus_SelectionChanged);
            }
        }

        private void ResetSearchFilters()
        {
            (App.Current as App).CurrentCustomerNumber = "";
            (App.Current as App).CurrentContractNumber = "";
            (App.Current as App).CurrentSelectedSalesConsultantId = 0;
            (App.Current as App).CurrentLotNumber = "";
            (App.Current as App).CurrentStreetName = "";
            (App.Current as App).CurrentSuburb = "";
            (App.Current as App).CurrentSelectedUserId = 0;
        }

        private void ResetEditEstimateUserID()
        {
            if (_currentApp.SelectedEstimateRevisionId > 0)
            {
                RetailSystemClient mrsClient = new RetailSystemClient();
                mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

                mrsClient.ResetEditEstimateUserIDCompleted += new EventHandler<ResetEditEstimateUserIDCompletedEventArgs>(mrsClient_ResetEditEstimateUserIDCompleted);
                mrsClient.ResetEditEstimateUserIDAsync(_currentApp.SelectedEstimateRevisionId, 0);
            }
        }
        void mrsClient_ResetEditEstimateUserIDCompleted(object sender, ResetEditEstimateUserIDCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                // success
                _currentApp.SelectedEstimateRevisionId = 0;
            }
            else
            {
                // error when reseting the user from edit mode
            }
        }

        private void buttonOnlinePriceBook_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Uri("/OnlinePriceBook.xaml", UriKind.Relative));
        }
    }
}
