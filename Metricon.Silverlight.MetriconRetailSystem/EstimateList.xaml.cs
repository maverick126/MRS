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
using System.Windows.Messaging;

using Metricon.Silverlight.MetriconRetailSystem.ViewModels;
using Metricon.Silverlight.MetriconRetailSystem.ChildWindows;
using Metricon.Silverlight.MetriconRetailSystem.MRSService;

using Telerik.Windows;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
using Telerik.Windows.Persistence;
using Telerik.Windows.Persistence.Storage;
using Telerik.Windows.Persistence.Services;
using System.IO.IsolatedStorage;
using System.IO;

namespace Metricon.Silverlight.MetriconRetailSystem
{
    public partial class EstimateList : Page
    {
        public static int SelectedEstimateRevisionId = 0;
        public static string SelectedOpportunityid = null;
        public static string SelectedContractid = null;
        public static int SelectedRevisionTypeID = 0;
        public static string SelectedCustomerName = "";
        public static string SelectedAccountID = "";
        public static int SelectedCustomerNumber = 0;
        public static int SelectedContractNumber = 0;
        public static int SelectedEstimateID = 0;
        public static string SelectedContractStatusName = "";
        public static string SelectedRevisionTypeCode = "";
        public static int SelectedRevisionNumber = 0;
        public static bool IsMilestoneRevisionSelected = false;
        public static bool IsVisible = false;
        public bool goaheadaccept = true;

        public static bool IsQueue = false;
        public static RevisionTypePermission revisiontypepermission = new RevisionTypePermission();
        public RetailSystemClient mrsClient;

        System.IO.Stream stream;
        private bool IsGridLoaded = false;
        private bool IsFilterLoaded = false;

        public EstimateList()
        {
            InitializeComponent();
            // register the custom property provider for the RadGridView:
            ServiceProvider.RegisterPersistenceProvider<ICustomPropertyProvider>(typeof(RadGridView), new GridViewCustomPropertyProvider());
        }

        // Executes when the user navigates to this page.
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //int selectedIndex = ((EstimateListViewModel)LayoutRoot.DataContext).SelectedTabIndex;

            //// to force load current tabl
            //((EstimateListViewModel)LayoutRoot.DataContext).SelectedTabIndex = -1;
            //((EstimateListViewModel)LayoutRoot.DataContext).SelectedTabIndex = selectedIndex;
        }

        private void RadContextMenu_ItemClick(object sender, RadRoutedEventArgs e)
        {
            try
            {
                RadContextMenu menu = (RadContextMenu)sender;
                RadMenuItem clickedItem = e.OriginalSource as RadMenuItem;
                GridViewRow row = menu.GetClickedElement<GridViewRow>();
                EstimateGridItem selectedEstimate = (EstimateGridItem)row.Item;
                bool validateUserActionAnyFailure = false;
                string menuAction = clickedItem != null ? Convert.ToString(clickedItem.Header) : "";

                SelectedRevisionTypeID = selectedEstimate.RevisionTypeId;
                if (SelectedRevisionTypeID == 24 && ((RadMenuItem)menu.FindName("bvar")).Visibility == System.Windows.Visibility.Visible &&
                    (menuAction == "Ready for Studio M Split Revisions" || menuAction == "Skip to Single Studio M Revision"))
                {
                    // When at PSTM-CSC, check if construction commenced then don't allow "Building Variation", "Ready for Studio M Split Revisions", "Skip to Single Studio M Revision"
                    // menuAction == "Building Variation" is allowed now - 18-03-2019 as per sprint 11
                    RadWindow.Alert(menuAction + " is not allowed, since the construction has commenced.\r\nPlease contact metricon support.");
                    validateUserActionAnyFailure = true;
                }

                if (!validateUserActionAnyFailure && row != null)
                {
                    if (((RadMenuItem)menu.FindName("pstm")).Visibility == System.Windows.Visibility.Visible
                          && menuAction == "Pre Site Variation")
                    {
                        // business wants to display Pre Site Variation Menu label (display purpose only), but processing wise it will be different
                        // for Pre Site variation (Pre Studio M) 
                        // and Pre Site variation (Post Studio M)
                        menuAction = "Pre Studio M Variation";
                    }
                    SelectedContractid = selectedEstimate.ContractID;
                    SelectedOpportunityid = selectedEstimate.Opportunityid;
                    SelectedEstimateRevisionId = selectedEstimate.RecordId;

                    revisiontypepermission.AllowToAddNSR = selectedEstimate.AllowToAddNSR;
                    revisiontypepermission.ValidateAccept = selectedEstimate.ValidateAcceptedFlag;
                    revisiontypepermission.ValidateStandardInclusion = selectedEstimate.ValidateStandardInclusion;
                    revisiontypepermission.ReadOnly = selectedEstimate.ReadOnly;
                    revisiontypepermission.AllowToAcceptItem = selectedEstimate.AllowToAcceptItem;
                    revisiontypepermission.AllowToViewStudioMTab = selectedEstimate.AllowToViewStudioMTab;
                    IsMilestoneRevisionSelected = (selectedEstimate.RevisionDetails.ToLower().Contains("pc") || selectedEstimate.RevisionDetails.ToLower().Contains("contract") || selectedEstimate.RevisionDetails.ToLower().Contains("variation")) ? true : false;

                    (App.Current as App).SelectedEstimateAllowToAddNSR = selectedEstimate.AllowToAddNSR;
                    (App.Current as App).SelectedEstimateValidateAccept = selectedEstimate.ValidateAcceptedFlag;
                    (App.Current as App).SelectedEstimateValidateStandardInclusion = selectedEstimate.ValidateStandardInclusion;
                    (App.Current as App).SelectedEstimateAllowToAcceptItem = selectedEstimate.AllowToAcceptItem;
                    (App.Current as App).SelectedEstimateAllowToViewStudioMTab = selectedEstimate.AllowToViewStudioMTab;
                    (App.Current as App).SelectedEstimateRevisionTypeID = selectedEstimate.RevisionTypeId;
                    (App.Current as App).SelectedContractType = selectedEstimate.ContractType;
                    (App.Current as App).OpportunityID = selectedEstimate.Opportunityid;
                    (App.Current as App).SelectedContractNumber = selectedEstimate.ContractNumber.ToString();
                    (App.Current as App).SelectedEstimateAllowToViewStudioMDocuSign = selectedEstimate.AllowToViewStudioMDocuSign;

                    SelectedAccountID = selectedEstimate.Accountid;
                    SelectedCustomerName = selectedEstimate.CustomerName;
                    SelectedCustomerNumber = selectedEstimate.CustomerNumber;
                    SelectedContractNumber = selectedEstimate.ContractNumber;
                    SelectedContractStatusName = selectedEstimate.ContractStatusName;
                    SelectedEstimateID = selectedEstimate.EstimateId;
                    SelectedRevisionTypeCode = selectedEstimate.RevisionTypeCode;
                    SelectedRevisionNumber = selectedEstimate.RevisionNumber;

                    RadWindow win = new RadWindow();
                    switch (menuAction)
                    {
                        case "View":
                            (App.Current as App).CurrentAction = "VIEW";
                            revisiontypepermission.ReadOnly = true;
                            // Users can only view previous revision when the estimate is in New Jobs queue
                            if (selectedEstimate.JobLocation != "SQS")
                            {
                                if (IsQueue || selectedEstimate.PreviousRevisionId > 0)
                                    SelectedEstimateRevisionId = selectedEstimate.PreviousRevisionId;
                                else
                                    SelectedEstimateRevisionId = selectedEstimate.RecordId;

                                this.NavigationService.Navigate(new Uri("/Estimate.xaml", UriKind.Relative));
                            }
                            else
                                MessageBox.Show("SQS Estimates cannot be viewed here");
                            break;
                        case "Print":
                            (App.Current as App).CurrentAction = "PRINT";
                            revisiontypepermission.ReadOnly = true;
                            // Users can only view previous revision when the estimate is in New Jobs queue
                            if (IsQueue)
                                SelectedEstimateRevisionId = selectedEstimate.PreviousRevisionId;
                            else
                                SelectedEstimateRevisionId = selectedEstimate.RecordId;

                            PrintPreview previewDlg = new PrintPreview(SelectedEstimateRevisionId, SelectedRevisionTypeID, IsMilestoneRevisionSelected);
                            win.Header = "Print Preview";
                            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                            win.Content = previewDlg;
                            win.ShowDialog();
                            break;
                        case "Edit":
                            revisiontypepermission.ReadOnly = false;
                            if (revisiontypepermission.ValidateStandardInclusion) // studio M
                            {
                                CheckEstimateLockStatus(SelectedEstimateRevisionId);
                            }
                            else
                            {
                                EditEstimate("OK");
                            }
                            break;
                        case "Assign To Me":
                            AssignToMe assignToMeDlg = new AssignToMe(row.DataContext);
                            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                            win.Header = "Assign Estimate To Me";
                            win.Content = assignToMeDlg;
                            win.Closed += new EventHandler<WindowClosedEventArgs>(win_Closed);
                            win.ShowDialog();
                            break;

                        case "View History":
                            EstimateHistory historyDlg = new EstimateHistory(row.DataContext);
                            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                            win.ResizeMode = ResizeMode.CanResize;
                            win.Header = "Estimate History";
                            win.Content = historyDlg;
                            win.ShowDialog();
                            break;

                        case "Audit Trail":
                            AuditTrail auditDlg = new AuditTrail(row.DataContext);
                            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                            win.Header = "Estimate Audit Trail";
                            win.Content = auditDlg;
                            win.ShowDialog();
                            break;

                        case "Assign":
                            AssignOwner assignDlg = new AssignOwner(row.DataContext);
                            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                            win.Header = "Assign Estimate";
                            win.Content = assignDlg;
                            win.Closed += new EventHandler<WindowClosedEventArgs>(win_Closed);
                            win.ShowDialog();
                            break;

                        case "Complete":
                            if (revisiontypepermission.ValidateStandardInclusion) //Studio M
                            {
                                ValidateStandardInclusion(EstimateList.SelectedEstimateRevisionId);
                            }
                            else if (revisiontypepermission.ValidateAccept) //Always True
                            {
                                //if (SelectedRevisionTypeID == 5) //CSC revision
                                //{
                                //    ValidateAppointmentDate(EstimateList.SelectedEstimateRevisionId);
                                //}
                                //else
                                //{
                                //Validate Accepted Flag for STA/SE and NSR Area/Group for all
                                ValidateAcceptedFlag(EstimateList.SelectedEstimateRevisionId);
                                //}
                            }
                            else
                            {
                                AcceptEstimate();
                            }

                            break;

                        case "Reject":
                            SetEstimateStatus rejectDlg = new SetEstimateStatus(SelectedEstimateRevisionId, 3);
                            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                            win.Header = "Reject Estimate Window";
                            win.Content = rejectDlg;
                            win.Closed += new EventHandler<WindowClosedEventArgs>(win_Closed);
                            win.ShowDialog();
                            break;

                        case "Activate":
                            SetEstimateStatus activateDlg = new SetEstimateStatus(SelectedEstimateRevisionId, 1);
                            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                            win.Header = "Activate Estimate Window";
                            win.Content = activateDlg;
                            win.Closed += new EventHandler<WindowClosedEventArgs>(win_Closed);
                            win.ShowDialog();
                            break;

                        case "Edit Comments":
                            ModifyComments commentsDlg = new ModifyComments(SelectedEstimateRevisionId);
                            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                            win.Header = "Edit Comment Window";
                            win.Content = commentsDlg;
                            win.Closed += new EventHandler<WindowClosedEventArgs>(win_Closed);
                            win.ShowDialog();
                            break;

                        case "Price Effective Date":
                            SetEffectiveDate effectiveDlg = new SetEffectiveDate(row.DataContext);
                            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                            win.Header = "Change Price Effective Date Window";
                            win.Content = effectiveDlg;
                            win.Closed += new EventHandler<WindowClosedEventArgs>(win_Closed);
                            win.ShowDialog();
                            break;

                        case "Due Date":
                            SetDueDate dueDlg = new SetDueDate(row.DataContext);
                            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                            win.Header = "Due Date Window";
                            win.Content = dueDlg;
                            win.Closed += new EventHandler<WindowClosedEventArgs>(win_Closed);
                            win.ShowDialog();
                            break;

                        case "Appointment Time":
                            SetAppointmentDate appointmentDlg = new SetAppointmentDate(row.DataContext);
                            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                            win.Header = "Appointment Time Window";
                            win.Content = appointmentDlg;
                            win.Closed += new EventHandler<WindowClosedEventArgs>(win_Closed);
                            win.ShowDialog();
                            break;

                        case "Difficulty Rating":
                            SetDifficultyRating ratingDlg = new SetDifficultyRating(row.DataContext);
                            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                            win.Header = "Change Job Difficulty Rating Window";
                            win.Content = ratingDlg;
                            win.Closed += new EventHandler<WindowClosedEventArgs>(win_Closed);
                            win.ShowDialog();
                            break;

                        case "On Hold":
                            SetEstimateStatus holdDlg = new SetEstimateStatus(SelectedEstimateRevisionId, 4);
                            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                            win.Header = "Hold Estimate Window";
                            win.Content = holdDlg;
                            win.Closed += new EventHandler<WindowClosedEventArgs>(win_Closed);
                            win.ShowDialog();
                            break;

                        case "Colour Selection":
                            ContractDraftActions colourDlg = new ContractDraftActions(row.DataContext, 7);
                            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                            win.Header = "Colour Selection";
                            win.Content = colourDlg;
                            win.Closed += new EventHandler<WindowClosedEventArgs>(win_ClosedAndChangeFilter);
                            win.ShowDialog();
                            break;

                        case "Electrical Selection":
                            ContractDraftActions electricalDlg = new ContractDraftActions(row.DataContext, 8);
                            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                            win.Header = "Electrical Selection";
                            win.Content = electricalDlg;
                            win.Closed += new EventHandler<WindowClosedEventArgs>(win_ClosedAndChangeFilter);
                            win.ShowDialog();
                            break;

                        case "Paving Selection":
                            ContractDraftActions pavingDlg = new ContractDraftActions(row.DataContext, 9);
                            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                            win.Header = "Paving Selection";
                            win.Content = pavingDlg;
                            win.Closed += new EventHandler<WindowClosedEventArgs>(win_ClosedAndChangeFilter);
                            win.ShowDialog();
                            break;

                        case "Tile Selection":
                            ContractDraftActions tileDlg = new ContractDraftActions(row.DataContext, 10);
                            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                            win.Header = "Tile Selection";
                            win.Content = tileDlg;
                            win.Closed += new EventHandler<WindowClosedEventArgs>(win_ClosedAndChangeFilter);
                            win.ShowDialog();
                            break;

                        case "Decking Selection":
                            ContractDraftActions deckingDlg = new ContractDraftActions(row.DataContext, 11);
                            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                            win.Header = "Decking Selection";
                            win.Content = deckingDlg;
                            win.Closed += new EventHandler<WindowClosedEventArgs>(win_ClosedAndChangeFilter);
                            win.ShowDialog();
                            break;

                        case "Carpet Selection":
                            ContractDraftActions carpetDlg = new ContractDraftActions(row.DataContext, 12);
                            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                            win.Header = "Carpet Selection";
                            win.Content = carpetDlg;
                            win.Closed += new EventHandler<WindowClosedEventArgs>(win_ClosedAndChangeFilter);
                            win.ShowDialog();
                            break;

                        case "Curtains and Blinds Selection":
                            ContractDraftActions curtainDlg = new ContractDraftActions(row.DataContext, 21);
                            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                            win.Header = "Curtains and Blinds Selection";
                            win.Content = curtainDlg;
                            win.Closed += new EventHandler<WindowClosedEventArgs>(win_ClosedAndChangeFilter);
                            win.ShowDialog();
                            break;

                        case "Timber Floor Selection":
                            ContractDraftActions floorDlg = new ContractDraftActions(row.DataContext, 22);
                            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                            win.Header = "Timber Floor Selection";
                            win.Content = floorDlg;
                            win.Closed += new EventHandler<WindowClosedEventArgs>(win_ClosedAndChangeFilter);
                            win.ShowDialog();
                            break;

                        case "Appliance Selection":
                            ContractDraftActions applianceDlg = new ContractDraftActions(row.DataContext, 28);
                            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                            win.Header = "Appliance Selection";
                            win.Content = applianceDlg;
                            win.Closed += new EventHandler<WindowClosedEventArgs>(win_ClosedAndChangeFilter);
                            win.ShowDialog();
                            break;

                        case "Landscaping Selection":
                            ContractDraftActions landscapingDlg = new ContractDraftActions(row.DataContext, 29);
                            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                            win.Header = "Landscaping Selection";
                            win.Content = landscapingDlg;
                            win.Closed += new EventHandler<WindowClosedEventArgs>(win_ClosedAndChangeFilter);
                            win.ShowDialog();
                            break;

                        case "Merge Studio M Revisions":
                            CreateRevisionConfirmation mergeDlg = new CreateRevisionConfirmation(SelectedEstimateRevisionId, 23);
                            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                            win.Header = "Merge Studio M Revisions";
                            win.Content = mergeDlg;
                            win.Closed += new EventHandler<WindowClosedEventArgs>(win_ClosedAndChangeFilter);
                            win.ShowDialog();
                            break;

                        case "Final Contract":
                            CreateContract finalDlg = new CreateContract(SelectedEstimateRevisionId, 13);
                            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                            win.Header = "Final Contract";
                            win.Content = finalDlg;
                            win.Closed += new EventHandler<WindowClosedEventArgs>(win_ClosedAndChangeFilter);
                            win.ShowDialog();
                            break;

                        case "Ready for Studio M Split Revisions":
                            CreateContract draftDlg = new CreateContract(SelectedEstimateRevisionId, 6);
                            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                            win.Header = "Ready for Studio M Split Revisions";
                            win.Content = draftDlg;
                            win.Closed += new EventHandler<WindowClosedEventArgs>(win_ClosedAndChangeFilter);
                            win.ShowDialog();
                            break;
                        case "Assign Studio M Split Revisions":
                            CreateStudioMSplit stmAssignDlg = new CreateStudioMSplit(SelectedEstimateRevisionId, 6);
                            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                            win.Header = "Ready for Studio M Split Revisions";
                            win.Content = stmAssignDlg;
                            win.Closed += new EventHandler<WindowClosedEventArgs>(win_ClosedAndChangeFilter);
                            win.ShowDialog();
                            break;

                        case "Skip to Single Studio M Revision":
                            CreateRevisionConfirmation stmDlg = new CreateRevisionConfirmation(SelectedEstimateRevisionId, 0);
                            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                            win.Header = "Skip to Single Studio M Revisions";
                            win.Content = stmDlg;
                            win.Closed += new EventHandler<WindowClosedEventArgs>(win_ClosedAndChangeFilter);
                            win.ShowDialog();
                            break;

                        case "Pre Site Variation":
                            CreateRevisionConfirmation pvarDlg = new CreateRevisionConfirmation(SelectedEstimateRevisionId, 14);
                            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                            win.Header = "Pre Site Variation";
                            win.Content = pvarDlg;
                            win.Closed += new EventHandler<WindowClosedEventArgs>(win_ClosedAndChangeFilter);
                            win.ShowDialog();
                            break;

                        case "Building Variation":
                            CreateRevisionConfirmation bvarDlg = new CreateRevisionConfirmation(SelectedEstimateRevisionId, 18);
                            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                            win.Header = "Building Variation";
                            win.Content = bvarDlg;
                            win.Closed += new EventHandler<WindowClosedEventArgs>(win_ClosedAndChangeFilter);
                            win.ShowDialog();
                            break;

                        case "Change Facade":
                            ChangeFacade changefacadeDlg = new ChangeFacade(SelectedEstimateRevisionId);
                            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                            win.ResizeMode = ResizeMode.NoResize;
                            win.Width = 1000;
                            win.Height = 600;
                            win.Header = "Change Facade";
                            win.Content = changefacadeDlg;
                            win.Closed += new EventHandler<WindowClosedEventArgs>(win_ClosedAndChangeFilter);
                            win.ShowDialog();
                            break;

                        case "Change Home":
                            ChangeHome changehomeDlg = new ChangeHome(SelectedEstimateRevisionId, int.Parse((App.Current as App).CurrentUserStateID));
                            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                            win.ResizeMode = ResizeMode.NoResize;
                            win.Width = 1000;
                            win.Height = 600;
                            win.Header = "Change Home";
                            win.Content = changehomeDlg;
                            win.Closed += new EventHandler<WindowClosedEventArgs>(win_ClosedAndChangeFilter);
                            win.ShowDialog();
                            break;

                        case "Change Job Flow":
                            ChangeContractType changejobflowDlg = new ChangeContractType(SelectedEstimateRevisionId, "Change Job Flow", IsMilestoneRevisionSelected);
                            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                            win.ResizeMode = ResizeMode.NoResize;
                            win.Width = 400;
                            win.Height = 200;
                            win.Header = "Change Job Flow";
                            win.Content = changejobflowDlg;
                            win.Closed += new EventHandler<WindowClosedEventArgs>(win_ClosedAndChangeFilter);
                            win.ShowDialog();
                            break;

                        case "Change Contract Type":
                            ChangeContractType changecontracttypDlg = new ChangeContractType(SelectedEstimateRevisionId, "Change Contract Type");
                            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                            win.ResizeMode = ResizeMode.NoResize;
                            win.Width = 400;
                            win.Height = 200;
                            win.Header = "Change Contract Type";
                            win.Content = changecontracttypDlg;
                            win.Closed += new EventHandler<WindowClosedEventArgs>(win_ClosedAndChangeFilter);
                            win.ShowDialog();
                            break;

                        case "Customer Support Coordinator":
                            CreateRevisionConfirmation cscDlg = new CreateRevisionConfirmation(SelectedEstimateRevisionId, 5);
                            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                            win.Header = "Customer Support Coordinator";
                            win.Content = cscDlg;
                            win.Closed += new EventHandler<WindowClosedEventArgs>(win_ClosedAndChangeFilter);
                            win.ShowDialog();
                            break;

                        case "Pre Studio M Variation":
                            CreateRevisionConfirmation pstmDlg = new CreateRevisionConfirmation(SelectedEstimateRevisionId, 24);
                            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                            win.Header = "Pre Site Variation";
                            win.Content = pstmDlg;
                            win.Closed += new EventHandler<WindowClosedEventArgs>(win_ClosedAndChangeFilter);
                            win.ShowDialog();
                            break;

                        case "DocuSign":
                            (App.Current as App).CurrentAction = "DocuSign";
                            //revisiontypepermission.ReadOnly = true;
                            // Users can only view previous revision when the estimate is in New Jobs queue
                            if (IsQueue)
                                SelectedEstimateRevisionId = selectedEstimate.PreviousRevisionId;
                            else
                                SelectedEstimateRevisionId = selectedEstimate.RecordId;

                            CustomerSign previewDlg2 = new CustomerSign(SelectedEstimateRevisionId, SelectedRevisionTypeID, SelectedEstimateID, SelectedRevisionNumber, SelectedAccountID);
                            win.Header = "DocuSign";
                            win.ResizeMode = ResizeMode.NoResize;
                            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                            win.Content = previewDlg2;
                            win.ShowDialog();
                            break;

                        case "Undo This Revision":
                        case "Undo Current Revision":
                            UndoThisRevisionConfirmation UndoThisRevisionConfirmationDlg = new UndoThisRevisionConfirmation(row.DataContext);
                            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                            win.ResizeMode = ResizeMode.NoResize;
                            win.Width = 600;
                            win.Height = 250;
                            win.Header = "Undo this revision";
                            win.Content = UndoThisRevisionConfirmationDlg;
                            win.Closed += new EventHandler<WindowClosedEventArgs>(win_ClosedAndChangeFilter);
                            win.ShowDialog();
                            break;

                        case "Reset Current Milestone":
                            UndoCurrentMilestoneConfirmation UndoLastMilestoneConfirmationDlg = new UndoCurrentMilestoneConfirmation(row.DataContext);
                            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                            win.ResizeMode = ResizeMode.NoResize;
                            win.Width = 600;
                            win.Height = 250;
                            win.Header = "Reset Current Milestone";
                            win.Content = UndoLastMilestoneConfirmationDlg;
                            win.Closed += new EventHandler<WindowClosedEventArgs>(win_ClosedAndChangeFilter);
                            win.ShowDialog();
                            break;

                        case "Reset HIA Contract and All Variations":
                            UndoSetContractConfirmation UndoSetContractConfirmationDlg = new UndoSetContractConfirmation(row.DataContext);
                            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                            win.ResizeMode = ResizeMode.NoResize;
                            win.Width = 600;
                            win.Height = 250;
                            win.Header = "Reset HIA Contract and All Variations";
                            win.Content = UndoSetContractConfirmationDlg;
                            win.Closed += new EventHandler<WindowClosedEventArgs>(win_ClosedAndChangeFilter);
                            win.ShowDialog();
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                RadWindow.Alert("Error in RadContextMenu_ItemClick:\r\n" + ex.Message);
            }
        }
        public void ValidateAcceptedFlag(int estimaterevisionid)
        {
            List<ValidationErrorMessage> result = new List<ValidationErrorMessage>();
            bool show = false;
            goaheadaccept = true;
            mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());
            mrsClient.ValidateAcceptFlagForRevisionCompleted += delegate(object o, ValidateAcceptFlagForRevisionCompletedEventArgs es)
            {
                if (es.Error == null)
                {
                    //foreach (ValidationErrorMessage s in es.Result)
                    //{
                    //    result.Add(s);
                    //}
                    //if (result.Count > 0)
                    //{
                    //    RadWindow win = new RadWindow();
                    //    ShowValidationMessage messageDlg = new ShowValidationMessage(result, show, estimaterevisionid);
                    //    win.Header = "Validation Error Message";
                    //    win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                    //    win.Content = messageDlg;
                    //    win.ShowDialog();
                    //}
                    //else
                    //{
                    //     AcceptEstimate();
                    //}
                    foreach (ValidationErrorMessage s in es.Result)
                    {
                        result.Add(s);
                        if (!s.AllowGoAhead)
                        {
                            goaheadaccept = false;
                        }
                    }
                    if (result.Count > 0 && !goaheadaccept)
                    {
                        show = false;
                    }
                    else if (result.Count > 0 && goaheadaccept)
                    {
                        show = true;
                    }
                    if (result.Count > 0)
                    {
                        RadWindow win = new RadWindow();
                        ShowValidationMessage messageDlg = new ShowValidationMessage(result, show, estimaterevisionid);
                        win.Header = "Validation Error Message";
                        win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                        win.Content = messageDlg;
                        win.Closed += new EventHandler<WindowClosedEventArgs>(ValidationWin_Closed);
                        win.ShowDialog();
                    }
                    else
                    {
                        AcceptEstimate();
                    }
                }
            };

            mrsClient.ValidateAcceptFlagForRevisionAsync(estimaterevisionid, ((App)App.Current).CurrentUserRoleId);

        }

        void ValidationWin_Closed(object sender, WindowClosedEventArgs e)
        {
            if (goaheadaccept)
            {
                AcceptEstimate();
            }
        }
        public void ValidateAppointmentDate(int estimaterevisionid)
        {
            mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            mrsClient.ValidateAppointmentDateCompleted += delegate(object o, ValidateAppointmentDateCompletedEventArgs es)
            {
                if (es.Error == null)
                {
                    if ((bool)es.Result)
                    {
                        ValidateAcceptedFlag(estimaterevisionid);
                    }
                    else
                    {
                        RadWindow.Alert("Please specify Appointment Time before Accepting/Rejecting this estimate");
                        return;
                    }
                }
            };

            mrsClient.ValidateAppointmentDateAsync(estimaterevisionid);
        }

        public void ValidateStandardInclusion(int estimaterevisionid)
        {
            List<ValidationErrorMessage> result = new List<ValidationErrorMessage>();
            bool show = true;
            goaheadaccept = true;
            mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());
            mrsClient.ValidateStudioMEstimateCompleted += delegate(object o, ValidateStudioMEstimateCompletedEventArgs es)
            {
                if (es.Error == null)
                {

                    //foreach (ValidationErrorMessage s in es.Result)
                    //{
                    //    if (show && !s.AllowGoAhead)
                    //    {
                    //        show = false;
                    //    }
                    //    result.Add(s);
                    //}
                    //if (result.Count>0)
                    //{
                    //    RadWindow win = new RadWindow();
                    //    ShowValidationMessage messageDlg = new ShowValidationMessage(result, show, estimaterevisionid);
                    //    win.Header = "Validation Error Message";
                    //    win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                    //    win.Closed += new EventHandler<WindowClosedEventArgs>(win_ClosedAndOpenAccept);
                    //    win.Content = messageDlg;
                    //    win.ShowDialog();                        
                    //}
                    //else
                    //{
                    //    AcceptEstimate();
                    //}
                    foreach (ValidationErrorMessage s in es.Result)
                    {
                        result.Add(s);
                        if (!s.AllowGoAhead)
                        {
                            goaheadaccept = false;
                        }
                    }
                    if (result.Count > 0 && !goaheadaccept)
                    {
                        show = false;
                    }
                    else if (result.Count > 0 && goaheadaccept)
                    {
                        show = true;
                    }
                    if (result.Count > 0)
                    {
                        RadWindow win = new RadWindow();
                        ShowValidationMessage messageDlg = new ShowValidationMessage(result, show, estimaterevisionid);
                        win.Header = "Validation Error Message";
                        win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                        win.Content = messageDlg;
                        win.Closed += new EventHandler<WindowClosedEventArgs>(ValidationWin_Closed);
                        win.ShowDialog();
                    }
                    else
                    {
                        AcceptEstimate();
                    }
                }
                else
                {
                    ExceptionHandler.PopUpErrorMessage(es.Error, "ValidateStudioMEstimateCompleted");
                }
            };

            mrsClient.ValidateStudioMEstimateAsync(estimaterevisionid);
        }

        private void AcceptEstimate()
        {
            RadWindow win = new RadWindow();
            SetEstimateStatus acceptDlg = new SetEstimateStatus(SelectedEstimateRevisionId, 2);
            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
            win.Header = "Complete Estimate Window";
            win.Content = acceptDlg;
            win.Closed += new EventHandler<WindowClosedEventArgs>(win_Closed);
            win.ShowDialog();
        }
        private void EditEstimate(string message)
        {
            if (message.ToUpper() == "OK")
            {
                (App.Current as App).CurrentAction = "EDIT";
                this.NavigationService.Navigate(new Uri("/Estimate.xaml", UriKind.Relative));
            }
            else if (message.Contains("IPAD"))
            {
                RadWindow.Alert("You can't edit this estimate. \r\nIt is currently used by IPAD user.");
                return;
            }
        }
        public void CheckEstimateLockStatus(int estimaterevisionid)
        {
            RetailSystemClient MRSclient = new RetailSystemClient();
            MRSclient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            MRSclient.CheckEstimateLockStatusCompleted += delegate(object o, CheckEstimateLockStatusCompletedEventArgs es)
            {
                if (es.Error == null)
                {
                    EditEstimate(es.Result.ToString());

                }
                else
                {
                    ExceptionHandler.PopUpErrorMessage(es.Error, "CheckEstimateLockStatusCompleted");
                }
            };

            MRSclient.CheckEstimateLockStatusAsync(estimaterevisionid);
        }

        void win_Closed(object sender, WindowClosedEventArgs e)
        {
            RadWindow dlg = (RadWindow)sender;
            bool? result = dlg.DialogResult;
            if (result.HasValue && result.Value)
                ((EstimateListViewModel)LayoutRoot.DataContext).RefreshTab();
        }

        void win_ClosedAndOpenAccept(object sender, WindowClosedEventArgs e)
        {
            RadWindow dlg = (RadWindow)sender;
            bool? result = dlg.DialogResult;
            if (result.HasValue && result.Value)
                AcceptEstimate();
        }

        void win_ClosedAndChangeFilter(object sender, WindowClosedEventArgs e)
        {
            RadWindow dlg = (RadWindow)sender;
            bool? result = dlg.DialogResult;
            if (result.HasValue && result.Value)
            {
                Frame parentFrame = (Frame)this.Parent.ParentOfType<Frame>();
                parentFrame.Refresh();
            }
        }

        private void Estimates_RowDetailsVisibilityChanged(object sender, Telerik.Windows.Controls.GridView.GridViewRowDetailsEventArgs e)
        {
            if (e.Visibility == Visibility.Visible)
            {
                EstimateListGridViewDetails control = e.DetailsElement as EstimateListGridViewDetails;
                if (control != null)
                {
                    control.PopulateContent();
                }
            }
        }


        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/ManagerTools.xaml", UriKind.Relative));
        }


        private void GridContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            try
            {
                RadContextMenu mc = (RadContextMenu)sender;

                GridViewRow row = mc.GetClickedElement<GridViewRow>();

                RadGridView gv = this.EstimateTabControl.FindChildByType<RadGridView>();

                if (gv != null && row != null)
                {
                    mc.Visibility = System.Windows.Visibility.Visible;

                    string temp = ((EstimateListViewModel)LayoutRoot.DataContext).SetActionVisibility(row.Item);
                    string[] visible = temp.Split(',');

                    if (mc != null)
                    {

                        //((RadMenuItem)mc.FindName("signviadocusign")).Visibility = System.Windows.Visibility.Collapsed;
                        //((RadMenuItem)mc.FindName("signinpersion")).Visibility = System.Windows.Visibility.Collapsed;
                        //((RadMenuItem)mc.FindName("canceldocusign")).Visibility = System.Windows.Visibility.Collapsed;
                        ((RadMenuItem)mc.FindName("customersign")).Visibility = System.Windows.Visibility.Collapsed;

                        if (visible[0].ToUpper() == "TRUE")
                            ((RadMenuItem)mc.FindName("view")).Visibility = System.Windows.Visibility.Visible;
                        else
                            ((RadMenuItem)mc.FindName("view")).Visibility = System.Windows.Visibility.Collapsed;

                        if (visible[1].ToUpper() == "TRUE")
                            ((RadMenuItem)mc.FindName("edit")).Visibility = System.Windows.Visibility.Visible;
                        else
                            ((RadMenuItem)mc.FindName("edit")).Visibility = System.Windows.Visibility.Collapsed;

                        if (visible[2].ToUpper() == "TRUE")
                            ((RadMenuItem)mc.FindName("accept")).Visibility = System.Windows.Visibility.Visible;
                        else
                            ((RadMenuItem)mc.FindName("accept")).Visibility = System.Windows.Visibility.Collapsed;

                        if (visible[3].ToUpper() == "TRUE")
                            ((RadMenuItem)mc.FindName("reject")).Visibility = System.Windows.Visibility.Visible;
                        else
                            ((RadMenuItem)mc.FindName("reject")).Visibility = System.Windows.Visibility.Collapsed;

                        if (visible[4].ToUpper() == "TRUE")
                            ((RadMenuItem)mc.FindName("editcomments")).Visibility = System.Windows.Visibility.Visible;
                        else
                            ((RadMenuItem)mc.FindName("editcomments")).Visibility = System.Windows.Visibility.Collapsed;

                        if (visible[5].ToUpper() == "TRUE")
                            ((RadMenuItem)mc.FindName("assigntome")).Visibility = System.Windows.Visibility.Visible;
                        else
                            ((RadMenuItem)mc.FindName("assigntome")).Visibility = System.Windows.Visibility.Collapsed;

                        if (visible[6].ToUpper() == "TRUE")
                            ((RadMenuItem)mc.FindName("assign")).Visibility = System.Windows.Visibility.Visible;
                        else
                            ((RadMenuItem)mc.FindName("assign")).Visibility = System.Windows.Visibility.Collapsed;

                        if (visible[7].ToUpper() == "TRUE")
                            ((RadMenuItem)mc.FindName("viewhistory")).Visibility = System.Windows.Visibility.Visible;
                        else
                            ((RadMenuItem)mc.FindName("viewhistory")).Visibility = System.Windows.Visibility.Collapsed;

                        if (visible[8].ToUpper() == "TRUE")
                            ((RadMenuItem)mc.FindName("difficultyrating")).Visibility = System.Windows.Visibility.Visible;
                        else
                            ((RadMenuItem)mc.FindName("difficultyrating")).Visibility = System.Windows.Visibility.Collapsed;

                        if (visible[9].ToUpper() == "TRUE")
                            ((RadMenuItem)mc.FindName("duedate")).Visibility = System.Windows.Visibility.Visible;
                        else
                            ((RadMenuItem)mc.FindName("duedate")).Visibility = System.Windows.Visibility.Collapsed;

                        if (visible[10].ToUpper() == "TRUE")
                            ((RadMenuItem)mc.FindName("priceeffectivedate")).Visibility = System.Windows.Visibility.Visible;
                        else
                            ((RadMenuItem)mc.FindName("priceeffectivedate")).Visibility = System.Windows.Visibility.Collapsed;

                        if (visible[11].ToUpper() == "TRUE")
                            ((RadMenuItem)mc.FindName("onhold")).Visibility = System.Windows.Visibility.Visible;
                        else
                            ((RadMenuItem)mc.FindName("onhold")).Visibility = System.Windows.Visibility.Collapsed;

                        if (visible[12].ToUpper() == "TRUE")
                            ((RadMenuItem)mc.FindName("activate")).Visibility = System.Windows.Visibility.Visible;
                        else
                            ((RadMenuItem)mc.FindName("activate")).Visibility = System.Windows.Visibility.Collapsed;

                        if (visible[13].ToUpper() == "TRUE")
                            ((RadMenuItem)mc.FindName("print")).Visibility = System.Windows.Visibility.Visible;
                        else
                            ((RadMenuItem)mc.FindName("print")).Visibility = System.Windows.Visibility.Collapsed;

                        if (visible[14].ToUpper() == "TRUE")
                            ((RadMenuItem)mc.FindName("appointment")).Visibility = System.Windows.Visibility.Visible;
                        else
                            ((RadMenuItem)mc.FindName("appointment")).Visibility = System.Windows.Visibility.Collapsed;

                        //if (visible[15].ToUpper() == "TRUE")
                        //    ((RadMenuItem)mc.FindName("changefacade")).Visibility = System.Windows.Visibility.Visible;
                        //else
                        //    ((RadMenuItem)mc.FindName("changefacade")).Visibility = System.Windows.Visibility.Collapsed;
                        //((RadMenuItem)mc.FindName("changehome")).Visibility = ((RadMenuItem)mc.FindName("changefacade")).Visibility;
                        //if (visible[16].ToUpper() == "TRUE")
                        //    ((RadMenuItem)mc.FindName("changecontracttype")).Visibility = System.Windows.Visibility.Visible;
                        //else
                        //    ((RadMenuItem)mc.FindName("changecontracttype")).Visibility = System.Windows.Visibility.Collapsed;

                        ((RadMenuItem)mc.FindName("audittrail")).Visibility = System.Windows.Visibility.Visible;
                        ((RadMenuItem)mc.FindName("changecontracttype")).Visibility = System.Windows.Visibility.Collapsed;
                        ((RadMenuItem)mc.FindName("changejobflow")).Visibility = System.Windows.Visibility.Collapsed;

                        EstimateGridItem gridItem = (EstimateGridItem)row.Item;

                        if (!gridItem.RecordType.Equals("queue", StringComparison.OrdinalIgnoreCase) && EstimateTabControl.SelectedIndex == 1)
                        {
                            RetailSystemClient MRSclient = new RetailSystemClient();
                            MRSclient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

                            //if ((App.Current as App).SelectedStatusId == 2 && (gridItem.RevisionTypeId == 5 || gridItem.RevisionTypeId == 24)) //Accepted Customer Support Coordinator or Pre Studio M Variation revision
                            //{
                            //    MRSclient.GetContractDraftCreationVisibilityCompleted += delegate(object o, GetContractDraftCreationVisibilityCompletedEventArgs es)
                            //    {
                            //        if (es.Error == null)
                            //        {
                            //            if (es.Result)
                            //                ((RadMenuItem)mc.FindName("condft")).Visibility = System.Windows.Visibility.Visible;
                            //            else
                            //                ((RadMenuItem)mc.FindName("condft")).Visibility = System.Windows.Visibility.Collapsed;
                            //        }
                            //        else
                            //        {
                            //            ExceptionHandler.PopUpErrorMessage(es.Error, "GetContractDraftCreationVisibilityCompleted");
                            //        }
                            //    };

                            //    MRSclient.GetContractDraftCreationVisibilityAsync(gridItem.RecordId);

                            //    MRSclient.GetFinalContractCreationVisibilityCompleted += delegate(object o, GetFinalContractCreationVisibilityCompletedEventArgs es)
                            //    {
                            //        if (es.Error == null)
                            //        {
                            //            if (es.Result)
                            //                ((RadMenuItem)mc.FindName("final")).Visibility = System.Windows.Visibility.Visible;
                            //            else
                            //                ((RadMenuItem)mc.FindName("final")).Visibility = System.Windows.Visibility.Collapsed;
                            //        }
                            //        else
                            //        {
                            //            ExceptionHandler.PopUpErrorMessage(es.Error, "GetContractDraftCreationVisibilityCompleted");
                            //        }
                            //    };

                            //    MRSclient.GetFinalContractCreationVisibilityAsync(gridItem.RecordId);
                            //}

                            if (gridItem.RevisionTypeId == 6 && //Ready for Studio M Split Revisions
                                ((App.Current as App).CurrentUserRoleId == 4 || //Home Decor Consultant
                                (App.Current as App).CurrentUserRoleId == 18 || //CSC 
                                (App.Current as App).CurrentUserRoleId == 22 || //Customer Service Manager
                                (App.Current as App).CurrentUserRoleId == 57 || //Operations Manager
                                (App.Current as App).CurrentUserRoleId == 72)) //Studio Operations Manager 
                            {

                                MRSclient.GetContractDraftActionAvailabilityCompleted += delegate(object o, GetContractDraftActionAvailabilityCompletedEventArgs es)
                                {
                                    if (es.Error == null)
                                    {
                                        ContractDraftActionAvailability contractDraftAction = es.Result;
                                        if (contractDraftAction.ColourSelectionAvailable && ((RadMenuItem)mc.FindName("assignstmsplitrevision")).Visibility == Visibility.Collapsed)
                                            ((RadMenuItem)mc.FindName("colour")).Visibility = System.Windows.Visibility.Visible;
                                        else
                                            ((RadMenuItem)mc.FindName("colour")).Visibility = System.Windows.Visibility.Collapsed;

                                        if (contractDraftAction.ElectricalSelectionAvailable)
                                            ((RadMenuItem)mc.FindName("electrical")).Visibility = System.Windows.Visibility.Visible;
                                        else
                                            ((RadMenuItem)mc.FindName("electrical")).Visibility = System.Windows.Visibility.Collapsed;

                                        if (contractDraftAction.PavingSelectionAvailable)
                                            ((RadMenuItem)mc.FindName("paving")).Visibility = System.Windows.Visibility.Visible;
                                        else
                                            ((RadMenuItem)mc.FindName("paving")).Visibility = System.Windows.Visibility.Collapsed;

                                        if (contractDraftAction.TilesSelectionAvailable)
                                            ((RadMenuItem)mc.FindName("tile")).Visibility = System.Windows.Visibility.Visible;
                                        else
                                            ((RadMenuItem)mc.FindName("tile")).Visibility = System.Windows.Visibility.Collapsed;

                                        if (contractDraftAction.DeckingSelectionAvailable)
                                            ((RadMenuItem)mc.FindName("decking")).Visibility = System.Windows.Visibility.Visible;
                                        else
                                            ((RadMenuItem)mc.FindName("decking")).Visibility = System.Windows.Visibility.Collapsed;

                                        if (contractDraftAction.CarpetSelectionAvailable)
                                            ((RadMenuItem)mc.FindName("carpet")).Visibility = System.Windows.Visibility.Visible;
                                        else
                                            ((RadMenuItem)mc.FindName("carpet")).Visibility = System.Windows.Visibility.Collapsed;

                                        if (contractDraftAction.CurtainSelectionAvailable)
                                            ((RadMenuItem)mc.FindName("curtain")).Visibility = System.Windows.Visibility.Visible;
                                        else
                                            ((RadMenuItem)mc.FindName("curtain")).Visibility = System.Windows.Visibility.Collapsed;

                                        if (contractDraftAction.FloorSelectionAvailable)
                                            ((RadMenuItem)mc.FindName("floor")).Visibility = System.Windows.Visibility.Visible;
                                        else
                                            ((RadMenuItem)mc.FindName("floor")).Visibility = System.Windows.Visibility.Collapsed;

                                        if (contractDraftAction.ApplianceSelectionAvailable)
                                            ((RadMenuItem)mc.FindName("appliance")).Visibility = System.Windows.Visibility.Visible;
                                        else
                                            ((RadMenuItem)mc.FindName("appliance")).Visibility = System.Windows.Visibility.Collapsed;

                                        if (contractDraftAction.LandscapingSelectionAvailable)
                                            ((RadMenuItem)mc.FindName("landscaping")).Visibility = System.Windows.Visibility.Visible;
                                        else
                                            ((RadMenuItem)mc.FindName("landscaping")).Visibility = System.Windows.Visibility.Collapsed;

                                        if (contractDraftAction.StudioMAvailable &&
                                            ((App.Current as App).CurrentUserRoleId == 4 || //Home Decor Consultant
                                            (App.Current as App).CurrentUserRoleId == 18 || //Customer Support Coordinator
                                            (App.Current as App).CurrentUserRoleId == 22 || //Home Decor Consultant
                                            (App.Current as App).CurrentUserRoleId == 57 || //Operations Manager
                                            (App.Current as App).CurrentUserRoleId == 72)) //Customer Support Manager
                                            ((RadMenuItem)mc.FindName("studio")).Visibility = System.Windows.Visibility.Visible;
                                        else
                                            ((RadMenuItem)mc.FindName("studio")).Visibility = System.Windows.Visibility.Collapsed;
                                    }
                                    else
                                    {
                                        ExceptionHandler.PopUpErrorMessage(es.Error, "GetContractDraftActionAvailabilityCompleted");
                                    }
                                };

                                MRSclient.GetContractDraftActionAvailabilityAsync(gridItem.RecordId);
                            }
                            else
                            {
                                ((RadMenuItem)mc.FindName("colour")).Visibility = System.Windows.Visibility.Collapsed;
                                ((RadMenuItem)mc.FindName("electrical")).Visibility = System.Windows.Visibility.Collapsed;
                                ((RadMenuItem)mc.FindName("paving")).Visibility = System.Windows.Visibility.Collapsed;
                                ((RadMenuItem)mc.FindName("tile")).Visibility = System.Windows.Visibility.Collapsed;
                                ((RadMenuItem)mc.FindName("decking")).Visibility = System.Windows.Visibility.Collapsed;
                                ((RadMenuItem)mc.FindName("carpet")).Visibility = System.Windows.Visibility.Collapsed;
                                ((RadMenuItem)mc.FindName("curtain")).Visibility = System.Windows.Visibility.Collapsed;
                                ((RadMenuItem)mc.FindName("floor")).Visibility = System.Windows.Visibility.Collapsed;
                                ((RadMenuItem)mc.FindName("appliance")).Visibility = System.Windows.Visibility.Collapsed;
                                ((RadMenuItem)mc.FindName("landscaping")).Visibility = System.Windows.Visibility.Collapsed;
                                ((RadMenuItem)mc.FindName("studio")).Visibility = System.Windows.Visibility.Collapsed;
                                ((RadMenuItem)mc.FindName("final")).Visibility = System.Windows.Visibility.Collapsed;
                            }

                            if (gridItem.RevisionTypeId == 13 && //Final Contract
                                ((App.Current as App).CurrentUserRoleId == 18 || //CSC 
                                (App.Current as App).CurrentUserRoleId == 22)) //Customer Service Manager
                            {
                                MRSclient.GetFinalContractActionAvailabilityCompleted += delegate(object o, GetFinalContractActionAvailabilityCompletedEventArgs es)
                                {
                                    if (es.Error == null)
                                    {
                                        FinalContractActionAvailability finalContractActions = es.Result;
                                        if (finalContractActions.PreSiteVariationAvailable)
                                            ((RadMenuItem)mc.FindName("pvar")).Visibility = System.Windows.Visibility.Visible;
                                        else
                                            ((RadMenuItem)mc.FindName("pvar")).Visibility = System.Windows.Visibility.Collapsed;

                                        if (finalContractActions.BuildingVariationAvailable)
                                            ((RadMenuItem)mc.FindName("bvar")).Visibility = System.Windows.Visibility.Visible;
                                        else
                                            ((RadMenuItem)mc.FindName("bvar")).Visibility = System.Windows.Visibility.Collapsed;
                                    }
                                    else
                                    {
                                        ExceptionHandler.PopUpErrorMessage(es.Error, "GetFinalContractActionAvailabilityCompleted");
                                    }
                                };

                                MRSclient.GetFinalContractActionAvailabilityAsync(gridItem.RecordId, gridItem.ContractNumber.ToString());
                            }

                            //CSC Revision
                            if ((gridItem.RevisionTypeId == 5 || gridItem.RevisionTypeId == 6 || gridItem.RevisionTypeId == 31 || gridItem.RevisionTypeId == 32 ||
                                gridItem.RevisionTypeId == 24 || gridItem.RevisionTypeId == 14 || gridItem.RevisionTypeId == 18) && //CSC, PSTM-CSC, PVAR-CSC, BVAR-BSC
                                ((App.Current as App).SelectedStatusId == 1 || (App.Current as App).SelectedStatusId == 2) && //Completed
                                ((App.Current as App).CurrentUserRoleId == 4 || // Home decorating consultant 
                                (App.Current as App).CurrentUserRoleId == 72 || // Home decorating operations manager
                                (App.Current as App).CurrentUserRoleId == 18 || // CSC 
                                (App.Current as App).CurrentUserRoleId == 22 ||  // Customer Service Manager
                                (App.Current as App).CurrentUserRoleId == 57 ||  // Operations Manager
                                (App.Current as App).CurrentUserRoleId == 49 ||  // Finance Manager
                                (App.Current as App).CurrentUserRoleId == 78 ||  // BSC
                                (App.Current as App).CurrentUserRoleId == 82 ||  // BSM
                                (App.Current as App).CurrentUserRoleId == 85 ))  // CAD                                
                            {
                                MRSclient.GetCustomerSupportActionAvailabilityCompleted += delegate(object o, GetCustomerSupportActionAvailabilityCompletedEventArgs es)
                                {
                                    if (es.Error == null)
                                    {
                                        CustomerSupportActionAvailability customerSupportActions = es.Result;

                                        ((RadMenuItem)mc.FindName("condft")).Visibility = System.Windows.Visibility.Collapsed;
                                        ((RadMenuItem)mc.FindName("stm")).Visibility = System.Windows.Visibility.Collapsed;
                                        ((RadMenuItem)mc.FindName("assignstmsplitrevision")).Visibility = System.Windows.Visibility.Collapsed;
                                        if ((App.Current as App).SelectedStatusId == 1 && customerSupportActions.AssignSTMSplitAvailable && gridItem.RevisionTypeId == 6) // In Progress
                                        {
                                            ((RadMenuItem)mc.FindName("assignstmsplitrevision")).Visibility = System.Windows.Visibility.Visible;
                                            ((RadMenuItem)mc.FindName("colour")).Visibility = System.Windows.Visibility.Collapsed;
                                        }
                                        else if ((App.Current as App).SelectedStatusId == 2 && (App.Current as App).CurrentUserRoleId != 4 && (App.Current as App).CurrentUserRoleId != 72) // Completed
                                        {
                                            if (customerSupportActions.ContractDraftAvailable)
                                            {
                                                if (gridItem.RevisionTypeId == 5 || gridItem.RevisionTypeId == 14 || gridItem.RevisionTypeId == 24 || gridItem.RevisionTypeId == 31)
                                                { 
                                                    ((RadMenuItem)mc.FindName("condft")).Visibility = System.Windows.Visibility.Visible;
                                                    ((RadMenuItem)mc.FindName("stm")).Visibility = System.Windows.Visibility.Visible;
                                                }
                                            }

                                            if (customerSupportActions.FinalContractAvailable)
                                                ((RadMenuItem)mc.FindName("final")).Visibility = System.Windows.Visibility.Visible;
                                            else
                                                ((RadMenuItem)mc.FindName("final")).Visibility = System.Windows.Visibility.Collapsed;

                                            if (customerSupportActions.CustomerSupportAvailable)
                                                ((RadMenuItem)mc.FindName("csc")).Visibility = System.Windows.Visibility.Visible;
                                            else
                                                ((RadMenuItem)mc.FindName("csc")).Visibility = System.Windows.Visibility.Collapsed;

                                            if (customerSupportActions.BuildingVariationAvailable)
                                                ((RadMenuItem)mc.FindName("bvar")).Visibility = System.Windows.Visibility.Visible;
                                            else
                                            {
                                                ((RadMenuItem)mc.FindName("bvar")).Visibility = System.Windows.Visibility.Collapsed;

                                                if (customerSupportActions.PreStudioVariationAvailable)
                                                    ((RadMenuItem)mc.FindName("pstm")).Visibility = System.Windows.Visibility.Visible;
                                                else
                                                {
                                                    ((RadMenuItem)mc.FindName("pstm")).Visibility = System.Windows.Visibility.Collapsed;

                                                    if (customerSupportActions.PreSiteVariationAvailable)
                                                        ((RadMenuItem)mc.FindName("pvar")).Visibility = System.Windows.Visibility.Visible;
                                                    else
                                                        ((RadMenuItem)mc.FindName("pvar")).Visibility = System.Windows.Visibility.Collapsed;
                                                }
                                            }
                                        }

                                        if (customerSupportActions.ChangeContractTypeAvailable)
                                            ((RadMenuItem)mc.FindName("changecontracttype")).Visibility = System.Windows.Visibility.Visible;

                                        if (customerSupportActions.ChangeJobFlowTypeAvailable)
                                            ((RadMenuItem)mc.FindName("changejobflow")).Visibility = System.Windows.Visibility.Visible;

                                        //if (gridItem.ContractType == "PC")
                                        //{
                                        //    ((RadMenuItem)mc.FindName("csc")).Visibility = System.Windows.Visibility.Visible;
                                        //    ((RadMenuItem)mc.FindName("pstm")).Visibility = System.Windows.Visibility.Collapsed;
                                        //}
                                        //else
                                        //{
                                        //    ((RadMenuItem)mc.FindName("csc")).Visibility = System.Windows.Visibility.Collapsed;
                                        //    ((RadMenuItem)mc.FindName("pstm")).Visibility = System.Windows.Visibility.Visible;
                                        //}
                                    }
                                    else
                                    {
                                        ExceptionHandler.PopUpErrorMessage(es.Error, "GetCustomerSupportActionAvailabilityCompleted");
                                    }
                                };

                                MRSclient.GetCustomerSupportActionAvailabilityAsync(gridItem.RecordId, gridItem.ContractNumber.ToString());
                            }
                            else
                            {
                                ((RadMenuItem)mc.FindName("condft")).Visibility = System.Windows.Visibility.Collapsed;
                                ((RadMenuItem)mc.FindName("stm")).Visibility = System.Windows.Visibility.Collapsed;
                                ((RadMenuItem)mc.FindName("final")).Visibility = System.Windows.Visibility.Collapsed;
                                ((RadMenuItem)mc.FindName("csc")).Visibility = System.Windows.Visibility.Collapsed;
                                ((RadMenuItem)mc.FindName("pstm")).Visibility = System.Windows.Visibility.Collapsed;
                                ((RadMenuItem)mc.FindName("pvar")).Visibility = System.Windows.Visibility.Collapsed;
                                ((RadMenuItem)mc.FindName("bvar")).Visibility = System.Windows.Visibility.Collapsed;
                            }

                            //Sales Estimating, Drafting and Sales Accept Revisions
                            if ((gridItem.RevisionTypeId == 2 || gridItem.RevisionTypeId == 3 || gridItem.RevisionTypeId == 4 ||
                                gridItem.RevisionTypeId == 15 || gridItem.RevisionTypeId == 25) && //Pre Site VAR SE or Pre Studio M VAR SE 
                                (App.Current as App).SelectedStatusId == 1 && (App.Current as App).SelectedTab!=2)  //In Progress
                            {
                                MRSclient.GetSalesEstimatorActionAvailabilityCompleted += delegate(object o, GetSalesEstimatorActionAvailabilityCompletedEventArgs es)
                                {
                                    if (es.Error == null)
                                    {
                                        SalesEstimatorActionAvailability estimatorActions = es.Result;

                                        // not required for drafting
                                        if (gridItem.RevisionTypeId != 3)
                                        {
                                            if (estimatorActions.ChangeFacadeAvailable)
                                                ((RadMenuItem)mc.FindName("changefacade")).Visibility = System.Windows.Visibility.Visible;
                                            else
                                                ((RadMenuItem)mc.FindName("changefacade")).Visibility = System.Windows.Visibility.Collapsed;
                                            ((RadMenuItem)mc.FindName("changehome")).Visibility = ((RadMenuItem)mc.FindName("changefacade")).Visibility;

                                            if (estimatorActions.ChangePriceEffectiveDateAvailable)
                                                ((RadMenuItem)mc.FindName("priceeffectivedate")).Visibility = System.Windows.Visibility.Visible;
                                            else
                                                ((RadMenuItem)mc.FindName("priceeffectivedate")).Visibility = System.Windows.Visibility.Collapsed;
                                        }

                                        if (estimatorActions.ChangeContractTypeAvailable)
                                            ((RadMenuItem)mc.FindName("changecontracttype")).Visibility = System.Windows.Visibility.Visible;

                                        if (estimatorActions.ChangeJobFlowTypeAvailable)
                                            ((RadMenuItem)mc.FindName("changejobflow")).Visibility = System.Windows.Visibility.Visible;
                                    }
                                    else
                                    {
                                        ExceptionHandler.PopUpErrorMessage(es.Error, "GetSalesEstimatorActionAvailabilityCompleted");
                                    }
                                };

                                MRSclient.GetSalesEstimatorActionAvailabilityAsync(gridItem.RecordId, (App.Current as App).CurrentUserId);
                            }
                            else
                            {
                                //Price Effective Date can be changed by Sales Manager so don't update it here in else condition
                                ((RadMenuItem)mc.FindName("changefacade")).Visibility = System.Windows.Visibility.Collapsed;
                                ((RadMenuItem)mc.FindName("changehome")).Visibility = System.Windows.Visibility.Collapsed;
                            }

                            // get docusign context munu
                            if (((App.Current as App).CurrentUserRoleId == 18 || //CSC 
                                (App.Current as App).CurrentUserRoleId == 22 || //Customer Service Manager 
                                (App.Current as App).CurrentUserRoleId == 78 || //Building Support Coordinator 
                                (App.Current as App).CurrentUserRoleId == 82) && //Building Support Manager
                                (App.Current as App).CurrentUserId == gridItem.OwnerId)
                            {
                                MRSclient.GetCustomerDocumentTypeCompleted += delegate(object o, GetCustomerDocumentTypeCompletedEventArgs es)
                                {
                                    if (es.Error == null)
                                    {
                                        string action = es.Result;

                                        if (action != null && action != "" && (App.Current as App).SelectedStatusId == 1)
                                        {
                                            //((RadMenuItem)mc.FindName("customersign")).Visibility = System.Windows.Visibility.Visible;

                                        }
                                    }
                                    else
                                    {
                                        ExceptionHandler.PopUpErrorMessage(es.Error, "GetCustomerDocumentTypeCompleted");
                                    }
                                };
                                MRSclient.GetCustomerDocumentTypeAsync(gridItem.RecordId);
                            }
                        }
                        if (((App.Current as App).CurrentUserRoleId == 57 || (App.Current as App).CurrentUserRoleId == 82) && ((EstimateListViewModel)LayoutRoot.DataContext).ShowUndoCurrentMilestone == Visibility.Visible) // only Operations Manager is able to undo set as contract. only the contract has the HIA or VO
                        {
                            ((RadMenuItem)mc.FindName("undocurrentmilestone")).Visibility = System.Windows.Visibility.Visible;
                        }
                        else
                        {
                            ((RadMenuItem)mc.FindName("undocurrentmilestone")).Visibility = System.Windows.Visibility.Collapsed;
                        }
                        if ((App.Current as App).CurrentUserRoleId == 57 && ((EstimateListViewModel)LayoutRoot.DataContext).ShowUndoSetContract == Visibility.Visible) // only Operations Manager is able to undo set as contract. only the contract has the HIA or VO
                        {
                            ((RadMenuItem)mc.FindName("undosetcontract")).Visibility = System.Windows.Visibility.Visible;
                        }
                        else
                        {
                            ((RadMenuItem)mc.FindName("undosetcontract")).Visibility = System.Windows.Visibility.Collapsed;
                        }
                    }
                }
                else
                {

                    mc.Visibility = System.Windows.Visibility.Collapsed;

                    ((RadMenuItem)mc.FindName("view")).Visibility = System.Windows.Visibility.Collapsed;
                    ((RadMenuItem)mc.FindName("edit")).Visibility = System.Windows.Visibility.Collapsed;
                    ((RadMenuItem)mc.FindName("complete")).Visibility = System.Windows.Visibility.Collapsed;
                    ((RadMenuItem)mc.FindName("reject")).Visibility = System.Windows.Visibility.Collapsed;
                    ((RadMenuItem)mc.FindName("editcomments")).Visibility = System.Windows.Visibility.Collapsed;
                    ((RadMenuItem)mc.FindName("assigntome")).Visibility = System.Windows.Visibility.Collapsed;
                    ((RadMenuItem)mc.FindName("assign")).Visibility = System.Windows.Visibility.Collapsed;
                    ((RadMenuItem)mc.FindName("viewhistory")).Visibility = System.Windows.Visibility.Collapsed;
                    ((RadMenuItem)mc.FindName("difficultyrating")).Visibility = System.Windows.Visibility.Collapsed;
                    ((RadMenuItem)mc.FindName("duedate")).Visibility = System.Windows.Visibility.Collapsed;
                    ((RadMenuItem)mc.FindName("priceeffectivedate")).Visibility = System.Windows.Visibility.Collapsed;
                    ((RadMenuItem)mc.FindName("onhold")).Visibility = System.Windows.Visibility.Collapsed;
                    ((RadMenuItem)mc.FindName("activate")).Visibility = System.Windows.Visibility.Collapsed;
                    ((RadMenuItem)mc.FindName("audittrail")).Visibility = System.Windows.Visibility.Collapsed;
                    ((RadMenuItem)mc.FindName("colour")).Visibility = System.Windows.Visibility.Collapsed;
                    ((RadMenuItem)mc.FindName("electrical")).Visibility = System.Windows.Visibility.Collapsed;
                    ((RadMenuItem)mc.FindName("paving")).Visibility = System.Windows.Visibility.Collapsed;
                    ((RadMenuItem)mc.FindName("tile")).Visibility = System.Windows.Visibility.Collapsed;
                    ((RadMenuItem)mc.FindName("decking")).Visibility = System.Windows.Visibility.Collapsed;
                    ((RadMenuItem)mc.FindName("carpet")).Visibility = System.Windows.Visibility.Collapsed;
                    ((RadMenuItem)mc.FindName("curtain")).Visibility = System.Windows.Visibility.Collapsed;
                    ((RadMenuItem)mc.FindName("floor")).Visibility = System.Windows.Visibility.Collapsed;
                    ((RadMenuItem)mc.FindName("appliance")).Visibility = System.Windows.Visibility.Collapsed;
                    ((RadMenuItem)mc.FindName("landscaping")).Visibility = System.Windows.Visibility.Collapsed;
                    ((RadMenuItem)mc.FindName("studio")).Visibility = System.Windows.Visibility.Collapsed;
                    ((RadMenuItem)mc.FindName("final")).Visibility = System.Windows.Visibility.Collapsed;
                    ((RadMenuItem)mc.FindName("pvar")).Visibility = System.Windows.Visibility.Collapsed;
                    ((RadMenuItem)mc.FindName("bvar")).Visibility = System.Windows.Visibility.Collapsed;
                    ((RadMenuItem)mc.FindName("changefacade")).Visibility = System.Windows.Visibility.Collapsed;
                    ((RadMenuItem)mc.FindName("changehome")).Visibility = System.Windows.Visibility.Collapsed;
                    ((RadMenuItem)mc.FindName("changecontracttype")).Visibility = System.Windows.Visibility.Collapsed;
                    ((RadMenuItem)mc.FindName("csc")).Visibility = System.Windows.Visibility.Collapsed;
                    ((RadMenuItem)mc.FindName("pstm")).Visibility = System.Windows.Visibility.Collapsed;
                    ((RadMenuItem)mc.FindName("print")).Visibility = System.Windows.Visibility.Collapsed;
                    ((RadMenuItem)mc.FindName("customersign")).Visibility = System.Windows.Visibility.Collapsed;
                    ((RadMenuItem)mc.FindName("undocurrentmilestone")).Visibility = System.Windows.Visibility.Collapsed;
                    ((RadMenuItem)mc.FindName("undosetcontract")).Visibility = System.Windows.Visibility.Collapsed;
                    ((RadMenuItem)mc.FindName("undosetcontract")).Visibility = System.Windows.Visibility.Collapsed;
                    //((RadMenuItem)mc.FindName("signviadocusign")).Visibility = System.Windows.Visibility.Collapsed;
                    //((RadMenuItem)mc.FindName("signinpersion")).Visibility = System.Windows.Visibility.Collapsed;
                    //((RadMenuItem)mc.FindName("canceldocusign")).Visibility = System.Windows.Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                RadWindow.Alert("Error in GridContextMenu_Opened:\r\n" + ex.Message);
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //IsolatedStorageProvider isoStorageProvider = new IsolatedStorageProvider();
            //isoStorageProvider.SaveToStorage();

            PersistenceManager manager = new PersistenceManager();
            RadGridView rgd = this.EstimateTabControl.FindChildByType<RadGridView>();
            this.stream = manager.Save(rgd);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

            //IsolatedStorageProvider isoStorageProvider = new IsolatedStorageProvider();
            //isoStorageProvider.LoadFromStorage();
            if (this.stream != null && this.stream.Length > 0)
            {
                this.stream.Position = 0L;
                PersistenceManager manager = new PersistenceManager();
                RadGridView rgd = this.EstimateTabControl.FindChildByType<RadGridView>();
                if (rgd != null)
                {
                    manager.Load(rgd, this.stream);
                }
            }
        }

        private void Estimates_FilterOperatorsLoading(object sender, FilterOperatorsLoadingEventArgs e)
        {
            if (e.Column.UniqueName == "revisionnumber" ||
                e.Column.UniqueName == "customername" ||
                e.Column.UniqueName == "homename")
            {
                e.DefaultOperator1 = Telerik.Windows.Data.FilterOperator.Contains;
                e.DefaultOperator2 = Telerik.Windows.Data.FilterOperator.Contains;
            }
        }

        private void Estimates_Filtered(object sender, GridViewFilteredEventArgs e)
        {
            RadGridView rgv = ((RadGridView)sender);
            if (e.ColumnFilterDescriptor != null)
            {
                var column = (GridViewBoundColumnBase)e.ColumnFilterDescriptor.Column as GridViewBoundColumnBase;
                var columnHeader = rgv.ChildrenOfType<GridViewHeaderCell>()
                    .Where(headerCell => headerCell.Column == column).FirstOrDefault();

                if (columnHeader != null)
                {
                    if (e.ColumnFilterDescriptor.IsActive == true)
                        columnHeader.Background = new RadialGradientBrush(Color.FromArgb(75, 255, 255, 71), Color.FromArgb(75, 255, 255, 0));
                    else
                        columnHeader.Background = null;
                }
            }
            SaveLayout(rgv);
        }

        private void SaveLayout(RadGridView rgv)
        {
            // Obtain the isolated storage for an application.
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                //IsolatedStorageFileStream s = store.OpenFile(@"GridsCommand.txt", FileMode.Create, FileAccess.Write);
                using (Stream stream = new IsolatedStorageFileStream(@"EstimatesListGridsCommand.txt", FileMode.Create, FileAccess.Write, store))
                {
                    if (stream != null)
                    {
                        PersistenceManager manager = new PersistenceManager();
                        Stream str = manager.Save(rgv);

                        CopyStream(str, stream);
                    }
                }
            }
        }
        private void LoadLayout(RadGridView rgv)
        {
            // Obtain the isolated storage for an application.
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (store.FileExists("EstimatesListGridsCommand.txt"))
                {
                    using (Stream stream = new IsolatedStorageFileStream(@"EstimatesListGridsCommand.txt", FileMode.Open, FileAccess.Read, store))
                    {
                        if (stream != null)
                        {
                            PersistenceManager manager = new PersistenceManager();
                            manager.Load(rgv, stream);
                        }
                    }
                }
            }
        }
        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[8 * 1024];
            int len;
            while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, len);
            }
        }
 
        private void txtContractNumber_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ((EstimateListViewModel)LayoutRoot.DataContext).ContractNumber = txtContractNumber.Text;
                ((EstimateListViewModel)LayoutRoot.DataContext).SearchEstimates();
            }
        }

        private void txtCustomerNumber_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ((EstimateListViewModel)LayoutRoot.DataContext).CustomerNumber = txtCustomerNumber.Text;
                ((EstimateListViewModel)LayoutRoot.DataContext).SearchEstimates();
            }
        }

        private void txtLotNumber_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ((EstimateListViewModel)LayoutRoot.DataContext).LotNumber = txtLotNumber.Text;
                ((EstimateListViewModel)LayoutRoot.DataContext).SearchEstimates();
            }
        }

        private void txtStreetName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ((EstimateListViewModel)LayoutRoot.DataContext).StreetName = txtStreetName.Text;
                ((EstimateListViewModel)LayoutRoot.DataContext).SearchEstimates();
            }
        }

        private void txtDistrict_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //((EstimateListViewModel)LayoutRoot.DataContext).District = txtDistrict.Text;
                ((EstimateListViewModel)LayoutRoot.DataContext).SearchEstimates();
            }
        }

        private void txtOpsCenter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //((EstimateListViewModel)LayoutRoot.DataContext).OpCentre = txtOpsCenter.Text;
                ((EstimateListViewModel)LayoutRoot.DataContext).SearchEstimates();
            }
        }

        private void txtSuburb_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ((EstimateListViewModel)LayoutRoot.DataContext).Suburb = txtSuburb.Text;
                ((EstimateListViewModel)LayoutRoot.DataContext).SearchEstimates();
            }
        }

        private void Estimates_Loaded(object sender, RoutedEventArgs e)
        {
            RadGridView rv = (RadGridView)sender;
            GridViewColumn col100 = rv.Columns["mrsgroup"];
            GridViewColumn col101 = rv.Columns["location"];
            col100.IsVisible = false;
            col101.IsVisible = false;

            LoadLayout(rv);

            if (EstimateTabControl.SelectedIndex == 1)
            {
                foreach (var col in rv.Columns)
                {
                    if (col.Header != null && !string.Equals(col.Header.ToString(), "Owner"))
                        col.ColumnFilterDescriptor.Clear();
                }
                //int currentUserRoleId = (App.Current as App).CurrentUserRoleId;
                //if (!(App.Current as App).IsManager && 
                //    currentUserRoleId != 18 && currentUserRoleId != 78 && currentUserRoleId != 85)
                //{ 
                //    GridViewColumn ownerColumn = rv.Columns["owner"];
                //    IColumnFilterDescriptor ownerFilter = ownerColumn.ColumnFilterDescriptor;
                //    ownerFilter.SuspendNotifications();
                //    ownerFilter.FieldFilter.Filter1.Operator = Telerik.Windows.Data.FilterOperator.Contains;
                //    ownerFilter.FieldFilter.Filter1.Value = (App.Current as App).CurrentUserFullName;
                //    ownerFilter.FieldFilter.Filter1.IsCaseSensitive = true;
                //    ownerFilter.ResumeNotifications();
                //    //ownerColumn.Background = new RadialGradientBrush(Color.FromArgb(75, 255, 255, 71), Color.FromArgb(75, 255, 255, 0));  
                //    var columnHeader = rv.ChildrenOfType<GridViewHeaderCell>()
                //        .Where(headerCell => headerCell.Column == ownerColumn).FirstOrDefault();
                //    if (columnHeader != null)
                //    {
                //        if (columnHeader.Column.ColumnFilterDescriptor.IsActive == true)
                //            columnHeader.Background = new RadialGradientBrush(Color.FromArgb(75, 255, 255, 71), Color.FromArgb(75, 255, 255, 0));
                //        else
                //            columnHeader.Background = null;
                //    }
                //}
                IsGridLoaded = true;
            }
            else
            {
                foreach (var col in rv.Columns)
                {
                    col.ColumnFilterDescriptor.Clear();
                }
                if (EstimateTabControl.SelectedIndex == 2)
                {
                    GridViewColumn col1 = rv.Columns["createdon"];
                    GridViewColumn col2 = rv.Columns["contracttype"];

                    col1.IsVisible = false;
                    col2.IsVisible = false;
                    col100.IsVisible = true;
                    col101.IsVisible = true;
                }
            }

                //var columnHeader = rv.ChildrenOfType<GridViewHeaderCell>()
                //    .Where(headerCell => headerCell.Column == column).FirstOrDefault();
                //if (columnHeader != null)
                //{
                //    if (columnHeader.Column.ColumnFilterDescriptor.IsActive == true)
                //        columnHeader.Background = new RadialGradientBrush(Color.FromArgb(75, 255, 255, 71), Color.FromArgb(75, 255, 255, 0));
                //    else
                //        columnHeader.Background = null;
                //}
                //if (col.ColumnFilterDescriptor.IsActive == true)
                //{
                //    //col.Background = new RadialGradientBrush(Color.FromArgb(75, 255, 255, 71), Color.FromArgb(75, 255, 255, 0));
                //    GridViewHeaderCell columnHeader = null;
                //    foreach (var colHead in rv.ChildrenOfType<GridViewHeaderCell>())
                //        if (colHead.Column.Header == "Owner")
                //        {
                //            columnHeader = colHead;
                //            break;
                //        }
                //        //.Where(headerCell => headerCell.Column.Header == "Owner").FirstOrDefault();                    
                //        //.Where(headerCell => headerCell.Column.DisplayIndex == col.DisplayIndex).FirstOrDefault();
                //    if (columnHeader != null)
                //            columnHeader.Background = new RadialGradientBrush(Color.FromArgb(75, 255, 255, 71), Color.FromArgb(75, 255, 255, 0));            
                //}
                //else
                //    col.Background = null;
            //}
        }

        private void Estimates_ColumnReordered(object sender, GridViewColumnEventArgs e)
        {
            SaveLayout(((RadGridView)sender));
        }

        private void Estimates_ColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            SaveLayout(((RadGridView)sender));
        }

        private void Estimates_Sorted(object sender, GridViewSortedEventArgs e)
        {
            SaveLayout(((RadGridView)sender));
        }

        private void Estimates_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (IsGridLoaded)
            {
                if (((RadGridView)sender).EnableColumnVirtualization)
                {
                    ((RadGridView)sender).EnableColumnVirtualization = false;
                    ((RadGridView)sender).IsReadOnly = true;
                }
                else
                {
                    DisplayFilterColor(sender);
                    IsFilterLoaded = false;
                }
            }
        }

        private void DisplayFilterColor(object sender)
        {
            RadGridView rv = (RadGridView)sender;
            if (EstimateTabControl.SelectedIndex == 1)
            {
                //LoadLayout(rv);

                foreach (var col in rv.Columns)
                {
                     //GridViewColumn ownerColumn = rv.Columns["owner"];
                    //IColumnFilterDescriptor ownerFilter = ownerColumn.ColumnFilterDescriptor;
                    //ownerFilter.SuspendNotifications();
                    //ownerFilter.FieldFilter.Filter1.Operator = Telerik.Windows.Data.FilterOperator.Contains;
                    //ownerFilter.FieldFilter.Filter1.Value = (App.Current as App).CurrentUserFullName;
                    //ownerFilter.FieldFilter.Filter1.IsCaseSensitive = true;
                    //ownerFilter.ResumeNotifications();
                    //ownerColumn.Background = new RadialGradientBrush(Color.FromArgb(75, 255, 255, 71), Color.FromArgb(75, 255, 255, 0));  
                    var columnHeader = rv.ChildrenOfType<GridViewHeaderCell>()
                        .Where(headerCell => headerCell.Column == col).FirstOrDefault();
                    if (columnHeader != null)
                    {
                        if (columnHeader.Column.ColumnFilterDescriptor.IsActive == true)
                        {
                            columnHeader.Background = new RadialGradientBrush(Color.FromArgb(75, 255, 255, 71), Color.FromArgb(75, 255, 255, 0));
                        }
                        else
                            columnHeader.Background = null;
                    }

                }
            }
        }

        private void Estimates_MouseMove(object sender, MouseEventArgs e)
        {
            //if (!IsFilterLoaded)
            //{
                DisplayFilterColor(sender);
                //we have to set the color all the time otherwise grid overwrites the color to default on the smaller screens with horizontal bar
                //IsFilterLoaded = true;
            //}
        }


    }
}
