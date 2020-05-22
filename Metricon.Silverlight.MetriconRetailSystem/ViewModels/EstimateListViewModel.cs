using System;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Collections.Generic;
using System.Windows;

using Metricon.Silverlight.MetriconRetailSystem.MRSService;
using Metricon.Silverlight.MetriconRetailSystem.Command;
using System.Linq;

namespace Metricon.Silverlight.MetriconRetailSystem.ViewModels
{
    public class EstimateListViewModel : ViewModelBase
    {
        private bool _isBusy;
        private bool _isHidden;
        private int _selectedTabIndex = -1;
        private int _selectedSalesConsultantId = 0;
        private int _selectedUserId = 0;
        private bool _ownerFilterVisible = true;
        private int _selectedRevisionTypeId = -1;

        private string _customerNumber;
        private string _contractNumber;
        private string _lotNumber;
        private string _streetName;
        private string _businessUnit;
        private string _districtCode;
        private string _opsCenterCode;
        private string _suburb;
        private int  _currentrole;
       
        public ObservableCollection<EstimateListTab> EstimateTabs { get; set; }
        public ObservableCollection<User> SalesConsultants { get; set; }
        public ObservableCollection<User> MRSUsers { get; set; }
        public ObservableCollection<GenericClassCodeName> BusinessUnitsList { get; set; }
        //public ObservableCollection<District> DistrictList { get; set; }
        //public ObservableCollection<OperatingCenter> OperatingCenterList { get; set; }

        public ObservableCollection<RevisionType> RevisionTypes { get; set; }
        public EstimateListTab NewEstimatesTab { get; set; }
        public EstimateListTab MyWorkspaceTab { get; set; }
        public EstimateListTab AcceptedEstimatesTab { get; set; }
        public EstimateListTab RejectedEstimatesTab { get; set; }
        public EstimateListTab OnHoldEstimatesTab { get; set; }
        public EstimateListTab CancelledEstimatesTab { get; set; }

        public EstimateListTab MyWorkspaceEstimatesTab { get; set; }
        public EstimateListTab SearchResultTab { get; set; } 

        private DelegateCommand _clearCommand;
        private DelegateCommand _searchCommand;

        public EstimateListViewModel()
        {
            EstimateTabs = new ObservableCollection<EstimateListTab>();
            _currentrole = (App.Current as App).CurrentUserRoleId;
            
            if (_currentrole != 3 && _currentrole != 2)
            {
                NewEstimatesTab = new EstimateListTab("New Jobs");
                EstimateTabs.Add(NewEstimatesTab);
            }
            /*
            MyWorkspaceTab = new EstimateListTab("Work In Progress");
            EstimateTabs.Add(MyWorkspaceTab);

            if (_currentrole != 3 && _currentrole != 2)
            {
                AcceptedEstimatesTab = new EstimateListTab("Completed");
                EstimateTabs.Add(AcceptedEstimatesTab);

                RejectedEstimatesTab = new EstimateListTab("Rejected");
                EstimateTabs.Add(RejectedEstimatesTab);

                OnHoldEstimatesTab = new EstimateListTab("On Hold");
                EstimateTabs.Add(OnHoldEstimatesTab);

                CancelledEstimatesTab = new EstimateListTab("Cancelled");
                EstimateTabs.Add(CancelledEstimatesTab);
            }
            */
            MyWorkspaceEstimatesTab = new EstimateListTab("My Workspace");
            EstimateTabs.Add(MyWorkspaceEstimatesTab);

            SearchResultTab = new EstimateListTab("Search Result");
            SearchResultTab.Visible = Visibility.Visible;
            EstimateTabs.Add(SearchResultTab);

            if (!IsDesignTime)
            {
                this.SalesConsultants = new ObservableCollection<User>();
                this.MRSUsers = new ObservableCollection<User>();
                this.BusinessUnitsList = new ObservableCollection<GenericClassCodeName>();
                //this.DistrictList = new ObservableCollection<District>();
                //this.OperatingCenterList = new ObservableCollection<OperatingCenter>();

                PopulateSalesConsultant();
                
                MRSUsers.Clear();
                OwnerFilterVisible = SelectedTabIndex == 1 && (App.Current as App).IsManager || _currentrole == 18 || _currentrole == 78 || _currentrole == 85;
                if (OwnerFilterVisible)
                {
                    if (MRSUsers.Count < 1)
                    {
                        if (_currentrole == 18 || _currentrole == 78 || _currentrole == 85)
                        {
                            SelectedUserId = (App.Current as App).CurrentUserId;
                        }
                    }
                }
                // RefreshTab() requires RevisionType to be populated 
                // so RefreshTab() is called or SelectedTabIndex changed is triggered in mrsClient_GetRevisionTypeAccessCompleted 
                PopulateRevisionType();               

                _searchCommand = new DelegateCommand(this.SearchEstimates);
                _clearCommand = new DelegateCommand(this.ClearSearchFilter);

                if ((App.Current as App).CurrentUserRoleId == 6 || //Estimating Manager
                    (App.Current as App).CurrentUserRoleId == 5 || //Sales Estimator
                    (App.Current as App).CurrentUserRoleId == 70)  //Sales Estimating Team Leader
                    Hidden = false;
                else
                    Hidden = true;

                PopulateDistrictsAndOpsCenter();
            }
        }

        public void RefreshTab()
        {
            IsBusy = true;
            EstimateList.IsQueue = false; // Reset it to 'false' unless the "New Jobs" tab is selected 
            //int estimateStatus = 0;
            SearchResultTab.Count = "";

            if (_currentrole != 3 && _currentrole != 2) // Not Sales Consultant or Sales Manager
            {
                switch (SelectedTabIndex)
                {
                    //new Jobs
                    case 0:
                        EstimateList.IsQueue = true;

                        //Clear Estimates in new job tab
                        NewEstimatesTab.Estimates = new ObservableCollection<EstimateGridItem>();
                        RetrieveQueuedEstimate();

                        OwnerFilterVisible = false;

                        break;

                    //My Workspace
                    case 1:
                        int roleId = (App.Current as App).CurrentUserRoleId;
                        //estimateStatus = 1;
                        //RetrieveAssignedEstimate(estimateStatus);
                        //Clear Estimates in my work space tab
                        MyWorkspaceEstimatesTab.Estimates = new ObservableCollection<EstimateGridItem>();

                        OwnerFilterVisible = (App.Current as App).IsManager || roleId == 18 || roleId == 78 || roleId == 85;

                        RetrieveEstimates();

                        break;

                    case 2:
                        SearchResultTab.Estimates = new ObservableCollection<EstimateGridItem>();

                        RetrieveSearchResultEstimate();

                        OwnerFilterVisible = false;

                        break;
  
                    default:
                        IsBusy = false;
                        break;
                }
            }
            else // Sales Consultant or Sales Manager
            { 
                //estimateStatus = 1;
                //RetrieveAssignedEstimate(estimateStatus);
                RetrieveEstimates();
            }
            (App.Current as App).CurrentCustomerNumber = CustomerNumber;
            (App.Current as App).CurrentContractNumber = ContractNumber;
            (App.Current as App).CurrentSelectedSalesConsultantId = SelectedSalesConsultantId;
            (App.Current as App).CurrentLotNumber = LotNumber;
            (App.Current as App).CurrentStreetName = StreetName;
            (App.Current as App).CurrentSuburb = Suburb;
            (App.Current as App).CurrentSelectedUserId = SelectedUserId;
        }

        private void RetrieveQueuedEstimate()
        {
            int regionId = (App.Current as App).CurrentRegionId;
            int roleId = (App.Current as App).CurrentUserRoleId;
            int revisionTypeFilter = GetRevisionTypeFilter();

            RetailSystemClient mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            mrsClient.GetQueuedEstimatesCompleted += new EventHandler<GetQueuedEstimatesCompletedEventArgs>(mrsClient_GetQueuedEstimatesCompleted);
            mrsClient.GetQueuedEstimatesAsync(revisionTypeFilter, regionId, roleId, CustomerNumber, ContractNumber, SelectedSalesConsultantId, LotNumber, StreetName, Suburb, BusinessUnit);
        }


        private void RetrieveSearchResultEstimate()
        {
            if ((CustomerNumber != null && CustomerNumber.Trim() != "") || 
                (ContractNumber != null && ContractNumber.Trim() != "") ||
                ( (SelectedSalesConsultantId != 0) ) ||
                (LotNumber != null && LotNumber.Trim() != "") ||
                (StreetName != null && StreetName.Trim() != "") ||
                (Suburb != null && Suburb.Trim() != "") 
               )
            {
                RetailSystemClient mrsClient = new RetailSystemClient();
                mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

                mrsClient.SearchSpecificJobCompleted += new EventHandler<SearchSpecificJobCompletedEventArgs>(mrsClient_SearchSpecificJobCompleted);
                mrsClient.SearchSpecificJobAsync(CustomerNumber, ContractNumber, SelectedSalesConsultantId.ToString(), LotNumber, StreetName, Suburb, BusinessUnit);
            }
            else
            {
                IsBusy = false;
            }
        }

        void mrsClient_SearchSpecificJobCompleted(object sender, SearchSpecificJobCompletedEventArgs e)
        {
            int count = 0;
            if (e.Error == null)
            {
                SearchResultTab.Estimates.Clear();
                foreach (var item in e.Result)
                {
                    count = count + 1;
                    SearchResultTab.Estimates.Add(item);
                }
                SearchResultTab.Count = "(" + count.ToString() + ")";
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "SearchSpecificJobCompleted");

            IsBusy = false;
        }
        /*
        private void RetrieveAssignedEstimate(int estimateStatus)
        {
            int regionId = (App.Current as App).CurrentRegionId;
            int roleId = (App.Current as App).CurrentUserRoleId;
            int userId = (App.Current as App).CurrentUserId;
            bool isManager = (App.Current as App).IsManager;
            int revisionTypeFilter = GetRevisionTypeFilter();

            RetailSystemClient mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            //if (isManager)
            //{
            //    mrsClient.GetAssignedEstimatesByRegionCompleted += new EventHandler<GetAssignedEstimatesByRegionCompletedEventArgs>(mrsClient_GetAssignedEstimatesByRegionCompleted);
            //    mrsClient.GetAssignedEstimatesByRegionAsync(revisionTypeFilter, roleId, estimateStatus, regionId, CustomerNumber, ContractNumber, SelectedSalesConsultantId, LotNumber, StreetName, Suburb);
            //}
            //else
            //{
            //    mrsClient.GetAssignedEstimatesByUserCompleted += new EventHandler<GetAssignedEstimatesByUserCompletedEventArgs>(mrsClient_GetAssignedEstimatesByUserCompleted);
            //    mrsClient.GetAssignedEstimatesByUserAsync(revisionTypeFilter, roleId, estimateStatus, userId, CustomerNumber, ContractNumber, SelectedSalesConsultantId, LotNumber, StreetName, Suburb);
            //}

            mrsClient.GetAssignedEstimatesCompleted += new EventHandler<GetAssignedEstimatesCompletedEventArgs>(mrsClient_GetAssignedEstimatesCompleted);
            if (isManager)
            {
                mrsClient.GetAssignedEstimatesAsync(revisionTypeFilter, roleId, estimateStatus, 0, regionId, CustomerNumber, ContractNumber, SelectedSalesConsultantId, LotNumber, StreetName, Suburb);
            }
            else
            {
                mrsClient.GetAssignedEstimatesAsync(revisionTypeFilter, roleId, estimateStatus, userId, regionId, CustomerNumber, ContractNumber, SelectedSalesConsultantId, LotNumber, StreetName, Suburb);
            }
        }*/

        private void RetrieveEstimates()
        {
            int regionId = (App.Current as App).CurrentRegionId;
            int roleId = (App.Current as App).CurrentUserRoleId;
            int userId = OwnerFilterVisible ? 0 : (App.Current as App).CurrentUserId;
            int revisionTypeFilter = (App.Current as App).SelectedRevisionTypeId;
            int estimateStatus = (App.Current as App).SelectedStatusId;

            RetailSystemClient mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            mrsClient.GetAssignedEstimatesCompleted += new EventHandler<GetAssignedEstimatesCompletedEventArgs>(mrsClient_GetAssignedEstimatesCompleted);
            //if (isManager)
            //{
                mrsClient.GetAssignedEstimatesAsync(revisionTypeFilter, roleId, estimateStatus, userId, regionId, CustomerNumber, ContractNumber, SelectedSalesConsultantId, LotNumber, StreetName, Suburb, BusinessUnit);
            //}
            //else
            //{
            //    mrsClient.GetAssignedEstimatesAsync(revisionTypeFilter, roleId, estimateStatus, userId, regionId, CustomerNumber, ContractNumber, SelectedSalesConsultantId, LotNumber, StreetName, Suburb);
            //}
        }

        void mrsClient_GetAssignedEstimatesCompleted(object sender, GetAssignedEstimatesCompletedEventArgs e)
        {
            if (e.Error == null)
            {

                if (_currentrole != 3 && _currentrole != 2)
                {
                    switch (SelectedTabIndex)
                    {

                        case 1:
                            foreach (var item in e.Result)
                            {
                                if (SelectedUserId == 0 || SelectedUserId == item.OwnerId)
                                {
                                    MyWorkspaceEstimatesTab.Estimates.Add(item);
                                }
                            }
                            break;
                        //case 1:
                        //    MyWorkspaceTab.Estimates.Clear();
                        //    foreach (var item in e.Result)
                        //    {
                        //        MyWorkspaceTab.Estimates.Add(item);
                        //    }
                        //    break;

                        //case 2:
                        //    AcceptedEstimatesTab.Estimates.Clear();
                        //    foreach (var item in e.Result)
                        //    {
                        //        AcceptedEstimatesTab.Estimates.Add(item);
                        //    }
                        //    break;

                        //case 3:
                        //    RejectedEstimatesTab.Estimates.Clear();
                        //    foreach (var item in e.Result)
                        //    {
                        //        RejectedEstimatesTab.Estimates.Add(item);
                        //    }
                        //    break;

                        //case 4:
                        //    OnHoldEstimatesTab.Estimates.Clear();
                        //    foreach (var item in e.Result)
                        //    {
                        //        OnHoldEstimatesTab.Estimates.Add(item);
                        //    }
                        //    break;

                        //case 5:
                        //    CancelledEstimatesTab.Estimates.Clear();
                        //    foreach (var item in e.Result)
                        //    {
                        //        CancelledEstimatesTab.Estimates.Add(item);
                        //    }
                        //    break;

                        //case 6:
                        //    MyWorkspaceEstimatesTab.Estimates.Clear();
                        //    foreach (var item in e.Result)
                        //    {
                        //        MyWorkspaceEstimatesTab.Estimates.Add(item);
                        //    }
                        //    break;

                        default:
                            break;
                    }
                }
                else
                {
                    /*
                    MyWorkspaceTab.Estimates.Clear();
                    foreach (var item in e.Result)
                    {
                        MyWorkspaceTab.Estimates.Add(item);
                    }*/

                    MyWorkspaceEstimatesTab.Estimates.Clear();
                    foreach (var item in e.Result)
                    {
                        if (SelectedUserId == 0 || SelectedUserId == item.OwnerId)
                        {
                            MyWorkspaceEstimatesTab.Estimates.Add(item);
                        }
                    }

                }
                if (OwnerFilterVisible)
                {
                    if (MRSUsers.Count < 1)
                    {
                        User userItem = null;
                        List<EstimateGridItem> listMRSUsers = e.Result.OrderBy(o => o.OwnerName).ToList();
                        MRSUsers.Add(new User() { FullName = "All Users", UserId = 0 });                        
                        foreach (var item in listMRSUsers)
                        {
                            userItem = new User() { FullName = item.OwnerName, UserId = item.OwnerId };
                            if (!MRSUsers.Any(p => p.UserId == userItem.UserId))
                            {
                                MRSUsers.Add(userItem);
                            }
                        }
                        if (!MRSUsers.Any(p => p.UserId == SelectedUserId))
                        { 
                            userItem = new User() { FullName = (App.Current as App).CurrentUserFullName, UserId = SelectedUserId };
                            MRSUsers.Add(userItem);
                        }
                        // force refresh
                        int userTemp = SelectedUserId;
                        SelectedUserId = 0;
                        SelectedUserId = userTemp;
                    }
                }

                RetrieveSearchResultEstimate();
            }
            else
            {
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetAssignedEstimatesCompleted");
                IsBusy = false;
            }            
        }

        public string SetActionVisibility(object dataContext)
        {
            StringBuilder result = new StringBuilder();
            EstimateGridItem gridItem = (EstimateGridItem)dataContext;

            bool previewVisible = false;
            bool historyVisible = false;
            bool assignToMeVisible = false;
            bool assignVisible = false;
            bool editVisible = false;
            bool acceptVisible = false;
            bool rejectVisible = false;
            bool effectiveDateVisible = false;
            bool dueDateVisible = false;
            bool difficultyVisible = false;
            bool onholdVisible = false;
            bool activateVisible = false;
            bool commentsVisible = false;
            bool appointmentVisible = false;
            bool printVisible = false;
            //bool changefacadeVisible = false;
            //bool changecontracttypeVisible=false;
            bool contractCancelled = gridItem.ContractStatusName == "Cancelled" ? true : false;

            switch (SelectedTabIndex)
            {
                //new Jobs
                case 0:
                    if (_currentrole != 3 && _currentrole != 2) //NOT Sales Consultant & Sales Manager
                    {
                        previewVisible = true;
                        printVisible = true;
                        historyVisible = true;
                        assignToMeVisible = true;
                        difficultyVisible = true;

                        if ((App.Current as App).IsManager)
                        {
                            assignVisible = !contractCancelled && true;
                            dueDateVisible = true;
                        }
                    }
                    else //Sales Consultant & Sales Manager
                    {
                        previewVisible = true;
                        printVisible = true;
                        historyVisible = true;

                        if ((App.Current as App).SelectedStatusId == (int)EstimateRevisionStatus.WorkInProgress)
                        {
                            if (gridItem.OwnerId == (App.Current as App).CurrentUserId)
                            {
                                editVisible = true;
                                acceptVisible = true;
                                difficultyVisible = true;
                                commentsVisible = true;
                            }

                            if ((App.Current as App).IsManager)
                            {
                                assignVisible = !contractCancelled && true;
                                effectiveDateVisible = true;
                                dueDateVisible = true;
                            }
                        }
                    }
                    break;

                //My Workspace
                case 1:

                    switch ((App.Current as App).SelectedStatusId)
                    {
                        case (int)EstimateRevisionStatus.WorkInProgress:
                            previewVisible = true;
                            printVisible = true;
                            historyVisible = true;

                            if (gridItem.OwnerId == (App.Current as App).CurrentUserId)
                            {
                                editVisible = true;
                                acceptVisible = true;                  
                                difficultyVisible = true;
                                commentsVisible = true;
                                //undoThisRevisionVisible = true;

                                // Only Sales Accept/PVAR-CSC/BVAR-BSC/PSTM-CSC revision can be rejected
                                if (gridItem.RevisionTypeId == 2 || gridItem.RevisionTypeId == 14 || gridItem.RevisionTypeId == 18 || gridItem.RevisionTypeId == 24)
                                {
                                    rejectVisible = true;
                                }
                                //if (gridItem.RevisionTypeId == 2 || gridItem.RevisionTypeId == 4) // sales estimator & STS can change facade
                                //{
                                //    changefacadeVisible = true;
                                //}
                                //if (gridItem.RevisionTypeId >= 2 && gridItem.RevisionTypeId <= 5)
                                //{
                                //    changecontracttypeVisible = true;
                                //}
                            }

                            if ((App.Current as App).IsManager)
                            {
                                assignVisible = !contractCancelled && true;
                                //effectiveDateVisible = true;
                                //onholdVisible = true;
                                dueDateVisible = true;
                                //undoThisRevisionVisible = true;
                            }

                            if ((App.Current as App).CurrentUserRoleId != 3)//open onhold menu to everyone except SC
                            {
                                onholdVisible = true;
                            }

                            //if ((App.Current as App).CurrentUserRoleId == 57 || //Operations Manager
                            //(App.Current as App).CurrentUserRoleId == 6 ||//Estimate Manager
                            //(App.Current as App).CurrentUserRoleId == 5 ||//Sales Estimator
                            //(App.Current as App).CurrentUserRoleId == 62)//Sales Technical Support

                            //{
                            //    effectiveDateVisible = true;
                            //    if (gridItem.RevisionTypeId == 2 || gridItem.RevisionTypeId == 4)
                            //    {
                            //        changefacadeVisible = true;
                            //    }
                            //}

                            if ((gridItem.RevisionTypeId >= 7 && gridItem.RevisionTypeId <= 12) || gridItem.RevisionTypeId == 21 || gridItem.RevisionTypeId == 22) //Studio M revisions
                            {
                                //Owner can change Appointment Time
                                if (gridItem.OwnerId == (App.Current as App).CurrentUserId) 
                                    appointmentVisible = true;

                                //Manager who has access to Studio M revisions can change Appointment Time
                                else if ((App.Current as App).IsManager) 
                                    appointmentVisible = true;

                                //Colour Consultant and Customer Support Coordinator can change Appointment Time
                                else if ((App.Current as App).CurrentUserRoleId == 4 || //Colour Consultant
                                (App.Current as App).CurrentUserRoleId == 18) //Customer Support Coordinator
                                    appointmentVisible = true;
                            }

                            if (gridItem.RevisionTypeId == 5 && //CSC revision
                                gridItem.OwnerId == (App.Current as App).CurrentUserId) 
                                appointmentVisible = true;

                            break;
                        
                        case (int)EstimateRevisionStatus.Accepted:
                            previewVisible = true;
                            printVisible = true;
                            historyVisible = true;

                            //if (gridItem.OwnerId == (App.Current as App).CurrentUserId)
                            //{
                            //    undoThisRevisionVisible = true;
                            //}
                                //if (gridItem.RevisionTypeId == 6 || gridItem.RevisionTypeId == 13) //Contract
                                //{
                                //    //Owner can change Appointment Time
                                //    if (gridItem.OwnerId == (App.Current as App).CurrentUserId)
                                //        appointmentVisible = true;

                                //      //Customer Service Manager and Customer Service Coordinator can change Appointment Time
                                //    else if ((App.Current as App).CurrentUserRoleId == 22 || //Customer Service Manager
                                //    (App.Current as App).CurrentUserRoleId == 18) //Customer Service Coordinator
                                //        appointmentVisible = true;
                                //}

                                break;

                        case (int)EstimateRevisionStatus.Rejected:
                            previewVisible = true;
                            printVisible = true;
                            historyVisible = true;

                            break;

                        case (int)EstimateRevisionStatus.OnHold:
                            previewVisible = true;
                            printVisible = true;
                            historyVisible = true;
                            activateVisible = !contractCancelled && true;

                            break;

                        case (int)EstimateRevisionStatus.Cancelled:
                            previewVisible = true;
                            printVisible = true;
                            historyVisible = true;
                            activateVisible = !contractCancelled && true;

                            break;

                        default:
                            break;
                    }

                    //previewVisible = true;
                    //historyVisible = true;

                    //if (gridItem.OwnerId == (App.Current as App).CurrentUserId)
                    //{
                    //    editVisible = true;
                    //    acceptVisible = true;                  
                    //    difficultyVisible = true;
                    //    commentsVisible = true;

                    //    // Only Sales Accept revision can be rejected
                    //    if (gridItem.RevisionTypeId == 2)
                    //        rejectVisible = true;  
                    //}

                    //if ((App.Current as App).IsManager)
                    //{
                    //    assignVisible = true;
                    //    effectiveDateVisible = true;
                    //    onholdVisible = true;
                    //    dueDateVisible = true;
                    //}

                    break;

            
                //search result
                case 2:

                    historyVisible = true;
                    previewVisible = true;
                    // only sales acceptor, estimate manger and operation manager can reassign on revision sales acceopt
                    if ((_currentrole == 62 || _currentrole == 6 || _currentrole == 57) && gridItem .RevisionTypeId== 2 && gridItem.JobLocation!="SQS")  
                    {
                        if ((_currentrole == 62))
                        {
                            if (gridItem.JobLocation.ToUpper().Contains("QUEUE"))
                            {
                                assignToMeVisible = true;
                            }
                            else
                            {
                                assignToMeVisible = false;
                            }
                        }
                        else
                        {
                            assignVisible = !contractCancelled && true;
                            assignToMeVisible = false;
                        }
                    }

                    break;
                /*
                            //Rejected
                            case 3:

                                previewVisible = true;
                                historyVisible = true;

                                break;

                            //On Hold
                            case 4:

                                previewVisible = true;
                                historyVisible = true;
                                activateVisible = true;

                                break;

                            //Cancelled
                            case 5:

                                previewVisible = true;
                                historyVisible = true;
                                activateVisible = true;

                                break;
                            */

                default:
                    break;
            }

            /*
            switch (gridItem.RevisionTypeId)
            {
                case 6: //Contract Draft
                    colourVisible = true;
                    electricalVisible = true;
                    pavingVisible = true;
                    tileVisible = true;
                    deckingVisible = true;
                    carpetVisible = true;
                    finalVisible = true;
                    break;
                case 13: //Final Contract
                    pvarVisible = true;
                    bvarVisible = true;
                    break;
                default:
                    break;
            }
            */
            /*
            gridItem.PreviewVisible = previewVisible;
            gridItem.HistoryVisible = historyVisible;
            gridItem.AssignToMeVisible = assignToMeVisible;
            gridItem.AssignVisible = assignVisible;
            gridItem.EditVisible = editVisible;
            gridItem.AcceptVisible = acceptVisible;
            gridItem.RejectVisible = rejectVisible;
            gridItem.EffectiveDateVisible = effectiveDateVisible;
            gridItem.DueDateVisible = dueDateVisible;
            gridItem.DifficultyVisible = difficultyVisible;
            gridItem.OnHoldVisible = onholdVisible;
            gridItem.ActivateVisible = activateVisible;
            gridItem.CommentsVisible = commentsVisible;
            */

            // if RSTM revision has split already, don't allow to edit
            editVisible = editVisible && gridItem.EditVisible;

            result.AppendFormat("{0},", previewVisible.ToString());
            result.AppendFormat("{0},", editVisible.ToString());
            result.AppendFormat("{0},", acceptVisible.ToString());
            result.AppendFormat("{0},", rejectVisible.ToString());
            result.AppendFormat("{0},", commentsVisible.ToString());
            result.AppendFormat("{0},", assignToMeVisible.ToString());
            result.AppendFormat("{0},", assignVisible.ToString());
            result.AppendFormat("{0},", historyVisible.ToString());
            result.AppendFormat("{0},", difficultyVisible.ToString());
            result.AppendFormat("{0},", dueDateVisible.ToString());
            result.AppendFormat("{0},", effectiveDateVisible.ToString());
            result.AppendFormat("{0},", onholdVisible.ToString());
            result.AppendFormat("{0},", activateVisible.ToString());
            result.AppendFormat("{0},", printVisible.ToString());
            result.Append(appointmentVisible.ToString());

            //AssignSTMSplitRevision = gridItem.AssignSTMSplitRevision ? Visibility.Visible : Visibility.Collapsed;
            AssignSTMSplitRevision = Visibility.Visible;

            if (gridItem.AllowToResetCurrentMilestone)
            {
                ShowUndoCurrentMilestone = Visibility.Visible;
                ShowUndoThisRevision = Visibility.Collapsed;
            }
            else
            {
                ShowUndoCurrentMilestone = Visibility.Collapsed;
                ShowUndoThisRevision = gridItem.AllowUndoCurrentRevision && (gridItem.OwnerId == (App.Current as App).CurrentUserId || (App.Current as App).IsManager) ? Visibility.Visible : Visibility.Collapsed;
            }
            ShowUndoSetContract = gridItem.AllowUndoSetContract ? Visibility.Visible: Visibility.Collapsed;

            //result.AppendFormat("{0},", changefacadeVisible.ToString());
            //result.Append( changecontracttypeVisible.ToString());

            //result = previewVisible.ToString() + "," + editVisible.ToString() + "," + acceptVisible.ToString() + "," + rejectVisible.ToString() + "," + commentsVisible.ToString();
            //result = result + "," + assignToMeVisible.ToString() + "," + assignVisible.ToString() + "," + historyVisible.ToString() + "," + difficultyVisible.ToString() + "," + dueDateVisible.ToString();
            //result = result + "," + effectiveDateVisible.ToString() + "," + onholdVisible.ToString() + "," + activateVisible.ToString();

            return result.ToString();
        }

        private int GetRevisionTypeFilter()
        {
            //string filter = string.Empty;

            //if (SelectedRevisionTypeId == 0)
            //{
                
            //    foreach (RevisionType type in RevisionTypes)
            //    {
            //        if (filter == string.Empty)
            //            filter = type.RevisionTypeId.ToString();
            //        else
            //            filter += "," + type.RevisionTypeId.ToString();
            //    }
            //}
            //else
            //    filter = SelectedRevisionTypeId.ToString();

            return SelectedRevisionTypeId;
        }

        //void mrsClient_GetAssignedEstimatesByUserCompleted(object sender, GetAssignedEstimatesByUserCompletedEventArgs e)
        //{
        //    if (e.Error == null)
        //    {
        //        if (_currentrole != 3 && _currentrole != 2)
        //        {
        //            switch (SelectedTabIndex)
        //            {
        //                case 1:
        //                    MyWorkspaceTab.Estimates.Clear();
        //                    foreach (var item in e.Result)
        //                    {
        //                        MyWorkspaceTab.Estimates.Add(item);
        //                    }
        //                    break;

        //                case 2:
        //                    AcceptedEstimatesTab.Estimates.Clear();
        //                    foreach (var item in e.Result)
        //                    {
        //                        AcceptedEstimatesTab.Estimates.Add(item);
        //                    }
        //                    break;

        //                case 3:
        //                    RejectedEstimatesTab.Estimates.Clear();
        //                    foreach (var item in e.Result)
        //                    {
        //                        RejectedEstimatesTab.Estimates.Add(item);
        //                    }
        //                    break;

        //                case 4:
        //                    OnHoldEstimatesTab.Estimates.Clear();
        //                    foreach (var item in e.Result)
        //                    {
        //                        OnHoldEstimatesTab.Estimates.Add(item);
        //                    }
        //                    break;

        //                case 5:
        //                    CancelledEstimatesTab.Estimates.Clear();
        //                    foreach (var item in e.Result)
        //                    {
        //                        CancelledEstimatesTab.Estimates.Add(item);
        //                    }
        //                    break;

        //                default:
        //                    break;
        //            }
        //        }
        //        else
        //        {
        //            MyWorkspaceTab.Estimates.Clear();
        //            foreach (var item in e.Result)
        //            {
        //                MyWorkspaceTab.Estimates.Add(item);
        //            }
        //        }
        //    }
        //    else
        //        ExceptionHandler.PopUpErrorMessage(e.Error, "GetAssignedEstimatesByUserCompleted");

        //    IsBusy = false;
        //}

        //void mrsClient_GetAssignedEstimatesByRegionCompleted(object s, GetAssignedEstimatesByRegionCompletedEventArgs e)
        //{
        //    if (e.Error == null)
        //    {

        //        if (_currentrole != 3 && _currentrole != 2)
        //        {
        //            switch (SelectedTabIndex)
        //            {
        //                case 1:
        //                    MyWorkspaceTab.Estimates.Clear();
        //                    foreach (var item in e.Result)
        //                    {
        //                        MyWorkspaceTab.Estimates.Add(item);
        //                    }
        //                    break;

        //                case 2:
        //                    AcceptedEstimatesTab.Estimates.Clear();
        //                    foreach (var item in e.Result)
        //                    {
        //                        AcceptedEstimatesTab.Estimates.Add(item);
        //                    }
        //                    break;

        //                case 3:
        //                    RejectedEstimatesTab.Estimates.Clear();
        //                    foreach (var item in e.Result)
        //                    {
        //                        RejectedEstimatesTab.Estimates.Add(item);
        //                    }
        //                    break;

        //                case 4:
        //                    OnHoldEstimatesTab.Estimates.Clear();
        //                    foreach (var item in e.Result)
        //                    {
        //                        OnHoldEstimatesTab.Estimates.Add(item);
        //                    }
        //                    break;

        //                case 5:
        //                    CancelledEstimatesTab.Estimates.Clear();
        //                    foreach (var item in e.Result)
        //                    {
        //                        CancelledEstimatesTab.Estimates.Add(item);
        //                    }
        //                    break;

        //                default:
        //                    break;
        //            }
        //        }
        //        else
        //        {
        //            MyWorkspaceTab.Estimates.Clear();
        //            foreach (var item in e.Result)
        //            {
        //                MyWorkspaceTab.Estimates.Add(item);
        //            }
        //        }
        //    }
        //    else
        //        ExceptionHandler.PopUpErrorMessage(e.Error, "GetAssignedEstimatesByRegionCompleted");

        //    IsBusy = false;
        //}

        void mrsClient_GetQueuedEstimatesCompleted(object sender, GetQueuedEstimatesCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                NewEstimatesTab.Estimates.Clear();
                foreach (var item in e.Result)
                {
                    NewEstimatesTab.Estimates.Add(item);
                }

                RetrieveSearchResultEstimate();
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetQueuedEstimatesCompleted");

            IsBusy = false;
        }



        #region Populate Sales Consultant

        public void PopulateSalesConsultant()
        {
            RetailSystemClient mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            mrsClient.GetUsersByRegionAndRoleCompleted += new EventHandler<GetUsersByRegionAndRoleCompletedEventArgs>(mrsClient_GetUsersByRegionAndRoleCompleted);
            mrsClient.GetUsersByRegionAndRoleAsync((App.Current as App).CurrentRegionId, 3);
        }

        public void PopulateUsersByRegionAndRevisionType()
        {
            RetailSystemClient mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            mrsClient.GetUsersByRegionAndRevisionTypeCompleted += new EventHandler<GetUsersByRegionAndRevisionTypeCompletedEventArgs>(mrsClient_GetUsersByRegionAndRevisionTypeCompleted);
            mrsClient.GetUsersByRegionAndRevisionTypeAsync((App.Current as App).CurrentRegionId, 0);
        }

        void mrsClient_GetUsersByRegionAndRoleCompleted(object sender, GetUsersByRegionAndRoleCompletedEventArgs e)
        {
            int userId = (App.Current as App).CurrentUserId;

            if (e.Error == null)
            {
                SalesConsultants.Clear();

                if (_currentrole != 3) //Sales Consultant
                {
                    SalesConsultants.Add(new User() { FullName = "All", UserId = 0 });
                    foreach (var item in e.Result)
                    {
                        SalesConsultants.Add(item);
                    }
                    SelectedSalesConsultantId = 0;
                }
                else
                {
                    foreach (var item in e.Result)
                    {
                        if (item.UserId == userId)
                        {
                            SalesConsultants.Add(item);
                        }
                    }
                    SelectedSalesConsultantId = userId;
                }
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetUsersByRegionAndRoleCompleted");
        }

        void mrsClient_GetUsersByRegionAndRevisionTypeCompleted(object sender, GetUsersByRegionAndRevisionTypeCompletedEventArgs e)
        {
            int userIdSelected = SelectedUserId;

            if (e.Error == null)
            {
                ObservableCollection<User> owners = e.Result;
                MRSUsers.Clear();

                MRSUsers.Add(new User() { FullName = "All", UserId = 0 });
                foreach (var item in e.Result)
                {
                    MRSUsers.Add(new User() { FullName = item.FullName, UserId = item.UserId });
                }
                SelectedUserId = userIdSelected;
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetUsersByRegionAndRevisionTypeCompleted");
        }

        #endregion

        #region Populate RevisionType

        public void PopulateRevisionType()
        {
            RetailSystemClient mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            this.RevisionTypes = new ObservableCollection<RevisionType>();

            mrsClient.GetRevisionTypeAccessCompleted += new EventHandler<GetRevisionTypeAccessCompletedEventArgs>(mrsClient_GetRevisionTypeAccessCompleted);
            mrsClient.GetRevisionTypeAccessAsync((App.Current as App).CurrentUserRoleId);
        }

        void mrsClient_GetRevisionTypeAccessCompleted(object sender, GetRevisionTypeAccessCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                RevisionTypes.Clear();
                RevisionTypes.Add(new RevisionType() { RevisionTypeName = "All", RevisionTypeId = 0 });
                foreach (var item in e.Result)
                {
                    RevisionTypes.Add(item);
                }

                SelectedRevisionTypeId = 0;

                if ((App.Current as App).SelectedTab != SelectedTabIndex)
                {
                    if (EstimateTabs.Count > (App.Current as App).SelectedTab)
                        SelectedTabIndex = (App.Current as App).SelectedTab;
                    else
                        SelectedTabIndex = 0;
                }
                else
                    RefreshTab();

            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetRevisionTypeAccessCompleted");
        }

        public void PopulateDistrictsAndOpsCenter()
        {
            RetailSystemClient mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            this.BusinessUnitsList = new ObservableCollection<GenericClassCodeName>();
            mrsClient.GetBusinessUnitsCompleted += new EventHandler<GetBusinessUnitsCompletedEventArgs>(mrsClient_GetBusinessUnitsListCompleted);
            mrsClient.GetBusinessUnitsAsync((App.Current as App).LoginPriceRegionId);

            //this.DistrictList = new ObservableCollection<District>();
            //mrsClient.GetDistrictListCompleted += new EventHandler<GetDistrictListCompletedEventArgs>(mrsClient_GetDistrictListCompleted);
            //mrsClient.GetDistrictListAsync((App.Current as App).CurrentUserRoleId);

            //this.OperatingCenterList = new ObservableCollection<OperatingCenter>();
            //mrsClient.GetOperatingCenterListCompleted += new EventHandler<GetOperatingCenterListCompletedEventArgs>(mrsClient_GetOperatingCenterListCompleted);
            //mrsClient.GetOperatingCenterListAsync((App.Current as App).CurrentUserRoleId);
        }

        void mrsClient_GetBusinessUnitsListCompleted(object sender, GetBusinessUnitsCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                BusinessUnitsList.Clear();
                if (e.Result != null && e.Result.Count > 0)
                {
                    foreach (var item in e.Result)
                    {
                        BusinessUnitsList.Add(item);
                    }
                    BusinessUnit = BusinessUnitsList[0].CodeValue;
                }
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetRevisionTypeAccessCompleted");
        }

        //void mrsClient_GetDistrictListCompleted(object sender, GetDistrictListCompletedEventArgs e)
        //{
        //    if (e.Error == null)
        //    {
        //        DistrictList.Clear();
        //        DistrictList.Add(new District() { DistrictName = "All", DistrictCode = string.Empty });
        //        DistrictCode = string.Empty;
        //        foreach (var item in e.Result)
        //        {
        //            DistrictList.Add(item);
        //        }
        //    }
        //    else
        //        ExceptionHandler.PopUpErrorMessage(e.Error, "GetRevisionTypeAccessCompleted");
        //}

        //void mrsClient_GetOperatingCenterListCompleted(object sender, GetOperatingCenterListCompletedEventArgs e)
        //{
        //    if (e.Error == null)
        //    {
        //        OperatingCenterList.Clear();
        //        OperatingCenterList.Add(new OperatingCenter() { OpsCenterName = "All", OpsCenterCode = string.Empty });
        //        OpsCenterCode = string.Empty;
        //        foreach (var item in e.Result)
        //        {
        //            OperatingCenterList.Add(item);
        //        }
        //    }
        //    else
        //        ExceptionHandler.PopUpErrorMessage(e.Error, "GetRevisionTypeAccessCompleted");
        //}

        #endregion

        #region Search Command

        public ICommand SearchCommand
        {
            get { return _searchCommand; }
            //private set;
        }

        public void SearchEstimates()
        {
            RefreshTab();
        }

        #endregion

        #region Clear Filter Command

        public ICommand ClearCommand
        {
            get { return _clearCommand; }
            //private set;
        }

        public void ClearSearchFilter()
        {
            CustomerNumber = null;
            ContractNumber = null;
            LotNumber = null;
            StreetName = null;
            Suburb = null;

            SelectedSalesConsultantId = 0;
            SelectedUserId = 0;
            SelectedRevisionTypeId = 0;

            if (BusinessUnitsList.Count > 1)
            {
                BusinessUnit = string.Empty;
            }

            (App.Current as App).CurrentCustomerNumber = "";
            (App.Current as App).CurrentContractNumber = "";
            (App.Current as App).CurrentSelectedSalesConsultantId = 0;
            (App.Current as App).CurrentLotNumber = "";
            (App.Current as App).CurrentStreetName = "";
            (App.Current as App).CurrentSuburb = "";
            (App.Current as App).CurrentSelectedUserId = 0;

            RefreshTab();
        }

        #endregion

        #region Properties

        public int SelectedTabIndex
        {
            get
            {
                return _selectedTabIndex;
            }

            set
            {
                if (_selectedTabIndex != value)
                {
                    _selectedTabIndex = value;
                    OnPropertyChanged("SelectedTabIndex");
                    (App.Current as App).SelectedTab = value;

                    LoadCurrentFilters();

                    RefreshTab();
                }
            }
        }

        private void LoadCurrentFilters()
        {
            if (CustomerNumber == null || CustomerNumber.Trim() == "")
            {
                CustomerNumber = (App.Current as App).CurrentCustomerNumber;
            }
            if (ContractNumber == null || ContractNumber.Trim() == "")
            {
                ContractNumber = (App.Current as App).CurrentContractNumber;
            }
            if (SelectedSalesConsultantId == 0)
            {
                SelectedSalesConsultantId = (App.Current as App).CurrentSelectedSalesConsultantId;
            }
            if (LotNumber == null || LotNumber.Trim() == "")
            {
                LotNumber = (App.Current as App).CurrentLotNumber;
            }
            if (StreetName == null || StreetName.Trim() == "")
            {
                StreetName = (App.Current as App).CurrentStreetName;
            }
            if (Suburb == null || Suburb.Trim() == "")
            {
                Suburb = (App.Current as App).CurrentSuburb;
            }
            if (SelectedUserId == 0)
            {
                SelectedUserId = (App.Current as App).CurrentSelectedUserId;
            }
        }

        public int SelectedSalesConsultantId
        {
            get
            {
                return _selectedSalesConsultantId;
            }

            set
            {
                if (_selectedSalesConsultantId != value)
                {
                    _selectedSalesConsultantId = value;
                    OnPropertyChanged("SelectedSalesConsultantId");
                }
            }   
        }

        public int SelectedUserId
        {
            get
            {
                return _selectedUserId;
            }

            set
            {
                if (_selectedUserId != value)
                {
                    _selectedUserId = value;
                    OnPropertyChanged("SelectedUserId");
                }
            }
        }

        public bool OwnerFilterVisible
        {
            get
            {
                return _ownerFilterVisible;
            }

            set
            {
                if (_ownerFilterVisible != value)
                {
                    _ownerFilterVisible = value;
                    OnPropertyChanged("OwnerFilterVisible");
                }
            }
        }

        public int SelectedRevisionTypeId
        {
            get
            {
                return _selectedRevisionTypeId;
            }

            set
            {
                if (_selectedRevisionTypeId != value)
                {
                    _selectedRevisionTypeId = value;
                    OnPropertyChanged("SelectedRevisionTypeId");
                }
            } 
        }

        public string CustomerNumber
        {
            get
            {
                return _customerNumber;
            }

            set
            {
                if (_customerNumber != value)
                {
                    _customerNumber = value;
                    OnPropertyChanged("CustomerNumber");
                }
            }
        }

        public string ContractNumber
        {
            get
            {
                return _contractNumber;
            }

            set
            {
                if (_contractNumber != value)
                {
                    _contractNumber = value;
                    OnPropertyChanged("ContractNumber");
                }
            }
        }

        public string LotNumber
        {
            get
            {
                return _lotNumber;
            }

            set
            {
                if (_lotNumber != value)
                {
                    _lotNumber = value;
                    OnPropertyChanged("LotNumber");
                }
            }
        }

        public string StreetName
        {
            get
            {
                return _streetName;
            }

            set
            {
                if (_streetName != value)
                {
                    _streetName = value;
                    OnPropertyChanged("StreetName");
                }
            }
        }

        public string Suburb
        {
            get
            {
                return _suburb;
            }

            set
            {
                if (_suburb != value)
                {
                    _suburb = value;
                    OnPropertyChanged("Suburb");
                }
            }
        }

        public string BusinessUnit
        {
            get
            {
                return _businessUnit;
            }

            set
            {
                if (_businessUnit != value)
                {
                    _businessUnit = value;
                    OnPropertyChanged("BusinessUnit");
                }
            }
        }

        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }

            set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    OnPropertyChanged("IsBusy");
                }
            }
        }

        public bool Hidden
        {
            get
            {
                return _isHidden;
            }

            set
            {
                if (_isHidden != value)
                {
                    _isHidden = value;
                    OnPropertyChanged("Hidden");
                }
            }
        }

        Visibility _assignSTMSplitRevision = Visibility.Visible;
        public Visibility AssignSTMSplitRevision
        {
            get
            {
                return _assignSTMSplitRevision;
            }
            set
            {
                _assignSTMSplitRevision = value;
                OnPropertyChanged("AssignSTMSplitRevision");
            }
        }

        Visibility _showUndoThisRevision = Visibility.Visible;
        public Visibility ShowUndoThisRevision
        {
            get
            {
                return _showUndoThisRevision;
            }
            set
            {
                _showUndoThisRevision = value;
                OnPropertyChanged("ShowUndoThisRevision");
            }
        }

        Visibility _showUndoCurrentMilestone = Visibility.Visible;
        public Visibility ShowUndoCurrentMilestone
        {
            get
            {
                return _showUndoCurrentMilestone;
            }
            set
            {
                _showUndoCurrentMilestone = value;
                OnPropertyChanged("ShowUndoCurrentMilestone");
            }
        }

        Visibility _showUndoSetContract = Visibility.Visible;
        public Visibility ShowUndoSetContract
        {
            get
            {
                return _showUndoSetContract;
            }
            set
            {
                _showUndoSetContract = value;
                OnPropertyChanged("ShowUndoSetContract");
            }
        }
        #endregion
    }

    public class EstimateListTab : ViewModelBase
    {
        public EstimateListTab(string name)
        {
            this.Name = name;
            this.Estimates = new ObservableCollection<EstimateGridItem>();
        }
        public string Name { get; set; }
        public ObservableCollection<EstimateGridItem> _estimate;
        public ObservableCollection<EstimateGridItem> Estimates
        {
            get
            {
                return _estimate;
            }
            set
            {
                _estimate = value;
                OnPropertyChanged("Estimates");
            }
        }
        private string _count;
        public Visibility _visible;
        public Visibility Visible
        {
            get
            {
                return _visible;
            }

            set
            {
                if (_visible != value)
                {
                    _visible = value;
                    OnPropertyChanged("Visible");
                }
            }
        }

        public string Count
        {
            get
            {
                return _count;
            }

            set
            {
                if (_count != value)
                {
                    _count = value;
                    OnPropertyChanged("Count");
                }
            }
        }
    }


    public class GridViewItem : EstimateGridItem
    {
        public GridViewItem(EstimateGridItem parent)
        {
            this.ContractNumber = parent.ContractNumber;

        }
 
    }
}
