using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Text;
using Telerik.Windows.Controls;
using Metricon.Silverlight.MetriconRetailSystem.Command;
using Metricon.Silverlight.MetriconRetailSystem.MRSService;
using System.Linq;
using System.Xml.Linq;

namespace Metricon.Silverlight.MetriconRetailSystem.ViewModels
{
    public class EstimateViewModel : ViewModelBase
    {
        #region variables declaration
        private bool _isBusy;
        private bool _isbusyoptiontree;
        private bool _isbusyoptiontree3;
        private bool _isbusyoptiontree4;
        private bool _isBusyEstimateInfo;
        private bool _editVisible;
        private bool _rejectVisible;
        private bool _isRoleAllowtoAccessThisRevision;
        private int _selectedTabIndex, _userid;
        private string _customerno;
        private string _customername;
        private string _contractno;
        private string _estimateno;

        private string _homePrice;
        private string _upgradeValue;
        private string _promotionValue;
        private string _siteCost;
        private string _subTotalPrice;
        private string _totalPrice;
        private string _totalcostbtp;
        private string _totalcostdbc;
        private string _margin;
        private string _targetmargin;
        private string _marginstring;
        private string _totlpriceexc;
        private string _totalmargin;

        private string _homename;
        private string _homerange;
        private string _promotionname;
        private decimal _requiredbperollback;
        private decimal _requiredbpetodaysprice;
        private decimal _requiredbpe5percent;
        private string _region;
        private string _effectivedate;
        private string _priceexpirydate;
        private int _stdpriceholddays;
        private int _titledlanddays;
        private int _basepriceextensiondays;
        private decimal _basepriceextensioncharge;
        private int _revisedpriceholddays;
        private string _revisedpriceexpirydate;
        private string _depositdate;
        private string _revisionno;
        private string _jobFlowType;
        private string _contracttype;
        private string _salesconsultant;
        private string _templatenamesearchtext;
        private int _selectedregionid;
        private int _selectedhomeid;
        private string _salesacceptor;
        private string _draftperson;
        private string _salesestimator;
        private string _csc;
        private string _selectedpromotion;
        private string _sharePointFolderUrl;
        private string _promocount, _upgradecount, _nonstandardcount, _sicount, _studiomcount;
        private string _customerdocumenttypevalue = "PC";
        private string _customerdocumenttype;
        private bool m_bRegionIncludeAll = true;
        private int _selectedcategoryId = 0;
        private int _totaldefaultanswer = 0;
        private Visibility _show;
        private Visibility _showmargin;
        private bool _setaspcstate = false;
        private Visibility _showsetdocumenttype;
        private bool _setascontractstate = false;
        private Visibility _showsetascontract;
        private List<EstimateDetails> _optiontreeSource;
        private List<EstimateDetails> _otherhomeoptiontreeSource;
        private List<EstimateDetails> _searchallproductsoptiontreeSource;
        private ObservableCollection<SharepointDoc> _sharepointdocuments;
        //private List<OptionTreeProducts> _optiontreeSource;
        private List<ProductImage> _imagelist;
        private ObservableCollection<SharepointDocumentType> _doctypelist;
        public List<EstimateDetails> _upgradeSource;
        private ObservableCollection<EstimateComments> _comments ;
        private ObservableCollection<EstimateDetails> _optiontree;
        private ObservableCollection<EstimateDetails> _otherhomeoptiontree;
        private ObservableCollection<EstimateDetails> _searchallproductsoptiontree;
        private ObservableCollection<EstimateDetails> _option ;
        private ObservableCollection<EstimateDetails> _notetemplate ;
        //private ObservableCollection<NoteTempate> _managernotetemplate = new ObservableCollection<NoteTempate>();
        private ObservableCollection<SQSSalesRegion> _salesregion;
        private ObservableCollection<StudioMItem> _studimproductsneedanswer;
        private ObservableCollection<SQSHome> _sqshome = new ObservableCollection<SQSHome>();
        private ObservableCollection<SQSHome> _sqshomeall = new ObservableCollection<SQSHome>();
        private ObservableCollection<SQSSalesRegion> _sqssalesregion = new ObservableCollection<SQSSalesRegion>();
        private ObservableCollection<SQSArea> _sqsarea = new ObservableCollection<SQSArea>();
        private ObservableCollection<SQSGroup> _sqsgroup = new ObservableCollection<SQSGroup>();

        private string savemode = "";

        public ObservableCollection<OptionListTab> OptionListTabs { get; set; }
        public OptionListTab UpgradeTab { get; set; }
        public OptionListTab NonStandardTab { get; set; }
        public OptionListTab SiteCostTab { get; set; }
        public OptionListTab PromotionTab { get; set; }
        public OptionListTab PreviewTab { get; set; }
        public OptionListTab StandardInclusionTab { get; set; }
        public string _estimateheaderbrief;

        private DelegateCommand<EstimateDetails> addCommand;
        //private DelegateCommand<EstimateDetails> removeCommand;
        private DelegateCommand<EstimateDetails> copyCommand;
        private DelegateCommand<EstimateDetails> copyfromhomeCommand;
        private DelegateCommand<EstimateDetails> copyfromAllProductsCommand;
        private DelegateCommand<EstimateDetails> updatedetailsCommand;

        private DelegateCommand<string> notesCommand;
        public ObservableCollection<NonStandardCategory> EstimateNonStandardCategory { get; set; }
        public ObservableCollection<NonStandardGroup> _estimatenonstandardgroup = new ObservableCollection<NonStandardGroup>();
        public ObservableCollection<PriceDisplayCode> EstimateNonStandardPriceDisplayCode { get; set; }
       
        private Command.DelegateCommand syncCommand;
        //private DelegateCommand<string> searchnotestempalteCommand;

        //private DelegateCommand<EstimateDetails> removenotetemplateitemCommand;
        //private DelegateCommand<EstimateDetails> addnotetemplateitemCommand;
        //private DelegateCommand<NoteTempate> removenotetemplateCommand;
        //private DelegateCommand<EstimateDetails> addnotetemplateitemCommand;

        private RetailSystemClient mrsClient;

        private BackgroundWorker optionTreeWorker = new BackgroundWorker();

        #endregion

        #region methods of the class

        public EstimateViewModel()
        {

            _optiontreeSource = new List<EstimateDetails>();
            _imagelist = new List<ProductImage>();
            _upgradeSource = new List<EstimateDetails>();
            _comments = new ObservableCollection<EstimateComments>();
            _optiontree = new ObservableCollection<EstimateDetails>();
            _option = new ObservableCollection<EstimateDetails>();
            _notetemplate = new ObservableCollection<EstimateDetails>();
            _studimproductsneedanswer = new ObservableCollection<StudioMItem>();
            _otherhomeoptiontreeSource = new List<EstimateDetails>();
            _searchallproductsoptiontreeSource = new List<EstimateDetails>();

            _salesregion = new ObservableCollection<SQSSalesRegion>();


            OptionListTabs = new ObservableCollection<OptionListTab>();
            //SelectedTabIndex = 2;
            //PromotionTab = new OptionListTab("Promotion Products");

            //PromotionTab.Visible = Visibility.Collapsed;
            //OptionListTabs.Add(PromotionTab);

            SiteCostTab = new OptionListTab("Site Works & Connections");
            SiteCostTab.Visible = Visibility.Collapsed;
            OptionListTabs.Add(SiteCostTab);

            UpgradeTab = new OptionListTab("Upgrades");
            UpgradeTab.Visible = Visibility.Collapsed;
            OptionListTabs.Add(UpgradeTab);

            NonStandardTab = new OptionListTab("Non Standard Requests");
            NonStandardTab.Visible = Visibility.Collapsed;
            OptionListTabs.Add(NonStandardTab);

            StandardInclusionTab = new OptionListTab("Standard Inclusion");
            StandardInclusionTab.Visible = Visibility.Collapsed;
            //OptionListTabs.Add(StandardInclusionTab);

            //if ((App.Current as App).SelectedEstimateAllowToViewPreviewTab)
            //{
            //PreviewTab = new OptionListTab("StudioM");
            //PreviewTab.Visible = Visibility.Collapsed;
            //OptionListTabs.Add(PreviewTab);
            //}
            PreviewTab = new OptionListTab("Preview");
            PreviewTab.Visible = Visibility.Collapsed;
            OptionListTabs.Add(PreviewTab);

            optionTreeWorker.DoWork += new DoWorkEventHandler(optionTreeWorker_DoWork);
            optionTreeWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(optionTreeWorker_RunWorkerCompleted);

            if (!IsDesignTime)
            {
                _userid = (App.Current as App).CurrentUserId;
                mrsClient = new RetailSystemClient();
                mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());
                mrsClient.Endpoint.Binding.SendTimeout = new TimeSpan(0, 10, 0);

                GetComments();

                GetAdditionalNotes();

                GetNonStandardCategory();

                GetPriceDisplayCodes();

                GetAccessPermission();

               // GetSharepointDocument();
                //GetSharepointDocumentType();

                if ((App.Current as App).CurrentAction == "EDIT")
                {
                    GetSetDocumentTypeVisibility();

                    SynchronizeNewOptionsToEstimate(EstimateList.SelectedEstimateRevisionId);
                }
                else
                {
                    ShowSetDocumentType = Visibility.Collapsed;
                    CustomerDocumentType = string.Empty;
                    ShowSetAsContract = Visibility.Collapsed;

                    //RefreshTab();
                    RefreshHeaderInfo();
                    SelectedTabIndex = 2;
                }
            }
        }

        void optionTreeWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.OptionTree = (ObservableCollection<EstimateDetails>)e.Result;

            IsBusyOptionTree = false;
        }

        void optionTreeWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            ObservableCollection<EstimateDetails> temp = new ObservableCollection<EstimateDetails>();

            switch (SelectedTabIndex)
            {
                case 0:
                    foreach (EstimateDetails item in _optiontreeSource)
                    {
                        if (item.SiteWorkItem && item.StandardOption && item.AreaId != 43)
                        {
                            temp.Add(item);
                        }
                    }
                    break;
                case 1:
                    foreach (EstimateDetails item in _optiontreeSource)
                    {
                        if (item.StandardOption && !item.SiteWorkItem && item.AreaId != 43)
                        {
                            temp.Add(item);
                        }
                    }

                    break;
                case 2:
                    foreach (EstimateDetails item in _optiontreeSource)
                    {
                        if (item.AreaId == 43 && item.StandardOption)
                        {
                            temp.Add(item);
                        }
                    }
                    break;
                //case 3:
                //    foreach (EstimateDetails item in _optiontreeSource)
                //    {
                //        if (!item.StandardOption)
                //        {
                //            temp.Add(item);
                //        }
                //    }
                //    break;
                case 3:
                    foreach (EstimateDetails item in _optiontreeSource)
                    {
                        //if (item.StudioMProduct)
                        //{
                        temp.Add(item);
                        //}
                    }
                    break;
                default:
                    break;
            }

            e.Result = temp;
        }


        #region sharepoint file function
        public void GetSharepointDocument()
        {
            mrsClient.Sharepoint_GetFileListCompleted += new EventHandler<Sharepoint_GetFileListCompletedEventArgs>(mrsClient_Sharepoint_GetFileListCompleted);
            mrsClient.Sharepoint_GetFileListAsync((App.Current as App).OpportunityID, (App.Current as App).SelectedContractNumber);
            //sqsclient = new MetriconSalesWebService.MetriconSalesSoapClient();
            //sqsclient.Sharepoint_GetSharepointFileListInFolderCompleted += new EventHandler<Sharepoint_GetSharepointFileListInFolderCompletedEventArgs>(sqsclient_Sharepoint_GetSharepointFileListInFolderCompleted);
            //sqsclient.Sharepoint_GetSharepointFileListInFolderAsync((App.Current as App).OpportunityID, (App.Current as App).SelectedContractNumber);
        }

        void mrsClient_Sharepoint_GetFileListCompleted(object sender, Sharepoint_GetFileListCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                SharepointDocuments = e.Result;
            }
            else
            {
                ExceptionHandler.PopUpErrorMessage(e.Error, "Sharepoint_GetSharepointFileListInFolder");
            }
            mrsClient.Sharepoint_GetFileListCompleted -= new EventHandler<Sharepoint_GetFileListCompletedEventArgs>(mrsClient_Sharepoint_GetFileListCompleted);
        }

        public void DeleteSharepointDocument(SharepointDoc doc)
        {
            mrsClient.Sharepoint_DeleteFileFromSharepointLibraryCompleted += new EventHandler<Sharepoint_DeleteFileFromSharepointLibraryCompletedEventArgs>(Sharepoint_DeleteFileFromSharepointLibraryCompleted);
            mrsClient.Sharepoint_DeleteFileFromSharepointLibraryAsync(doc,(App.Current as App).OpportunityID, (App.Current as App).SelectedContractNumber);
        }
        void Sharepoint_DeleteFileFromSharepointLibraryCompleted(object sender, Sharepoint_DeleteFileFromSharepointLibraryCompletedEventArgs e)
        {
        }


        public void GetSharepointDocumentType()
        {
            mrsClient.Sharepoint_GetSalesDocumentTypeCompleted += new EventHandler<Sharepoint_GetSalesDocumentTypeCompletedEventArgs>(mrsClient_Sharepoint_GetSalesDocumentTypeCompleted);
            mrsClient.Sharepoint_GetSalesDocumentTypeAsync();
        }

        void mrsClient_Sharepoint_GetSalesDocumentTypeCompleted(object sender, Sharepoint_GetSalesDocumentTypeCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                this.SharepointDocType = e.Result;
            }
            else
            {
                ExceptionHandler.PopUpErrorMessage(e.Error, "Sharepoint_GetSalesDocumentTypeCompleted");
            }
            mrsClient.Sharepoint_GetSalesDocumentTypeCompleted -= new EventHandler<Sharepoint_GetSalesDocumentTypeCompletedEventArgs>(mrsClient_Sharepoint_GetSalesDocumentTypeCompleted);
        }


        public void UploadDocumentToSharepoint(string filename, byte[] contents, string doccategory, string doctype)
        {
            //string contentstring = System.Convert.ToBase64String(contents, 0, contents.Length);
            mrsClient.Sharepoint_SharepointUploadFileCompleted += new EventHandler<Sharepoint_SharepointUploadFileCompletedEventArgs>(Sharepoint_SharepointUploadFileCompleted);
            mrsClient.Sharepoint_SharepointUploadFileAsync(filename, contents, (App.Current as App).SelectedContractNumber, (App.Current as App).OpportunityID, doccategory, doctype);
        }
        void Sharepoint_SharepointUploadFileCompleted(object sender, Sharepoint_SharepointUploadFileCompletedEventArgs e)
        {
            GetSharepointDocument();
        }
        #endregion
        public void GetComments()
        {
            mrsClient.GetCommentsForAnEstimateCompleted += new EventHandler<GetCommentsForAnEstimateCompletedEventArgs>(mrsClient_GetCommentsForAnEstimateCompleted);
            mrsClient.GetCommentsForAnEstimateAsync(EstimateList.SelectedEstimateRevisionId.ToString());
        }

        public void GetAccessPermission()
        {
            mrsClient.GetAccessPermissionCompleted += new EventHandler<GetAccessPermissionCompletedEventArgs>(mrsClient_GetAccessPermissionCompleted);
            mrsClient.GetAccessPermissionAsync(EstimateList.SelectedEstimateRevisionId.ToString(), (App.Current as App).CurrentUserId.ToString(), (App.Current as App).CurrentUserRoleId.ToString());
        }

        public void GetAdditionalNotes()
        {
            mrsClient.GetAdditionalNotesTemplateAndProductsCompleted += new EventHandler<GetAdditionalNotesTemplateAndProductsCompletedEventArgs>(mrsClient_AdditionalNotesTemplateAndProductsCompleted);
            mrsClient.GetAdditionalNotesTemplateAndProductsAsync(EstimateList.SelectedEstimateRevisionId, (App.Current as App).CurrentUserId);
        }

        void mrsClient_GetCommentsForAnEstimateCompleted(object sender, GetCommentsForAnEstimateCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                this.Comments = e.Result;
            }
            else
            {
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetCommentsForAnEstimateCompleted");
            }
            mrsClient.GetCommentsForAnEstimateCompleted -= new EventHandler<GetCommentsForAnEstimateCompletedEventArgs>(mrsClient_GetCommentsForAnEstimateCompleted);
        }

        void mrsClient_GetAccessPermissionCompleted(object sender, GetAccessPermissionCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                this.IsRoleAllowtoAccessThisRevision = (bool)e.Result;
            }
            else
            {
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetAccessPermissionCompleted");
            }
            mrsClient.GetAccessPermissionCompleted -= new EventHandler<GetAccessPermissionCompletedEventArgs>(mrsClient_GetAccessPermissionCompleted);
        }
        //void mrsClient_GetOptionTreeAsEstimateDetailsCompleted(object sender, GetOptionTreeAsEstimateDetailsCompletedEventArgs e)
        //{
        //    if (e.Error == null)
        //    {
        //        // Copy full option tree to re-use later
        //        _optiontreeSource = new List<EstimateDetails>();
        //        _optiontreeSource.AddRange(e.Result);

        //        if (!optionTreeWorker.IsBusy)
        //            // Build Option Tree in another thread 
        //            optionTreeWorker.RunWorkerAsync();
        //    }
        //    else
        //    {
        //        ExceptionHandler.PopUpErrorMessage(e.Error, "GetOptionTreeAsEstimateDetailsCompleted");
        //    }

        //    IsBusyOptionTree = false;

        //    // Remove Event Handler
        //    mrsClient.GetOptionTreeAsEstimateDetailsCompleted -= new EventHandler<GetOptionTreeAsEstimateDetailsCompletedEventArgs>(mrsClient_GetOptionTreeAsEstimateDetailsCompleted);
        //}

        void mrsClient_GetEstimateHeaderCompleted(object sender, GetEstimateHeaderCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                EstimateHeader header = e.Result;
                CustomerNo = header.CustomerNumber.ToString();
                CustomerName = header.CustomerName;
                ContractNo = header.ContractNumber.ToString();
                EstimateNo = header.EstimateId.ToString();
                RevisionNo = header.RevisionNumber.ToString()+"("+header.RevisionTypeCode+")";
                ContractType = header.ContractType;
                if (_setascontractstate && ContractType.StartsWith("PC"))
                {
                    _setaspcstate = true;
                    CustomerDocumentType = "Set As PC";
                }
                JobFlowType = header.JobFlowType;
                HomePrice = header.HomePrice.ToString("c");
                HomeName = header.HomeName;
                HomeRange = header.HomeRange;
                UpgradeValue = header.UpgradeValue.ToString("c");
                PromotionValue = header.PromotionValue.ToString("c");
                SiteCost = header.SiteWorkValue.ToString("c");
                TotalPrice = header.TotalPrice.ToString("c");
                TotalPriceExc = header.TotalPriceExc.ToString("c");
                TotalMargin = header.TotalMargin.ToString("c");
                TotalCostBTP = header.TotalCostBTP.ToString("c");
                TotalCostDBC = header.TotalCostDBC.ToString("c");
                Margin = header.Margin.ToString();
                MarginString = header.MarginString;
                TargetMargin = header.TargetMargin.ToString();
                Region = header.Region;
                LotAddress = GetLotAddress(header.LotNumber != null ? header.LotNumber.ToString() : "", header.StreetNumber != null ? header.StreetNumber.ToString() : "", header.StreetAddress != null ? header.StreetAddress : "", header.Suburb != null ? header.Suburb : "", header.State != null ? header.State : "", header.PostCode != null ? header.PostCode : "");
                EffectiveDate = header.EffectiveDate.ToString("dd/MM/yyyy");
                DepositDate=header.DepositDate.ToString("dd/MM/yyyy");
                PriceExpiryDate= header.PriceExpiryDate.ToString("dd/MM/yyyy");
                StdPriceHoldDays = header.StdPriceHoldDays;
                BasePriceExtensionDays = header.BasePriceExtensionDays;
                RevisedPriceHoldDays = header.ReversedPriceHoldDays;
                RequiredPBE5Percent = header.RequiredPBE5Percent;
                RequiredPBERollback = header.RequiredPBERollback;
                SalesConsultant = header.SalesConsultantName;
                SalesAcceptor = header.SalesAcceptor;
                SalesEstimator = header.SalesEstimator;
                DraftPerson = header.DraftPerson;
                SelectedPromotion = header.PromotionName;
                
                EstimateHeaderBrief = "Estimate Information -- Customer: " + CustomerName + " / Contract:" + ContractNo + " / Home:" + HomeName + " / Revision:" + RevisionNo + "        Click to see more..." ;

                if (header.RevisionTypeCode !=null && (header.RevisionTypeCode.ToUpper() == "STM-COL" || header.RevisionTypeCode.ToUpper() == "STM-ELE" || header.RevisionTypeCode.ToUpper() == "STM-PAV" || header.RevisionTypeCode.ToUpper() == "STM-TIL" || header.RevisionTypeCode.ToUpper() == "STM-DEC" || header.RevisionTypeCode.ToUpper() == "STM-CAR"))
                {
                    ShowHeaderFields = Visibility.Collapsed;
                }
                else
                {
                    ShowHeaderFields = Visibility.Visible;
                }

                if (header.StatusId == 1 && // Work In Progress
                    header.OwnerId == (App.Current as App).CurrentUserId // Current Login is the Owner of the Estimate 
                    && (App.Current as App).CurrentAction != "EDIT") // Not in Edit mode
                    EditVisible = true;
                else
                {
                    EditVisible = false;

                    // Only Sales Accept can reject Estimates
                    if ((App.Current as App).CurrentAction == "EDIT" && // in Edit mode
                        (header.RevisionTypeId == 2 || // Sales Accept revision
                        header.RevisionTypeId == 14 || // PVAR-CSC
                        header.RevisionTypeId == 18 || // BVAR-BSC
                        header.RevisionTypeId == 24))  // PSTM-CSC
                        RejectVisible = true;
                    else
                        RejectVisible = false;
                }

                Estimate.RevisionTypeId = header.RevisionTypeId;

                if (header != null && header.Opportunityid!=null)
                    SharePointFolderUrl = App.Current.Resources["sharepointpath"].ToString() + header.Opportunityid.ToString();
                //SharePointFolderUrl = @"http://mweb/departments/it/CustomerDocs/" + header.ContractNumber.ToString();

                
                if(!IsBusyEstimateInfo)
                  IsBusy = false;

                IsBusyEstimateInfo = false;
                //RefreshTab();
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetEstimateHeaderCompleted");
        }

        void mrsClient_AdditionalNotesTemplateAndProductsCompleted(object sender, GetAdditionalNotesTemplateAndProductsCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                this.NotesTemplateTree = e.Result;
                mrsClient.GetAdditionalNotesTemplateAndProductsCompleted -= new EventHandler<GetAdditionalNotesTemplateAndProductsCompletedEventArgs>(mrsClient_AdditionalNotesTemplateAndProductsCompleted);
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "AdditionalNotesTemplateAndProductsCompleted");
        }

        public void RefreshHeaderInfo()
        {
            IsBusyEstimateInfo = true;

            RetailSystemClient MRSclient = new RetailSystemClient();
            MRSclient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            MRSclient.GetEstimateHeaderCompleted += new EventHandler<GetEstimateHeaderCompletedEventArgs>(mrsClient_GetEstimateHeaderCompleted);
            MRSclient.GetEstimateHeaderAsync(EstimateList.SelectedEstimateRevisionId);

            bool isSalesEstimatingRevision = false;

            if ((App.Current as App).SelectedEstimateRevisionTypeID == 4 || //Sales Estimating
            (App.Current as App).SelectedEstimateRevisionTypeID == 15 || //Presite Variation - Sales Estimating
            (App.Current as App).SelectedEstimateRevisionTypeID == 25 || //Pre Studio M Variation - Sales Estimating
            (App.Current as App).SelectedEstimateRevisionTypeID == 19)  //Building Variation - Building Estimator
            {
                isSalesEstimatingRevision = true;
            }

            if ((App.Current as App).CurrentUserRoleId == 57 || (App.Current as App).CurrentUserRoleId == 6)
            { IsColMarginPercentAvailable = true; }
            else
            {
                IsColMarginPercentAvailable = ((App)App.Current).CurrentRoleAccessModule.AccessMarginModule && isSalesEstimatingRevision;
            }

            if (IsColMarginPercentAvailable)
            {
                ShowMargin = Visibility.Visible;
            }
            else
            {
                ShowMargin = Visibility.Collapsed;
            }

            ShowPrintMargin = ShowMargin;

            if (((App.Current as App).CurrentUserRoleId == 5 || (App.Current as App).CurrentUserRoleId == 6 || (App.Current as App).CurrentUserRoleId == 57) && ((App.Current as App).SelectedEstimateRevisionTypeID == 4 || (App.Current as App).SelectedEstimateRevisionTypeID == 15 || (App.Current as App).SelectedEstimateRevisionTypeID == 19 || (App.Current as App).SelectedEstimateRevisionTypeID == 25))// only SE, SE manager and ops manager is able to see in SE revison
            { 
                ShowEnterMargin= Visibility.Visible;
            }
            else
            {
                ShowEnterMargin = Visibility.Collapsed;
            }
        }

        public void SetVisibliltyOfGreenTickOnTabHeader()
        {
                bool shownonstandard=false;
                bool showsitework = false;
                bool showupgrade = false;
                bool showpreview = false;
                int pcount, ucount, ncount, scount, studiocount,siteworkcount,upgradecount,previewcount;
                pcount = 0;
                ucount = 0;
                ncount = 0;
                scount = 0;
                studiocount = 0;
                siteworkcount = 0;
                //if ((App.Current as App).CurrentUserRoleId == 62 || (App.Current as App).CurrentUserRoleId == 5)
                if (IsRoleAllowtoAccessThisRevision && (App.Current as App).SelectedEstimateAllowToAcceptItem)
                {
                    shownonstandard = true;
                    showsitework = true;
                    showupgrade = true;
                    showpreview = true;
                }

                foreach (EstimateDetails item in _upgradeSource)
                {

                    //if ((App.Current as App).CurrentUserRoleId == 62 || (App.Current as App).CurrentUserRoleId == 5)
                    if (IsRoleAllowtoAccessThisRevision && (App.Current as App).SelectedEstimateAllowToAcceptItem)
                    {

                        if (item.AreaId == 43)
                        {
                            if (!item.ItemAccepted)
                            {
                                shownonstandard = false;
                            }
                        }
                        if (item.SiteWorkItem)
                        {
                            if (!item.ItemAccepted)
                            {
                                showsitework = false;
                            }
                        }
                        if (item.AreaId != 43 && !item.SiteWorkItem)
                        {
                            if (!item.ItemAccepted)
                            {
                                showupgrade = false;
                            }
                        }
                    }
                    if (!item.StandardOption)
                    {
                        scount = scount + 1;
                    }
                    else // options
                    {
                        if (item.AreaId == 43)
                        {
                            ncount = ncount + 1;
                        }
                        //else if (item.AreaName.ToUpper().Contains("PROMOTION"))
                        //{
                        //    pcount = pcount + 1;
                        //}
                        else if (item.SiteWorkItem)
                        {
                            siteworkcount = siteworkcount + 1;
                        }
                        else
                        {
                            ucount = ucount + 1;
                        }
                    }

                    //if (item.StudioMProduct)
                    //{
                        studiocount = studiocount + 1;
                    //}
                }
                

                UpgradeTab.Count = "(" + ucount.ToString() + ")";
                //PromotionTab.Count = "(" + pcount.ToString() + ")";
                NonStandardTab.Count = "(" + ncount.ToString() + ")";
                StandardInclusionTab.Count = "(" + scount.ToString() + ")";
                SiteCostTab.Count = "(" + siteworkcount.ToString() + ")";
                if (PreviewTab != null)
                {
                    PreviewTab.Count = "(" + studiocount.ToString() + ")";
                }

                if (shownonstandard)
                {
                    NonStandardTab.Visible = Visibility.Visible;
                }
                else
                {
                    NonStandardTab.Visible = Visibility.Collapsed;
                }
                if (showsitework)
                {
                    SiteCostTab.Visible = Visibility.Visible;
                }
                else
                {
                    SiteCostTab.Visible = Visibility.Collapsed;
                }
                if (showupgrade)
                {
                    UpgradeTab.Visible = Visibility.Visible;
                }
                else
                {
                    UpgradeTab.Visible = Visibility.Collapsed;
                }
                if (shownonstandard && showsitework && showupgrade)
                {
                    PreviewTab.Visible = Visibility.Visible;
            }
                else
                {
                    PreviewTab.Visible = Visibility.Collapsed;
                }
        }
        
        public void GetNonStandardCategory()
        {
            //IsBusyEstimateInfo = true;

            //RetailSystemClient MRSclient = new RetailSystemClient();
            //MRSclient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            //mrsClient.GetNonstandardCategoryCompleted += new EventHandler<GetNonstandardCategoryCompletedEventArgs>(mrsClient_GetNonstandardCategoryCompleted);
            //mrsClient.GetNonstandardCategoryAsync();

            //mrsClient.GetNonstandardCategoryByStateCompleted += new EventHandler<GetNonstandardCategoryByStateCompletedEventArgs>(mrsClient_GetNonstandardCategoryByStateCompleted);
            //mrsClient.GetNonstandardCategoryByStateAsync(int.Parse(((App)(App.Current)).CurrentUserStateID), 0);
        }

        //public void mrsClient_GetNonstandardCategoryCompleted(object sender, GetNonstandardCategoryCompletedEventArgs e)
        //{
        //    EstimateNonStandardCategory = new ObservableCollection<NonStandardCategory>();
        //    if (e.Error == null)
        //    {
        //        this.EstimateNonStandardCategory = e.Result;
        //        mrsClient.GetNonstandardCategoryByStateCompleted -= new EventHandler<GetNonstandardCategoryByStateCompletedEventArgs>(mrsClient_GetNonstandardCategoryByStateCompleted);

        //        //GetNonStandardGroups(0);
        //    }
        //    else
        //        ExceptionHandler.PopUpErrorMessage(e.Error, "GetNonstandardCategoryCompleted");
        //}

        public void mrsClient_GetNonstandardCategoryByStateCompleted(object sender, GetNonstandardCategoryByStateCompletedEventArgs e)
        {
            EstimateNonStandardCategory = new ObservableCollection<NonStandardCategory>();
            if (e.Error == null)
            {
                this.EstimateNonStandardCategory = e.Result;
                mrsClient.GetNonstandardCategoryByStateCompleted -= new EventHandler<GetNonstandardCategoryByStateCompletedEventArgs>(mrsClient_GetNonstandardCategoryByStateCompleted);

                //GetNonStandardGroups(0);
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetNonstandardCategoryByStateCompleted");
        }
        //public void GetNonStandardGroups(int selectedareaid)
        //{
        //    mrsClient.GetNonstandardGroupsCompleted += new EventHandler<GetNonstandardGroupsCompletedEventArgs>(mrsClient_GetNonstandardGroupsCompleted);
        //    mrsClient.GetNonstandardGroupsAsync(selectedareaid, int.Parse(((App)(App.Current)).CurrentUserStateID));
        //}

        void mrsClient_GetNonstandardGroupsCompleted(object sender, GetNonstandardGroupsCompletedEventArgs e)
        {
            //EstimateNonStandardGroup = new ObservableCollection<NonStandardGroup>();
            if (e.Error == null)
            {
                EstimateNonStandardGroup=e.Result;
                mrsClient.GetNonstandardGroupsCompleted -= new EventHandler<GetNonstandardGroupsCompletedEventArgs>(mrsClient_GetNonstandardGroupsCompleted);
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetNonstandardGroupsCompleted");
            
        }

        public void GetPriceDisplayCodes()
        {
            mrsClient.GetPriceDisplayCodesCompleted += new EventHandler<GetPriceDisplayCodesCompletedEventArgs>(mrsClient_GetPriceDisplayCodesCompleted);
            mrsClient.GetPriceDisplayCodesAsync();
        }

        void mrsClient_GetPriceDisplayCodesCompleted(object sender, GetPriceDisplayCodesCompletedEventArgs e)
        {
            EstimateNonStandardPriceDisplayCode = new ObservableCollection<PriceDisplayCode>();
            if (e.Error == null)
            {
                this.EstimateNonStandardPriceDisplayCode = e.Result;
                mrsClient.GetPriceDisplayCodesCompleted -= new EventHandler<GetPriceDisplayCodesCompletedEventArgs>(mrsClient_GetPriceDisplayCodesCompleted);
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetPriceDisplayCodesCompleted");
        }

        public void GetSetDocumentTypeVisibility()
        {
            mrsClient.GetCustomerDocumentTypeCompleted += new EventHandler<GetCustomerDocumentTypeCompletedEventArgs>(mrsClient_GetCustomerDocumentTypeCompleted);
            mrsClient.GetCustomerDocumentTypeAsync(EstimateList.SelectedEstimateRevisionId);
        }

        void mrsClient_GetCustomerDocumentTypeCompleted(object sender, GetCustomerDocumentTypeCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    // Removed as per the new workflow procduce in APRIL 2018 
                    // (App.Current as App).CurrentUserStateID == "2" && 
                    // TBD - Add condition to hide contract for Town Planning by comparing Brand ID
                    if (string.Equals(e.Result, "PCContract"))
                    {
                        ShowSetDocumentType = Visibility.Visible;
                        CustomerDocumentType = "Set As PC";
                        ShowSetAsContract = Visibility.Visible;
                        _setaspcstate = true;
                        _setascontractstate = true;
                    }
                    else if (string.Equals(e.Result, "PC"))
                    {
                        ShowSetDocumentType = Visibility.Visible;
                        CustomerDocumentType = "Set As " + e.Result;
                        ShowSetAsContract = Visibility.Collapsed;
                        _setaspcstate = true;
                        _setascontractstate = true;
                    }
                    else if (string.Equals(e.Result, "Variation"))
                    {
                        ShowSetDocumentType = Visibility.Visible;
                        CustomerDocumentType = "Set As " + e.Result;
                        ShowSetAsContract = Visibility.Collapsed;
                    }
                    else if (string.Equals(e.Result, "STC") || string.Equals(e.Result, "STM") || string.Equals(e.Result, "Contract"))
                    {
                        ShowSetDocumentType = Visibility.Collapsed;
                        CustomerDocumentType = string.Empty;
                        ShowSetAsContract = Visibility.Visible;
                        _setascontractstate = true;
                    }
                    else if (string.Equals(e.Result, "PC-TP") || string.Equals(e.Result, "PC-CAS"))
                    {
                        ShowSetDocumentType = Visibility.Visible;
                        CustomerDocumentType = "Set As PC";
                        ShowSetAsContract = Visibility.Collapsed;
                        _setaspcstate = true;
                    }
                    else
                    {
                        ShowSetDocumentType = Visibility.Collapsed;
                        CustomerDocumentType = string.Empty;
                        ShowSetAsContract = Visibility.Collapsed;
                    }
                    _customerdocumenttypevalue = CustomerDocumentType;
                }
                else
                {
                    ShowSetDocumentType = Visibility.Collapsed;
                    CustomerDocumentType = string.Empty;
                    ShowSetAsContract = Visibility.Collapsed;
                    _customerdocumenttypevalue = string.Empty;
                }
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetPriceDisplayCodesCompleted");
        }

        public void RefreshTab()
        {
            IsBusy = true;
            IsBusyOptionTree = true;

            if (_upgradeSource.Count == 0)
            {
                RetailSystemClient MRSclient = new RetailSystemClient();
                MRSclient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

                MRSclient.GetEstimateDetailsCompleted += new EventHandler<GetEstimateDetailsCompletedEventArgs>(mrsClient_GetEstimateDetailsCompleted);
                MRSclient.GetEstimateDetailsAsync(EstimateList.SelectedEstimateRevisionId);
            }
            else
            {
                //Modified 7/12/2012
                //if (NonStandardTab != null && NonStandardTab.Options.Count > 0)
                //    NonStandardTab.Options.Clear();
                //if (SiteCostTab != null && SiteCostTab.Options.Count > 0)
                //    SiteCostTab.Options.Clear();
                //if (UpgradeTab != null && UpgradeTab.Options.Count > 0)
                //    UpgradeTab.Options.Clear();
                //if (StandardInclusionTab != null && StandardInclusionTab.Options.Count > 0)
                //    StandardInclusionTab.Options.Clear();
                //if (PreviewTab != null && PreviewTab.Options.Count > 0)
                //    PreviewTab.Options.ClearPreviewTab

                mrsClient_GetEstimateDetailsCompleted(null, null);
            }

        }

        public void SetDefaultAnswerToStudioMProducts(string action)
        {
            IsBusy = true;
            IsBusyEstimateInfo = true;

            savemode = action;
            mrsClient.GetItemsNeedSetDefaultAnswerCompleted += new EventHandler<GetItemsNeedSetDefaultAnswerCompletedEventArgs>(mrsClient_GetItemsNeedSetDefaultAnswerCompleted);
            mrsClient.GetItemsNeedSetDefaultAnswerAsync(EstimateList.SelectedEstimateRevisionId.ToString());
            
        }
        void mrsClient_GetItemsNeedSetDefaultAnswerCompleted(object sender, GetItemsNeedSetDefaultAnswerCompletedEventArgs ev)
        {
            if (ev != null)
            {
                if (ev.Error == null)
                {
                   mrsClient.GetItemsNeedSetDefaultAnswerCompleted -= new EventHandler<GetItemsNeedSetDefaultAnswerCompletedEventArgs>(mrsClient_GetItemsNeedSetDefaultAnswerCompleted);
                   if (savemode.Trim().ToUpper() == "ALL")
                   {
                       SetAllStudioMAnswer(ev.Result);
                   }
                   else
                   {
                       SetStudioMAnswer(ev.Result);
                   }
                }
                else
                    ExceptionHandler.PopUpErrorMessage(ev.Error, "GetItemsNeedSetDefaultAnswerCompleted");

            }

        }
        private void SetAllStudioMAnswer(ObservableCollection<StudioMItem> studiomproducts)
        {
            string answerXML = "";
            string questionheader="";
            string brandheader = "";
            bool morethanonebrand = false;
            bool morethanoneanswer = false;
            string idstring = "";
            string studiomstring = "";
            foreach (StudioMItem item in studiomproducts)
            {
                answerXML = "";
                try
                {
                    XDocument doc = new XDocument();
                    doc = XDocument.Parse(item.StudioMQestion);

                    IEnumerable<XElement> el = (from p in doc.Descendants("Brand")
                                                orderby (string)p.Attribute("name")
                                                select p);
                    //if (el.Count() > 1)
                    //{
                    //    morethanonebrand=true;
                    //    //break;
                    //}
                    //else // only set answer for products only have one brand
                    //{
                        brandheader= @"<Brands>";
                        //foreach (XElement sup in el)
                        //{
                            
                            string supplierbrandid = el.First().Attribute("id").Value;
                            string suppliername = el.First().Attribute("name").Value;
                            IEnumerable<XElement> question = (from q in doc.Descendants("Question") 
                                                              where (string)q.Parent.Parent.Attribute("id") == supplierbrandid &&
                                                                    (string)q.Attribute("mandatory")=="1"
                                                              select q);

                            brandheader = brandheader + @"<Brand id=""" + supplierbrandid + @""" name=""" + suppliername.Replace(@"""", @"&quot;").Replace(@"&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("'", "&apos;") + @""">";
                            answerXML = answerXML + brandheader;


                            answerXML =answerXML+@"<Questions>";

                            foreach (XElement qu in question)
                            {
                                string questionid = qu.Attribute("id").Value;
                                string questiontext = qu.Attribute("text").Value;
                                string questiontype = qu.Attribute("type").Value;

                                questionheader = @"<Question id=""" + questionid + @""" text=""" + questiontext.Replace(@"""", @"&quot;") + @""" type=""" + questiontype.Replace(@"""", @"&quot;").Replace(@"&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("'", "&apos;") + @""">";
                                answerXML = answerXML + questionheader;

                                if (questiontype.ToUpper() == "SINGLE SELECTION" || questiontype.ToUpper() == "MULTIPLE SELECTION")
                                {

                                    IEnumerable<XElement> answer = (IEnumerable<XElement>)(from aw in doc.Descendants("Answer")
                                                                    where (string)aw.Parent.Parent.Attribute("id") == questionid &&
                                                                    (string)aw.Parent.Parent.Parent.Parent.Attribute("id") == supplierbrandid
                                                                    orderby (string)aw.Attribute("text")
                                                                    select aw);

                                    //if (answer.Count() > 1)
                                    //{
                                    //    morethanoneanswer = true;
                                    //    //answerXML = answerXML.Replace(questionheader, "");
                                    //    answerXML="";
                                    //    break;   
                                    //}
                                    //else
                                    //{

                                    //    foreach (XElement aw in answer)
                                    //    {

                                            string answerid = answer.First().Attribute("id").Value;
                                            string answertext = answer.First().Attribute("text").Value;

                                            answerXML = answerXML + @"<Answers>";
                                            answerXML = answerXML + @"<Answer id=""" + answerid + @""" text=""" + answertext.Replace(@"""", @"&quot;").Replace(@"&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("'", "&apos;") + @"""/>";
                                            answerXML = answerXML + @"</Answers>";
                                            answerXML = answerXML + @"</Question>";
                                    //    }
                                    //}
                                }
                                else if (questiontype.ToUpper() == "FREE TEXT")
                                {
                                    answerXML = answerXML + @"<Answers>";
                                    answerXML = answerXML + @"<Answer id=""0"" text=""Test Free text""/>";
                                    answerXML = answerXML + @"</Answers>";
                                    answerXML = answerXML + @"</Question>";
                                }
                                else
                                {
                                    answerXML = "";
                                    //answerXML = answerXML.Replace(questionheader, "");// if question is not single selection then remove question from xml string
                                    //questionheader = ""; 
                                    break;
                                }

                            }
                            if (answerXML.Contains("<Questions>"))
                            {
                                answerXML = answerXML + @"</Questions>";
                            }
                             
                        //}
                        if (answerXML.Contains("<Questions>"))
                        {
                            answerXML = answerXML + "</Brand></Brands>";
                        }
                    //}
                    if (answerXML.Contains("<Questions></Questions>"))
                    {
                        answerXML = "";
                    }
                    item.StudioMDefaultAnswer = answerXML;
                }
                catch (Exception ex)
                {
                    item.StudioMDefaultAnswer = "";
                }

                

            }


            //construct Q/A string
            _totaldefaultanswer = 0;
            foreach (StudioMItem sm in studiomproducts)
            {
                if (sm.StudioMDefaultAnswer.Trim() != "")
                {
                    _totaldefaultanswer = _totaldefaultanswer + 1;
                    if (idstring == "")
                    {
                        idstring = sm.MRSEstimateDetailsId.ToString();
                        studiomstring = sm.StudioMDefaultAnswer;
                    }
                    else
                    {
                        idstring = idstring+","+ sm.MRSEstimateDetailsId.ToString();
                        studiomstring = studiomstring + "^" + sm.StudioMDefaultAnswer;
                    }
                }
            }
            if (idstring != "")
            {
                mrsClient.SetDefaultAnswerForEstimateRevisionCompleted += new EventHandler<SetDefaultAnswerForEstimateRevisionCompletedEventArgs>(mrsClient_SetDefaultAnswerForEstimateRevisionCompleted);
                mrsClient.SetDefaultAnswerForEstimateRevisionAsync(idstring, studiomstring, (App.Current as App).CurrentUserId.ToString());
            }
            else
            {
                string message = "No questions were answered.";
                RadWindow.Alert(message);
                IsBusy = false;
                IsBusyEstimateInfo = false;
            }

        }

        private void SetStudioMAnswer(ObservableCollection<StudioMItem> studiomproducts)
        {
            string answerXML = "";
            string questionheader = "";
            string brandheader = "";
            bool morethanonebrand = false;
            bool morethanoneanswer = false;
            string idstring = "";
            string studiomstring = "";
            foreach (StudioMItem item in studiomproducts)
            {
                answerXML = "";
                try
                {
                    XDocument doc = new XDocument();
                    doc = XDocument.Parse(item.StudioMQestion);

                    IEnumerable<XElement> el = (from p in doc.Descendants("Brand")
                                                orderby (string)p.Attribute("name")
                                                select p);
                    if (el.Count() > 1)
                    {
                        morethanonebrand = true;
                        //break;
                    }
                    else // only set answer for products only have one brand
                    {
                        brandheader = @"<Brands>";
                        foreach (XElement sup in el)
                        {
                            string supplierbrandid = sup.Attribute("id").Value;
                            string suppliername = sup.Attribute("name").Value;
                            IEnumerable<XElement> question = (from q in doc.Descendants("Question")
                                                              where (string)q.Parent.Parent.Attribute("id") == supplierbrandid &&
                                                                    (string)q.Attribute("mandatory") == "1"
                                                              select q);

                            brandheader = brandheader + @"<Brand id=""" + supplierbrandid + @""" name=""" + suppliername.Replace(@"""", @"&quot;").Replace(@"&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("'", "&apos;") + @""">";
                            answerXML = answerXML + brandheader;


                            answerXML = answerXML + @"<Questions>";

                            foreach (XElement qu in question)
                            {
                                string questionid = qu.Attribute("id").Value;
                                string questiontext = qu.Attribute("text").Value;
                                string questiontype = qu.Attribute("type").Value;

                                if (questiontype.ToUpper() == "SINGLE SELECTION")
                                {
                                    questionheader = @"<Question id=""" + questionid + @""" text=""" + questiontext.Replace(@"""", @"&quot;") + @""" type=""" + questiontype.Replace(@"""", @"&quot;").Replace(@"&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("'", "&apos;") + @""">";
                                    answerXML = answerXML + questionheader;
                                    IEnumerable<XElement> answer = (from aw in doc.Descendants("Answer")
                                                                    where (string)aw.Parent.Parent.Attribute("id") == questionid &&
                                                                    (string)aw.Parent.Parent.Parent.Parent.Attribute("id") == supplierbrandid
                                                                    orderby (string)aw.Attribute("text")
                                                                    select aw);

                                    if (answer.Count() > 1)
                                    {
                                        morethanoneanswer = true;
                                        //answerXML = answerXML.Replace(questionheader, "");
                                        answerXML = "";
                                        break;
                                    }
                                    else
                                    {

                                        foreach (XElement aw in answer)
                                        {

                                            string answerid = aw.Attribute("id").Value;
                                            string answertext = aw.Attribute("text").Value;

                                            answerXML = answerXML + @"<Answers>";
                                            answerXML = answerXML + @"<Answer id=""" + answerid + @""" text=""" + answertext.Replace(@"""", @"&quot;").Replace(@"&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("'", "&apos;") + @"""/>";
                                            answerXML = answerXML + @"</Answers>";
                                            answerXML = answerXML + @"</Question>";
                                        }
                                    }
                                }
                                else
                                {
                                    answerXML = "";
                                    //answerXML = answerXML.Replace(questionheader, "");// if question is not single selection then remove question from xml string
                                    //questionheader = ""; 
                                    break;
                                }

                            }
                            if (answerXML.Contains("<Questions>"))
                            {
                                answerXML = answerXML + @"</Questions>";
                            }

                        }
                        if (answerXML.Contains("<Questions>"))
                        {
                            answerXML = answerXML + "</Brand></Brands>";
                        }
                    }
                    if (answerXML.Contains("<Questions></Questions>"))
                    {
                        answerXML = "";
                    }
                    item.StudioMDefaultAnswer = answerXML;
                }
                catch (Exception ex)
                {
                    item.StudioMDefaultAnswer = "";
                }



            }


            //construct Q/A string
            _totaldefaultanswer = 0;
            foreach (StudioMItem sm in studiomproducts)
            {
                if (sm.StudioMDefaultAnswer.Trim() != "")
                {
                    _totaldefaultanswer = _totaldefaultanswer + 1;
                    if (idstring == "")
                    {
                        idstring = sm.MRSEstimateDetailsId.ToString();
                        studiomstring = sm.StudioMDefaultAnswer;
                    }
                    else
                    {
                        idstring = idstring + "," + sm.MRSEstimateDetailsId.ToString();
                        studiomstring = studiomstring + "^" + sm.StudioMDefaultAnswer;
                    }
                }
            }
            if (idstring != "")
            {
                mrsClient.SetDefaultAnswerForEstimateRevisionCompleted += new EventHandler<SetDefaultAnswerForEstimateRevisionCompletedEventArgs>(mrsClient_SetDefaultAnswerForEstimateRevisionCompleted);
                mrsClient.SetDefaultAnswerForEstimateRevisionAsync(idstring, studiomstring, (App.Current as App).CurrentUserId.ToString());
            }
            else
            {
                string message = "No questions were answered.";
                RadWindow.Alert(message);
                IsBusy = false;
                IsBusyEstimateInfo = false;
            }

        }

        void mrsClient_SetDefaultAnswerForEstimateRevisionCompleted(object sender, SetDefaultAnswerForEstimateRevisionCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                string message = _totaldefaultanswer.ToString() + " products were answered.";
                RadWindow.Alert(message);
                mrsClient.SetDefaultAnswerForEstimateRevisionCompleted -= new EventHandler<SetDefaultAnswerForEstimateRevisionCompletedEventArgs>(mrsClient_SetDefaultAnswerForEstimateRevisionCompleted);
                ForceRefreshTab();
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetPriceDisplayCodesCompleted");

            IsBusy = false;
            IsBusyEstimateInfo = false;

        }

        #region test set all studio m answer

        #endregion

        //For refreshing the whole estimate when estimate was updated from the Validation Message page
        public void ForceRefreshTab()
        {
            IsBusy = true;
            IsBusyOptionTree = true;

            RetailSystemClient MRSclient = new RetailSystemClient();
            MRSclient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            MRSclient.GetEstimateDetailsCompleted += new EventHandler<GetEstimateDetailsCompletedEventArgs>(mrsClient_ForceGetEstimateDetailsCompleted);
            MRSclient.GetEstimateDetailsAsync(EstimateList.SelectedEstimateRevisionId);
        }

        void mrsClient_ForceGetEstimateDetailsCompleted(object sender, GetEstimateDetailsCompletedEventArgs e)
        {
            if (e != null)
            {
                if (e.Error == null)
                {
                    _upgradeSource.Clear();
                    _upgradeSource.AddRange(e.Result);
                }
                else
                    ExceptionHandler.PopUpErrorMessage(e.Error, "ForceGetEstimateDetailsCompleted");

            }

            switch (SelectedTabIndex)
            {
                case 0:
                    SiteCostTab.Options.Clear();
                    foreach (EstimateDetails item in _upgradeSource)
                    {
                        if (item.SiteWorkItem && item.StandardOption && item.AreaId != 43)
                        {
                            SiteCostTab.Options.Add(item);
                        }
                    }
                    break;
                case 1:
                    UpgradeTab.Options.Clear();
                    foreach (EstimateDetails item in _upgradeSource)
                    {
                        if (item.StandardOption && !item.SiteWorkItem && item.AreaId != 43)
                        {
                            UpgradeTab.Options.Add(item);
                        }
                    }
                    break;
                case 2:
                    NonStandardTab.Options.Clear();
                    foreach (EstimateDetails item in _upgradeSource)
                    {
                        if (item.AreaId == 43 && item.StandardOption)
                        {
                            NonStandardTab.Options.Add(item);
                        }
                    }
                    break;
                //case 3:
                //    StandardInclusionTab.Options.Clear();
                //    foreach (EstimateDetails item in _upgradeSource)
                //    {
                //        if (!item.StandardOption)
                //        {
                //            StandardInclusionTab.Options.Add(item);
                //        }
                //    }
                //    break;
                case 3:
                    PreviewTab.Options.Clear();
                    foreach (EstimateDetails item in _upgradeSource)
                    {
                        PreviewTab.Options.Add(item);
                    }
                    break;
                default:
                    break;
            }

            mrsClient.GetOptionTreeAsOptionTreeProductsCompleted += new EventHandler<GetOptionTreeAsOptionTreeProductsCompletedEventArgs>(mrsClient_GetOptionTreeAsOptionTreeProductsCompleted);
            mrsClient.GetOptionTreeAsOptionTreeProductsAsync(EstimateList.SelectedEstimateRevisionId.ToString());

            SetVisibliltyOfGreenTickOnTabHeader();
            IsBusyEstimateInfo = false;
            if (!IsBusyEstimateInfo)
            {
                IsBusy = false;
            }
            // Remove Event Handler;
            mrsClient.GetEstimateDetailsCompleted -= new EventHandler<GetEstimateDetailsCompletedEventArgs>(mrsClient_ForceGetEstimateDetailsCompleted);
        }

        void mrsClient_GetEstimateDetailsCompleted(object sender, GetEstimateDetailsCompletedEventArgs e)
        {
            if (e != null)
            {
                if (e.Error == null)
                    _upgradeSource.AddRange(e.Result); 
                else
                    ExceptionHandler.PopUpErrorMessage(e.Error, "GetEstimateDetailsCompleted");
            }

            (App.Current as App).SelectedTabIndexEstimateDetails = SelectedTabIndex;
            switch (SelectedTabIndex)
            {
                case 0:
                    SiteCostTab.Options.Clear();
                    foreach (EstimateDetails item in _upgradeSource)
                    {
                        if (item.SiteWorkItem && item.StandardOption && item.AreaId != 43)
                        {
                            SiteCostTab.Options.Add(item);
                        }
                    }
                    break;
                case 1:
                    UpgradeTab.Options.Clear();
                    foreach (EstimateDetails item in _upgradeSource)
                    {
                        if (item.StandardOption && !item.SiteWorkItem && item.AreaId != 43)
                        {
                            UpgradeTab.Options.Add(item);
                        }
                    }
                    break;
                case 2:
                    NonStandardTab.Options.Clear();
                    foreach (EstimateDetails item in _upgradeSource)
                    {
                        if (item.AreaId == 43 && item.StandardOption)
                        {
                            NonStandardTab.Options.Add(item);
                        }
                    }
                    break;
                //case 3:
                //    StandardInclusionTab.Options.Clear();
                //    foreach (EstimateDetails item in _upgradeSource)
                //    {
                //        if (!item.StandardOption)
                //        {
                //            StandardInclusionTab.Options.Add(item);
                //        }
                //    }
                //    break;
                case 3:
                    PreviewTab.Options.Clear();
                    foreach (EstimateDetails item in _upgradeSource)
                    {
                        PreviewTab.Options.Add(item);
                    }
                    break;
                default:
                    break;
            }
           
 
            
            if (_optiontreeSource.Count == 0)
            {
                /* Modified 20130725 to speed up data download 
                mrsClient.GetOptionTreeAsEstimateDetailsCompleted += new EventHandler<GetOptionTreeAsEstimateDetailsCompletedEventArgs>(mrsClient_GetOptionTreeAsEstimateDetailsCompleted);
                mrsClient.GetOptionTreeAsEstimateDetailsAsync(EstimateList.SelectedEstimateRevisionId.ToString());
                */

                mrsClient.GetOptionTreeAsOptionTreeProductsCompleted += new EventHandler<GetOptionTreeAsOptionTreeProductsCompletedEventArgs>(mrsClient_GetOptionTreeAsOptionTreeProductsCompleted);
                mrsClient.GetOptionTreeAsOptionTreeProductsAsync(EstimateList.SelectedEstimateRevisionId.ToString());
            }
            else
            {
                IsBusyOptionTree = true;

                if (!optionTreeWorker.IsBusy)
                    // Build Option Tree in another thread 
                    optionTreeWorker.RunWorkerAsync();

            }
            SetVisibliltyOfGreenTickOnTabHeader();
            IsBusyEstimateInfo = false;
            if (!IsBusyEstimateInfo)
            {
                IsBusy = false;
            }
            // Remove Event Handler;
            mrsClient.GetEstimateDetailsCompleted -= new EventHandler<GetEstimateDetailsCompletedEventArgs>(mrsClient_GetEstimateDetailsCompleted);
        }

        void mrsClient_GetOptionTreeAsOptionTreeProductsCompleted(object sender, GetOptionTreeAsOptionTreeProductsCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                _optiontreeSource = new List<EstimateDetails>();
                
                List<EstimateDetails> optiontreeTemp = new List<EstimateDetails>();

                foreach (OptionTreeProducts p in e.Result)
                {
                    EstimateDetails ed = new EstimateDetails();
                    ed.ProductDescription = p.D;
                    ed.ProductName = p.N;
                    ed.Price = p.P;
                    ed.AreaName = p.A;
                    ed.GroupName = p.G;
                    ed.AreaId = p.AI;
                    ed.GroupId = p.GI;
                    ed.StandardOption = p.S;
                    ed.Quantity = p.Q;
                    ed.TotalPrice = ed.Quantity * ed.Price;
                    ed.HomeDisplayOptionId = p.I;
                    ed.SiteWorkItem = p.W;
                    ed.StudioMSortOrder = p.O;
                    ed.Uom = p.U;
                    //ed.CostExcGST = p.CG;
                    //ed.DerivedCost = p.DC;

                    if (p.M == 0) // Not Studio M
                    {
                        ed.StudioMProduct = false;
                    }
                    else if (p.M == 1) // Studio M Manadatory
                    {
                        ed.StudioMProduct = true;
                        ed.StudioMIcon = "./images/color_swatch.png";
                        ed.StudioMTooltips = "Studio M Product. Question not answered yet.";
                    }
                    else if (p.M == 2) // Studio M No Question
                    {
                        ed.StudioMProduct = true;
                        ed.StudioMIcon = "./images/green_box.png";
                        ed.StudioMTooltips = "There are no studio M questions.";
                    }
                    else if (p.M == 3) // Studio M Non-Mandatory
                    {
                        ed.StudioMProduct = true;
                        ed.StudioMIcon = "./images/color_swatch_gray.png";
                        ed.StudioMTooltips = "Studio M Product. Answers are not mandatory.";
                    }

                    if (p.S)
                    {
                        ed.SOSI = "./images/upgrade.png";
                        ed.SOSIToolTips = "Upgrade Option.";
                    }

                    //if (p.DC)
                    //{
                    //    ed.DerivedCostIcon = "./images/link.png";
                    //    ed.DerivedCostTooltips = "Derived Cost.";
                    //}
                    //else
                    //{
                    //    ed.DerivedCostIcon = "./images/spacer.gif";
                    //    ed.DerivedCostTooltips = "";
                    //}
                    optiontreeTemp.Add(ed);
                }

                // Copy full option tree to re-use later
                _optiontreeSource.AddRange(optiontreeTemp);

                if (!optionTreeWorker.IsBusy)
                    // Build Option Tree in another thread 
                    optionTreeWorker.RunWorkerAsync();
            }
            else
            {
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetOptionTreeAsOptionTreeProductsCompleted");
            }

            IsBusyOptionTree = false;

            // Remove Event Handler
            mrsClient.GetOptionTreeAsOptionTreeProductsCompleted -= new EventHandler<GetOptionTreeAsOptionTreeProductsCompletedEventArgs>(mrsClient_GetOptionTreeAsOptionTreeProductsCompleted);

        }

        public void SaveOptionFromTree(EstimateDetails pag)
        {
            IsBusy = true;
            IsBusyOptionTree = true;

            RetailSystemClient MRSclient = new RetailSystemClient();
            MRSclient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            MRSclient.SaveSelectedItemCompleted += delegate(object o, SaveSelectedItemCompletedEventArgs es)
            {
                if (es.Error == null)
                {
                    if (es.Result > 0)
                    {
                        pag.EstimateRevisionDetailsId = es.Result; // Specify the ID of the Sales Estimate Details 
                        pag.CreatedByUserId = (App.Current as App).CurrentUserId; // Set CreatedBy to the Current User 
                        pag.CreatedByUserManagerIds = (App.Current as App).CurrentUserId.ToString(); // Set Manager to the Current User 
                        pag.Changes = "NEW";

                        this.OptionTree.Remove(pag);
                        _optiontreeSource.Remove(pag);
                        _upgradeSource.Add(pag);

                        switch (SelectedTabIndex)
                        {
                            case 0:
                                SiteCostTab.Options.Add(pag);
                                break;
                            case 1:
                                UpgradeTab.Options.Add(pag);
                                break;
                            case 2:
                                NonStandardTab.Options.Add(pag);
                                break;
                            //case 3:
                            //    StandardInclusionTab.Options.Add(pag);
                            //    break;
                            case 3:
                                PreviewTab.Options.Add(pag);
                                break;
                            default:
                                break;
                        }
                        //RefreshTab();
                        RefreshHeaderInfo();
                        SetVisibliltyOfGreenTickOnTabHeader();
                    }
                }
                else
                    ExceptionHandler.PopUpErrorMessage(es.Error, "SaveSelectedItemCompleted");

                IsBusy = false;
                IsBusyOptionTree = false;
                IsBusyEstimateInfo = false;
            };

            MRSclient.SaveSelectedItemAsync(pag.HomeDisplayOptionId, EstimateList.SelectedEstimateRevisionId, pag.ProductAreaGroupID, _userid);
        }

        public void CopyItemFromOptionTreeToEstimate(EstimateDetails pag)
        {
            IsBusy = true;
            bool NSRavailable = false;
            // check if there is any NSR avaialbe in the option tree
            foreach (EstimateDetails ed in _optiontreeSource)
            {
                if (ed.AreaId == 43 && ed.GroupId == 24)
                {
                    NSRavailable = true;
                    break;
                }
            }
    
            if (NSRavailable)
            {
                RetailSystemClient client = new RetailSystemClient();
                client.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

                EstimateDetails result = new EstimateDetails();

                client.CopyItemFromOptionTreeToEstimateCompleted += delegate(object o, CopyItemFromOptionTreeToEstimateCompletedEventArgs es)
                {
                    if (es.Error == null)
                    {
                        if (es.Result != null)
                        {
                            result.AreaName = es.Result.AreaName;
                            result.GroupName = es.Result.GroupName;
                            result.AreaId = es.Result.AreaId;
                            result.GroupId = es.Result.GroupId;
                            result.ProductName = es.Result.ProductName;
                            result.ProductId = es.Result.ProductId;
                            result.HomeDisplayOptionId = es.Result.HomeDisplayOptionId;
                            result.NonstandardCategoryID = es.Result.NonstandardCategoryID;
                            result.NonstandardGroupID = es.Result.NonstandardGroupID;
                            result.EstimateRevisionDetailsId = es.Result.EstimateRevisionDetailsId;
                            result.Uom = es.Result.Uom;

                            result.ProductDescription = es.Result.ProductDescription;
                            result.UpdatedProductDescription = es.Result.ProductDescription;
                            result.ProductDescriptionShort = es.Result.ProductDescriptionShort;
                            result.ExtraDescription = es.Result.ExtraDescription;
                            result.UpdatedExtraDescription = es.Result.ExtraDescription;
                            result.InternalDescription = es.Result.InternalDescription;
                            result.UpdatedInternalDescription = es.Result.InternalDescription;
                            result.AdditionalNotes = es.Result.AdditionalNotes;
                            result.UpdatedAdditionalNotes = es.Result.AdditionalNotes;

                            result.Price = es.Result.Price;
                            result.UpdatedPrice = es.Result.Price;
                            result.Quantity = es.Result.Quantity;
                            result.UpdatedQuantity = es.Result.Quantity;
                            result.TotalPrice = es.Result.TotalPrice;
                            result.UpdatedTotalPrice = es.Result.UpdatedTotalPrice;
                            result.StandardOption = true;
                            result.SOSI = "./images/upgrade.png";
                            result.SOSIToolTips = "Upgrade Option.";
                            result.ItemAllowToRemove = true;
                            if (es.Result.DerivedCost)
                            {
                                result.DerivedCost = true;
                                result.DerivedCostIcon = "./images/link.png";
                                result.DerivedCostTooltips = "Derived cost.";
                            }
                            else
                            {
                                result.DerivedCost = true;
                                result.DerivedCostIcon = "./images/spacer.gif";
                                result.DerivedCostTooltips = "";
                            }
                            result.CostExcGST = es.Result.CostExcGST;
                            result.UpdatedBTPCostExcGST = es.Result.UpdatedBTPCostExcGST;
                            result.UpdatedDBCCostExcGST = es.Result.UpdatedDBCCostExcGST;
                            result.Margin = es.Result.Margin;
                            result.MarginDBCCost = es.Result.MarginDBCCost;
                            //result.ItemAllowToChangePrice = true;
                            //result.ItemAllowToChangeQuantity = true;
                            result.ItemAllowToChangePrice = true;
                            result.ItemAllowToChangeQuantity = true;
                            result.ItemAllowToChangeDisplayCode = es.Result.ItemAllowToChangeDisplayCode;
                            result.ItemAllowToChangeDescription = es.Result.ItemAllowToChangeDescription;



                            if (SelectedTabIndex == 2) // if copy from NSR to NSR, copy areaid and groupid
                            {
                                result.AreaName = es.Result.AreaName;
                                result.NonstandardCategoryID = es.Result.NonstandardCategoryID;
                                result.NonstandardGroupID = es.Result.NonstandardGroupID;
                            }

                            result.CreatedByUserId = (App.Current as App).CurrentUserId; // Set CreatedBy to the Current User
                            result.CreatedByUserManagerIds = (App.Current as App).CurrentUserId.ToString(); // Set Manager to the Current User 

                            result.PriceDisplayCodeDesc = es.Result.PriceDisplayCodeDesc;
                            result.PriceDisplayCodeId = es.Result.PriceDisplayCodeId;
                            result.Changes = "NEW";
                            //this.OptionTree.Remove(result);
                            //this.UpgradeTab.Options.Add(result);
                            _upgradeSource.Add(result);

                            if (SelectedTabIndex == 2) // NSR tab
                            {
                                NonStandardTab.Options.Add(result);
                            }
                            else if (SelectedTabIndex == 3) //Preview tab
                            {
                                PreviewTab.Options.Add(result);
                            }

                            foreach (EstimateDetails item in _optiontreeSource)
                            {
                                if (item.HomeDisplayOptionId == result.HomeDisplayOptionId)
                                {
                                    _optiontreeSource.Remove(item);
                                    this.OptionTree.Remove(item);

                                    break;
                                }
                            }


                            RefreshHeaderInfo();
                            SetVisibliltyOfGreenTickOnTabHeader();
                        }
                        else
                            ExceptionHandler.PopUpErrorMessage(new Exception("Copy Item Failed"), "CopyItemFromOptionTreeToEstimateCompleted");
                    }
                    else
                        ExceptionHandler.PopUpErrorMessage(es.Error, "CopyItemFromOptionTreeToEstimateCompleted");

                    IsBusy = false;
                };

                client.CopyItemFromOptionTreeToEstimateAsync(pag.HomeDisplayOptionId, pag.EstimateRevisionDetailsId, EstimateList.SelectedEstimateRevisionId, pag.ProductAreaGroupID, _userid);
            }
            else
            {
                RadWindow.Alert("There are no Non Standard Products available, Copy function cannot be used!");
                IsBusy = false;
            }
        }

        public void CopyItemFromMasterHomeToEstimate(EstimateDetails pag)
        {
            IsBusy = true;
            bool NSRavailable = false;
            // check if there is any NSR avaialbe in the option tree
            foreach (EstimateDetails ed in _optiontreeSource)
            {
                if (ed.AreaId == 43 && ed.GroupId == 24)
                {
                    NSRavailable = true;
                    break;
                }
            }

            if (NSRavailable)
            {
                RetailSystemClient client = new RetailSystemClient();
                client.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

                EstimateDetails result = new EstimateDetails();

                client.CopyItemFromMasterHomeToEstimateCompleted += delegate(object o, CopyItemFromMasterHomeToEstimateCompletedEventArgs es)
                {
                    if (es.Error == null)
                    {
                        if (es.Result != null)
                        {
                            result.AreaName = es.Result.AreaName;
                            result.GroupName = es.Result.GroupName;
                            result.AreaId = es.Result.AreaId;
                            result.GroupId = es.Result.GroupId;
                            result.ProductName = es.Result.ProductName;
                            result.ProductId = es.Result.ProductId;
                            result.HomeDisplayOptionId = es.Result.HomeDisplayOptionId;
                            result.NonstandardCategoryID = es.Result.NonstandardCategoryID;
                            result.NonstandardGroupID = es.Result.NonstandardGroupID;
                            result.EstimateRevisionDetailsId = es.Result.EstimateRevisionDetailsId;
                            result.Uom = es.Result.Uom;

                            result.ProductDescription = es.Result.ProductDescription;
                            result.UpdatedProductDescription = es.Result.ProductDescription;
                            result.ProductDescriptionShort = es.Result.ProductDescriptionShort;
                            result.ExtraDescription = es.Result.ExtraDescription;
                            result.UpdatedExtraDescription = es.Result.ExtraDescription;
                            result.InternalDescription = es.Result.InternalDescription;
                            result.UpdatedInternalDescription = es.Result.InternalDescription;
                            result.AdditionalNotes = es.Result.AdditionalNotes;
                            result.UpdatedAdditionalNotes = es.Result.AdditionalNotes;

                            result.Price = es.Result.Price;
                            result.UpdatedPrice = es.Result.Price;
                            result.Quantity = es.Result.Quantity;
                            result.UpdatedQuantity = es.Result.Quantity;
                            result.TotalPrice = es.Result.TotalPrice;
                            result.UpdatedTotalPrice = es.Result.UpdatedTotalPrice;
                            result.StandardOption = true;
                            result.SOSI = "./images/upgrade.png";
                            result.SOSIToolTips = "Upgrade Option.";
                            result.ItemAllowToRemove = true;
                            if (es.Result.DerivedCost)
                            {
                                result.DerivedCost = true;
                                result.DerivedCostIcon = "./images/link.png";
                                result.DerivedCostTooltips = "Derived cost.";
                            }
                            else
                            {
                                result.DerivedCost = true;
                                result.DerivedCostIcon = "./images/spacer.gif";
                                result.DerivedCostTooltips = "";
                            }
                            result.CostExcGST = es.Result.CostExcGST;
                            result.UpdatedBTPCostExcGST = es.Result.UpdatedBTPCostExcGST;
                            result.UpdatedDBCCostExcGST = es.Result.UpdatedDBCCostExcGST;
                            result.Margin = es.Result.Margin;

                            result.ItemAllowToChangePrice = true;
                            result.ItemAllowToChangeQuantity = true;
                            result.ItemAllowToChangeDisplayCode = es.Result.ItemAllowToChangeDisplayCode;
                            result.ItemAllowToChangeDescription = es.Result.ItemAllowToChangeDescription;

                            result.CreatedByUserId = (App.Current as App).CurrentUserId; // Set CreatedBy to the Current User
                            result.CreatedByUserManagerIds = (App.Current as App).CurrentUserId.ToString(); // Set Manager to the Current User 

                            result.PriceDisplayCodeDesc = es.Result.PriceDisplayCodeDesc;
                            result.PriceDisplayCodeId = es.Result.PriceDisplayCodeId;
                            result.Changes = "NEW";
                            //this.OptionTree.Remove(result);
                            //this.UpgradeTab.Options.Add(result);
                            _upgradeSource.Add(result);

                            //remove from option tree
                            foreach (EstimateDetails pag2 in OptionTree)
                            {
                                if (pag2.HomeDisplayOptionId == result.HomeDisplayOptionId)
                                {
                                    pag2.EstimateRevisionDetailsId = result.EstimateRevisionDetailsId;
                                    this.OptionTree.Remove(pag2);
                                    _optiontreeSource.Remove(pag2);
                                    break;
                                }
                            }

                            if (SelectedTabIndex == 2) // NSR tab
                            {
                                NonStandardTab.Options.Add(result);
                            }
                            else if (SelectedTabIndex == 3) //Preview tab
                            {
                                PreviewTab.Options.Add(result);
                            }    

                            RefreshHeaderInfo();
                            SetVisibliltyOfGreenTickOnTabHeader();
                        }
                        else
                            ExceptionHandler.PopUpErrorMessage(new Exception("Copy Item Failed"), "CopyItemFromMasterHomeToEstimateCompleted");
                    }
                    else
                        ExceptionHandler.PopUpErrorMessage(es.Error, "CopyItemFromMasterHomeToEstimateCompleted");

                    IsBusy = false;
                };

                client.CopyItemFromMasterHomeToEstimateAsync(SelectedRegionId, pag.HomeDisplayOptionId, EstimateList.SelectedEstimateRevisionId, _userid);
            }
            else
            {
                RadWindow.Alert("There are no Non Standard Products available, Copy function cannot be used!");
                IsBusy = false;
            }
        }

        public void CopyItemFromAllProductsToEstimate(EstimateDetails pag)
        {
            IsBusy = true;
            bool NSRavailable = false;


            // check if there is any NSR avaialbe in the option tree
            foreach (EstimateDetails ed in _optiontreeSource)
            {
                if (ed.AreaId == 43 && ed.GroupId == 24)
                {
                    NSRavailable = true;
                    break;
                }
            }

            if (NSRavailable)
            {
                RetailSystemClient client = new RetailSystemClient();
                client.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

                EstimateDetails result = new EstimateDetails();

                client.CopyItemFromAllProductsToEstimateCompleted += delegate(object o, CopyItemFromAllProductsToEstimateCompletedEventArgs es)
                {
                    if (es.Error == null)
                    {
                        if (es.Result != null)
                        {
                            result.AreaName = es.Result.AreaName;
                            result.GroupName = es.Result.GroupName;
                            result.AreaId = es.Result.AreaId;
                            result.GroupId = es.Result.GroupId;
                            result.ProductName = es.Result.ProductName;
                            result.ProductId = es.Result.ProductId;
                            result.HomeDisplayOptionId = es.Result.HomeDisplayOptionId;
                            result.NonstandardCategoryID = es.Result.NonstandardCategoryID;
                            result.NonstandardGroupID = es.Result.NonstandardGroupID;
                            result.EstimateRevisionDetailsId = es.Result.EstimateRevisionDetailsId;
                            result.Uom = es.Result.Uom;

                            result.ProductDescription = es.Result.ProductDescription;
                            result.UpdatedProductDescription = es.Result.ProductDescription;
                            result.ProductDescriptionShort = es.Result.ProductDescriptionShort;
                            result.ExtraDescription = es.Result.ExtraDescription;
                            result.UpdatedExtraDescription = es.Result.ExtraDescription;
                            result.InternalDescription = es.Result.InternalDescription;
                            result.UpdatedInternalDescription = es.Result.InternalDescription;
                            result.AdditionalNotes = es.Result.AdditionalNotes;
                            result.UpdatedAdditionalNotes = es.Result.AdditionalNotes;

                            result.Price = es.Result.Price;
                            result.UpdatedPrice = es.Result.Price;
                            result.Quantity = es.Result.Quantity;
                            result.UpdatedQuantity = es.Result.Quantity;
                            result.TotalPrice = es.Result.TotalPrice;
                            result.UpdatedTotalPrice = es.Result.UpdatedTotalPrice;
                            result.StandardOption = true;
                            result.SOSI = "./images/upgrade.png";
                            result.SOSIToolTips = "Upgrade Option.";
                            result.ItemAllowToRemove = true;
                            if (es.Result.DerivedCost)
                            {
                                result.DerivedCost = true;
                                result.DerivedCostIcon = "./images/link.png";
                                result.DerivedCostTooltips = "Derived cost.";
                            }
                            else
                            {
                                result.DerivedCost = true;
                                result.DerivedCostIcon = "./images/spacer.gif";
                                result.DerivedCostTooltips = "";
                            }
                            result.CostExcGST = es.Result.CostExcGST;
                            result.UpdatedBTPCostExcGST = es.Result.UpdatedBTPCostExcGST;
                            result.UpdatedDBCCostExcGST = es.Result.UpdatedDBCCostExcGST;
                            result.Margin = es.Result.Margin;

                            result.ItemAllowToChangePrice = true;
                            result.ItemAllowToChangeQuantity = true;
                            result.ItemAllowToChangeDisplayCode = es.Result.ItemAllowToChangeDisplayCode;
                            result.ItemAllowToChangeDescription = es.Result.ItemAllowToChangeDescription;

                            result.CreatedByUserId = (App.Current as App).CurrentUserId; // Set CreatedBy to the Current User
                            result.CreatedByUserManagerIds = (App.Current as App).CurrentUserId.ToString(); // Set Manager to the Current User 

                            result.PriceDisplayCodeDesc = es.Result.PriceDisplayCodeDesc;
                            result.PriceDisplayCodeId = es.Result.PriceDisplayCodeId;
                            result.Changes = "NEW";
                            //this.OptionTree.Remove(result);
                            //this.UpgradeTab.Options.Add(result);
                            _upgradeSource.Add(result);

                            //remove from option tree
                            foreach (EstimateDetails pag2 in OptionTree)
                            {
                                if (pag2.HomeDisplayOptionId == result.HomeDisplayOptionId)
                                {
                                    pag2.EstimateRevisionDetailsId = result.EstimateRevisionDetailsId;
                                    this.OptionTree.Remove(pag2);
                                    _optiontreeSource.Remove(pag2);
                                    break;
                                }
                            }

                            if (SelectedTabIndex == 2) // NSR tab
                            {
                                NonStandardTab.Options.Add(result);
                            }
                            else if (SelectedTabIndex == 3) //Preview tab
                            {
                                PreviewTab.Options.Add(result);
                            }

                            RefreshHeaderInfo();
                            SetVisibliltyOfGreenTickOnTabHeader();
                        }
                        else
                            ExceptionHandler.PopUpErrorMessage(new Exception("Copy Item Failed"), "CopyItemFromAllProductsToEstimateCompleted");
                    }
                    else
                        ExceptionHandler.PopUpErrorMessage(es.Error, "CopyItemFromAllProductsToEstimateCompleted");

                    IsBusy = false;
                };

                client.CopyItemFromAllProductsToEstimateAsync(SelectedRegionId, pag.ProductId, EstimateList.SelectedEstimateRevisionId, _userid);
            }
            else
            {
                RadWindow.Alert("There are no Non Standard Products available, Copy function cannot be used!");
                IsBusy = false;
            }
        }

        //public void RemoveOptionFromEstimate(EstimateDetails pag)
        //{
        //    // obsolute, we may have to delete this procedure
        //    RetailSystemClient MRSclient = new RetailSystemClient();
        //    MRSclient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

        //    MRSclient.DeleteProductAsync(pag.EstimateRevisionDetailsId, "", 0, _userid);
        //    MRSclient.DeleteProductCompleted += delegate(object o, DeleteProductCompletedEventArgs es)
        //    {
        //        if (es.Error == null)
        //        {
        //            ForceRefreshTab();
        //        }
        //    };
        //}

        //public void RemoveOptionFromEstimate(EstimateDetails pag, string reason, int reasonid)
        //{
        //    RetailSystemClient MRSclient = new RetailSystemClient();
        //    MRSclient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

        //    MRSclient.DeleteProductAsync(pag.EstimateRevisionDetailsId, reason, reasonid, _userid);
        //    MRSclient.DeleteProductCompleted += delegate(object o, DeleteProductCompletedEventArgs es)
        //    {
        //        if (es.Error == null)
        //        {
        //            ForceRefreshTab();
        //        }
        //    };
        //}

        //public void RemoveOptionFromEstimate(EstimateDetails pag, string estimatedetailsidstring, string areaidstring, string groupidstring, string reason, int reasonid)
        //{
        //    RetailSystemClient MRSclient = new RetailSystemClient();
        //    MRSclient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

        //    MRSclient.DeleteProductsAsync(pag.EstimateRevisionDetailsId, estimatedetailsidstring, areaidstring, groupidstring, reason, reasonid, _userid);
        //    MRSclient.DeleteProductsCompleted += delegate(object o, DeleteProductsCompletedEventArgs es)
        //    {
        //        if (es.Error == null)
        //        {
        //            ForceRefreshTab();
        //        }
        //    };
        //}

        public void RemoveOptionFromList(EstimateDetails pag, List<EstimateDetails> removedpromotionitems, List<PromotionPAG> leftpromotionitems)
        {
            IsBusy = true;

            _upgradeSource.Remove(pag);

            switch (SelectedTabIndex)
            {
                case 0:
                    SiteCostTab.Options.Remove(pag);
                    break;
                case 1:
                    UpgradeTab.Options.Remove(pag);
                    break;
                case 2:
                    NonStandardTab.Options.Remove(pag);
                    break;
                //case 3:
                //    StandardInclusionTab.Options.Remove(pag);
                //    break;
                case 3:
                    PreviewTab.Options.Remove(pag);
                    break;
                default:
                    break;
            }

            if (pag.StudioMIcon == "./images/green_box.png")
            {
                if (pag.StudioMAnswerMandatory)
                {
                    pag.StudioMIcon = "./images/color_swatch.png";
                    pag.StudioMTooltips = "Studio M Product. Question not answered yet.";
                }
                else
                {
                    pag.StudioMIcon = "./images/color_swatch_gray.png";
                    pag.StudioMTooltips = "Studio M Product. Answers are not mandatory.";
                }
            }

            if (removedpromotionitems.Count > 0)
            {
                foreach (EstimateDetails ed2 in removedpromotionitems)
                {
                    foreach (EstimateDetails us in _upgradeSource)
                    {
                        if (us.EstimateRevisionDetailsId == ed2.EstimateRevisionDetailsId)
                        {
                            _upgradeSource.Remove(us);
                            switch (SelectedTabIndex)
                            {
                                case 0:
                                    SiteCostTab.Options.Remove(us);
                                    break;
                                case 1:
                                    UpgradeTab.Options.Remove(us);
                                    break;
                                case 2:
                                    NonStandardTab.Options.Remove(us);
                                    break;
                                //case 3:
                                //    StandardInclusionTab.Options.Remove(us);
                                //    break;
                                case 3:
                                    PreviewTab.Options.Remove(us);
                                    break;
                                default:
                                    break;
                            }
                            break;
                        }
                    }
                }
            }

            if (leftpromotionitems.Count > 0)
            {
                foreach (PromotionPAG ed4 in leftpromotionitems)
                {
                    foreach (EstimateDetails us2 in _upgradeSource)
                    {
                        if (us2.EstimateRevisionDetailsId == ed4.RevisionDetailsID)
                        {
                            us2.PromotionProduct=false;
                            us2.SOSI="./images/spacer.gif";
                            break;
                        }
                    }
                }
            }

            if (pag.AreaId == 43 && pag.GroupId == 24)
            {
                bool validProduct = true;

                RetailSystemClient client = new RetailSystemClient();
                client.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

                client.CheckValidProductByRevisionCompleted += delegate(object o, CheckValidProductByRevisionCompletedEventArgs es)
                {
                    if (es.Error == null)
                        validProduct = es.Result;
                    else
                        ExceptionHandler.PopUpErrorMessage(es.Error, "CheckValidProductByRevisionCompleted");

                    if (validProduct)
                    {
                        this.OptionTree.Add(pag);
                        _optiontreeSource.Add(pag);
                    }

                    //RefreshTab();
                    RefreshHeaderInfo();
                    SetVisibliltyOfGreenTickOnTabHeader();

                    IsBusy = false;
                };

                client.CheckValidProductByRevisionAsync(EstimateList.SelectedEstimateRevisionId, pag.ProductId);
            }
            else
            {
                this.OptionTree.Add(pag);
                _optiontreeSource.Add(pag);

                if (removedpromotionitems.Count > 0)
                {
                    foreach (EstimateDetails ed3 in removedpromotionitems)
                    {
                        ed3.PromotionProduct = false;
                        this.OptionTree.Add(ed3);
                        _optiontreeSource.Add(ed3);
                    }
                }

                //RefreshTab();
                RefreshHeaderInfo();
                SetVisibliltyOfGreenTickOnTabHeader();

                IsBusy = false;
            }
        }
        
        public void UpdateEstimateDetails(EstimateDetails pag, int applyanswertoallgroup)
        {
            IsBusy = true;
            int itemaccepted;

            RetailSystemClient MRSclient = new RetailSystemClient();
            MRSclient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            MRSclient.UpdateEstimateDetailsCompleted += delegate(object o, AsyncCompletedEventArgs es)
            {
                if (es.Error == null)
                {
                    //RefreshTab(); //to refresh Grid after 'Apply Studio M answers to all product with the same product id...'
                    RefreshHeaderInfo();
                    SetVisibliltyOfGreenTickOnTabHeader();
                    if (IsBusy)
                        IsBusy = false;
                    if (IsBusyEstimateInfo)
                        IsBusyEstimateInfo = false;
                }
                else
                    ExceptionHandler.PopUpErrorMessage(es.Error, "UpdateEstimateDetails");
            };
            
            pag.Price = pag.UpdatedPrice;
            pag.SiteWorkItem = pag.UpdatedSiteWorkItem;
            pag.Quantity = pag.UpdatedQuantity;
            pag.ProductDescription = pag.UpdatedProductDescription;
            pag.ExtraDescription = pag.UpdatedExtraDescription;
            pag.InternalDescription = pag.UpdatedInternalDescription;
            pag.AdditionalNotes = pag.UpdatedAdditionalNotes;
            pag.NonstandardCategoryID = pag.UpdatedNonstandardCategoryID;
            pag.NonstandardGroupID = pag.UpdatedNonstandardGroupID;
            pag.PriceDisplayCodeId = pag.UpdatedPriceDisplayCodeId;
            if (pag.UpdatedProductDescription.Length > 100)
            {
                pag.ProductDescriptionShort = pag.ProductDescriptionShort.Substring(0, 99);
            }
            
            //Only update PriceDisplayCode when the updated value is valid (NSR only)
            if (pag.UpdatedPriceDisplayCodeId > 0)
                pag.PriceDisplayCodeId = pag.UpdatedPriceDisplayCodeId;
            
            pag.Changed = true;
            if (pag.ItemAccepted)
            {
                itemaccepted = 1;
            }
            else
            {
                itemaccepted = 0;
            }

            MRSclient.UpdateEstimateDetailsAsync(pag.EstimateRevisionDetailsId, 
                pag.Price, 
                pag.Quantity, 
                pag.TotalPrice,
                pag.ProductDescription, 
                pag.ExtraDescription, 
                pag.InternalDescription, 
                pag.AdditionalNotes,
                pag.StudioMAnswer, 
                itemaccepted, 
                pag.NonstandardCategoryID, 
                pag.NonstandardGroupID, 
                pag.UpdatedPriceDisplayCodeId,
                _userid, 
                applyanswertoallgroup,
                pag.SelectedImageID,
                pag.SiteWorkItem,
                pag.UpdatedBTPCostExcGST,
                pag.UpdatedDBCCostExcGST
                );
        }

        public void SaveSelectedOptionsFromTreeToEstimate(string estimatedetailsidstring, 
            string standardinclusionidstring, 
            string revisionid, 
            string studiomanswer, 
            string userid, 
            string derivedcoststring, 
            string costbtpexcgststring,
            string costbtpoverwriteflagstring,
            string costdbcexcgststring,
            string costdbcoverwriteflagstring,
            string quantitystring,
            string pricestring,
            string isacceptedstring,
            string areaidstring,
            string groupidstring,
            string pricedisplaycodestring,
            string issiteworkstring,
            string productdescriptionstring,
            string additionalnotestring,
            string extradescriptionstring,
            string internaldescriptionstring)
        {
            IsBusy = true;
            IsBusyOptionTree = true;
            bool refreshheader = true;

            RetailSystemClient MRSclient = new RetailSystemClient();
            MRSclient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            MRSclient.SaveSelectedItemsFromOptionTreeToEstimateCompleted += delegate(object o, SaveSelectedItemsFromOptionTreeToEstimateCompletedEventArgs es)
            {
                if (es.Error == null)
                {
                    bool itemsAdded = false;
                    //string[] temp=estimatedetailsidstring.Split(',');
                    foreach (EstimateDetails ed in es.Result)
                    {
                        int a = ed.HomeDisplayOptionId;

                        foreach (EstimateDetails pag in OptionTree)
                        {
                            if (pag.HomeDisplayOptionId == ed.HomeDisplayOptionId && 
                                pag.StandardInclusionId == ed.StandardInclusionId)
                            {
                                pag.EstimateRevisionDetailsId = ed.EstimateRevisionDetailsId;
                                ed.CreatedByUserId = (App.Current as App).CurrentUserId; // Set CreatedBy to the Current User 
                                ed.CreatedByUserManagerIds = (App.Current as App).CurrentUserId.ToString(); // Set Manager to the Current User 
                                ed.Changes = "NEW";
                                itemsAdded = true;

                                this.OptionTree.Remove(pag);
                                _optiontreeSource.Remove(pag);

                                _upgradeSource.Add(ed);

                                switch (SelectedTabIndex)
                                {
                                    case 2:
                                        NonStandardTab.Options.Add(ed);
                                        break;
                                    case 0:
                                        SiteCostTab.Options.Add(ed);
                                        break;
                                    case 1:
                                        UpgradeTab.Options.Add(ed);
                                        break;
                                    //case 3:
                                    //    StandardInclusionTab.Options.Add(ed);
                                    //    break;
                                    case 3:
                                        PreviewTab.Options.Add(ed);
                                        break;
                                    default:
                                        break;
                                }

                                if (ed.IsMasterPromotion)
                                {
                                    refreshheader = false;
                                    //AutoSelectPromotionProducts(ed);
                                    _upgradeSource.Clear();
                                    RefreshTab();
                                    RefreshHeaderInfo();
                                }


                                break;
                            }
                        }
                        if (ed.IsMasterPromotion)
                        {
                            break;
                        }
                    }
                    //_upgradeSource.Clear();
                    if (refreshheader)
                    {
                        RefreshHeaderInfo();
                        if (!itemsAdded)
                        {
                            ForceRefreshTab();
                        }
                        SetVisibliltyOfGreenTickOnTabHeader();
                        IsBusy = false;
                        IsBusyOptionTree = false;
                    }

                }
                else
                    ExceptionHandler.PopUpErrorMessage(es.Error, "SaveSelectedItemsFromOptionTreeToEstimateCompleted");


            };

            MRSclient.SaveSelectedItemsFromOptionTreeToEstimateAsync(estimatedetailsidstring, 
                standardinclusionidstring, 
                revisionid, 
                studiomanswer, 
                userid, 
                "OPTIONTREE", 
                derivedcoststring, 
                costbtpexcgststring,
                costbtpoverwriteflagstring,
                costdbcexcgststring,
                costdbcoverwriteflagstring,
                quantitystring,
                pricestring,
                isacceptedstring,
                areaidstring,
                groupidstring,
                pricedisplaycodestring,
                issiteworkstring,
                productdescriptionstring,
                additionalnotestring,
                extradescriptionstring,
                internaldescriptionstring);
        }

        public void ReplaceEstimateItemsSaveSelectedOptionsFromTreeToEstimate(EstimateDetails deletedED, string optionidstring,
              string standardinclusionidstring,
              string revisionid,
              string studiomanswer,
              string userid,
              string derivedcoststring,
              string costbtpexcgststring,
              string costdbcexcgststring,
              string quantitystring,
              string pricestring,
              string isacceptedstring,
              string areaidstring,
              string groupidstring,
              string pricedisplaycodestring,
              string issiteworkstring,
              string productdescriptionstring,
              string additionalnotestring,
              string extradescriptionstring,
              string internaldescriptionstring,
              string copyquantity,
              string copyadditionalnotes,
              string copyextradescription,
              string copyinternalnotes
            )
        {
            IsBusy = true;
            IsBusyOptionTree = true;
            bool refreshheader = true;

            RetailSystemClient MRSclient = new RetailSystemClient();
            MRSclient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            MRSclient.ReplaceSaveSelectedItemsFromOptionTreeToEstimateCompleted += delegate(object o, ReplaceSaveSelectedItemsFromOptionTreeToEstimateCompletedEventArgs es)
            {
                if (es.Error == null)
                {
                    //string[] temp=estimatedetailsidstring.Split(',');
                    foreach (EstimateDetails ed in es.Result)
                    {
                        int a = ed.HomeDisplayOptionId;

                        foreach (EstimateDetails pag in OptionTree)
                        {
                            if (pag.HomeDisplayOptionId == ed.HomeDisplayOptionId &&
                                pag.StandardInclusionId == ed.StandardInclusionId)
                            {
                                pag.EstimateRevisionDetailsId = ed.EstimateRevisionDetailsId;
                                ed.CreatedByUserId = (App.Current as App).CurrentUserId; // Set CreatedBy to the Current User 
                                ed.CreatedByUserManagerIds = (App.Current as App).CurrentUserId.ToString(); // Set Manager to the Current User 
                                ed.Changes = "NEW";

                                this.OptionTree.Remove(pag);
                                _optiontreeSource.Remove(pag);

                                _upgradeSource.Add(ed);

                                switch (SelectedTabIndex)
                                {
                                    case 2:
                                        NonStandardTab.Options.Add(ed);
                                        break;
                                    case 0:
                                        SiteCostTab.Options.Add(ed);
                                        break;
                                    case 1:
                                        UpgradeTab.Options.Add(ed);
                                        break;
                                    //case 3:
                                    //    StandardInclusionTab.Options.Add(ed);
                                    //    break;
                                    case 3:
                                        PreviewTab.Options.Add(ed);
                                        break;
                                    default:
                                        break;
                                }

                                if (ed.IsMasterPromotion)
                                {
                                    refreshheader = false;
                                    //AutoSelectPromotionProducts(ed);
                                    _upgradeSource.Clear();
                                    RefreshTab();
                                    RefreshHeaderInfo();
                                }


                                break;
                            }
                        }

                    }
                    //_upgradeSource.Clear();
                    if (refreshheader)
                    {
                        List<EstimateDetails> RemovedPAG = new List<EstimateDetails>();
                        List<PromotionPAG> LeftPAG = new List<PromotionPAG>();
                        //RemoveOptionFromEstimate(deletedED, estimatedetailsidstring, areaidstring, groupidstring, "Replace Item", 6);
                        //RemoveOptionFromList(deletedED, RemovedPAG, LeftPAG);
                        ForceRefreshTab();

                        RefreshHeaderInfo();
                        //RefreshTab();
                        SetVisibliltyOfGreenTickOnTabHeader();
                        IsBusy = false;
                        IsBusyOptionTree = false;
                        SelectedTabIndex = 3;
                    }

                }
                else
                    ExceptionHandler.PopUpErrorMessage(es.Error, "SaveSelectedItemsFromOptionTreeToEstimateCompleted");


            };

            MRSclient.ReplaceSaveSelectedItemsFromOptionTreeToEstimateAsync(deletedED.EstimateRevisionDetailsId.ToString(), optionidstring,
                standardinclusionidstring,
                revisionid,
                studiomanswer,
                userid,
                "OPTIONTREE",
                derivedcoststring,
                costbtpexcgststring,
                costdbcexcgststring,
                quantitystring,
                pricestring,
                isacceptedstring,
                areaidstring,
                groupidstring,
                pricedisplaycodestring,
                issiteworkstring,
                productdescriptionstring,
                additionalnotestring,
                extradescriptionstring,
                internaldescriptionstring,
                copyquantity,
                copyadditionalnotes,
                copyextradescription,
                copyinternalnotes
                );
        }

        private void AutoSelectPromotionProducts(EstimateDetails pag)
        {
            IsBusy = true;
            IsBusyOptionTree = true;
            RetailSystemClient MRSclient = new RetailSystemClient();
            MRSclient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            MRSclient.GetPromotionProductByMasterPromotionRevisionDetailsIDCompleted += delegate(object o, GetPromotionProductByMasterPromotionRevisionDetailsIDCompletedEventArgs es)
            {
                if (es.Error == null)
                {
                    bool addnew;
                    // add to the screen and upgrade source
                    foreach (EstimateDetails ed in es.Result)
                    {
                        addnew = true;
                        foreach (EstimateDetails pag3 in _upgradeSource)
                        {
                            if (pag3.HomeDisplayOptionId == ed.HomeDisplayOptionId)
                            {
                                addnew = false;
                                break;
                            }
                        }
                        if (addnew)
                        {
                            _upgradeSource.Add(ed);

                            switch (SelectedTabIndex)
                            {
                                case 2:
                                    NonStandardTab.Options.Add(ed);
                                    break;
                                case 0:
                                    SiteCostTab.Options.Add(ed);
                                    break;
                                case 1:
                                    UpgradeTab.Options.Add(ed);
                                    break;
                                //case 3:
                                //    StandardInclusionTab.Options.Add(ed);
                                //    break;
                                case 3:
                                    PreviewTab.Options.Add(ed);
                                    break;
                                default:
                                    break;
                            }
                        }
                        
                        //remove from option tree
                        foreach (EstimateDetails pag2 in OptionTree)
                        {
                            if (pag2.HomeDisplayOptionId == ed.HomeDisplayOptionId)
                            {
                                pag2.EstimateRevisionDetailsId = ed.EstimateRevisionDetailsId;
                                this.OptionTree.Remove(pag2);
                                _optiontreeSource.Remove(pag2);
                                break;
                            }
                        }
                    }

                    RefreshHeaderInfo();
                    //RefreshTab
                    SetVisibliltyOfGreenTickOnTabHeader();
                    IsBusy = false;
                    IsBusyOptionTree = false;
                }
            };
            MRSclient.GetPromotionProductByMasterPromotionRevisionDetailsIDAsync(pag.EstimateRevisionDetailsId.ToString());

        }

        private bool IsDetailsDataValid(EstimateDetails pag)
        {
            bool result = true;
            //try
            //{
            //     = decimal.Parse(pag.Quantity);
            //}
            //catch (Exception ex)
            //{
            //    ok = false;
            //    RadWindow.Alert("Please enter a valid quantity !");
            //}

            //try
            //{
            //    price = decimal.Parse(txtPrice.Text);
            //}
            //catch (Exception ex)
            //{
            //    ok = false;
            //    RadWindow.Alert("Please enter a valid price !");
            //}
            return result;

        }

        public void SynchronizeNewOptionsToEstimate(int revisionid)
        {
            IsBusy = true;
            IsBusyEstimateInfo = true;

            mrsClient.SynchronizeNewOptionToEstimateCompleted += delegate(object o, SynchronizeNewOptionToEstimateCompletedEventArgs es)
            {
                if (es.Error == null)
                {
                    bool result = es.Result;

                    RefreshHeaderInfo();

                    //RefreshTab();

                    SelectedTabIndex = 2; //This will trigger RefreshTab()
                }
                else
                    ExceptionHandler.PopUpErrorMessage(es.Error, "SynchronizeNewOptionToEstimateCompleted");
            };

            mrsClient.SynchronizeNewOptionToEstimateAsync(revisionid);
        }

        public void SynchronizeCustomerDetails()
        {
            IsBusy = true;
            IsBusyEstimateInfo = true;

            mrsClient.SynchroniseCustomerDetailsCompleted += delegate(object o, SynchroniseCustomerDetailsCompletedEventArgs es)
            {
                if (es.Error == null)
                {
                    string result = es.Result;

                    if (string.IsNullOrWhiteSpace(result))
                    {
                        RadWindow.Alert("Customer details have been synchronized successfully.");

                        RefreshHeaderInfo();

                        RefreshTab();
                    }
                    else
                    {
                        RadWindow.Alert(result);
                    }
                }
                else
                    ExceptionHandler.PopUpErrorMessage(es.Error, "SynchronizeCustomerDetails");
                IsBusy = false;
                IsBusyEstimateInfo = false;

            };

            mrsClient.SynchroniseCustomerDetailsAsync(ContractNo);
        }

        public void AddAddtionalNotesTemplate(string templatename)
        {

            //foreach (EstimateDetails ed in NotesTemplate)
            //{
            //    if (ed.TemplateName == templatename && ed.EstimateDetailsId != 0)
            //    {
            //        switch (SelectedTabIndex)
            //        {
            //            case 0:
            //                this.UpgradeTab.Options.Add(ed);
            //                break;
            //            case 1:
            //                if (ed.AreaName.ToUpper().Contains("NON STANDARD REQUEST"))
            //                {
            //                    this.NonStandardTab.Options.Add(ed);
            //                }
            //                break;
            //            case 2:
            //                if (ed.AreaName.ToUpper().Contains("SITE WORKS & CONNECTIONS"))
            //                {
            //                    this.SiteCostTab.Options.Add(ed);
            //                }
            //                break;
            //            case 3:
            //                this.PromotionTab.Options.Add(ed);
            //                break;
            //            default:
            //                break;
            //        }
            //    }
            //}

            IsBusy = true;
            IsBusyEstimateInfo = true;
            IsBusyOptionTree = true;

            RetailSystemClient MRSclient = new RetailSystemClient();
            MRSclient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            MRSclient.AddAdditonalNotesTemplateCompleted += delegate(object o, AddAdditonalNotesTemplateCompletedEventArgs es)
            {
                if (es.Error == null)
                {
                    if (es.Result)
                    {
                        _optiontreeSource.Clear();
                        _upgradeSource.Clear();

                        RefreshTab();
                        RefreshHeaderInfo();
                    }
                    else
                    {
                        RadWindow.Alert("AddAdditonalNotesTemplate failed!\r\nPlease try again or contact the System Administrator.");

                        IsBusy = false;
                        IsBusyEstimateInfo = false;
                        IsBusyOptionTree = false;
                    }
                }
                else
                {
                    ExceptionHandler.PopUpErrorMessage(es.Error, "AddAdditonalNotesTemplateCompleted");

                    IsBusy = false;
                    IsBusyEstimateInfo = false;
                    IsBusyOptionTree = false;
                }
            };

            MRSclient.AddAdditonalNotesTemplateAsync(templatename, EstimateList.SelectedEstimateRevisionId, _userid);
        }

        public void GetProductImage(string productid, int supplierid)
        {
            RetailSystemClient MRSclient = new RetailSystemClient();
            MRSclient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());
            MRSclient.GetProductImagesCompleted += delegate(object o, GetProductImagesCompletedEventArgs es)
            {
                if (es.Error == null)
                {
                        ImageList.Clear();
                        if (es.Result != null)
                        {
                            foreach (ProductImage pi in es.Result)
                            {
                                ImageList.Add(pi);
                            }
                        }
                }
                else
                {
                    ExceptionHandler.PopUpErrorMessage(es.Error, "GetProductImagesCompleted");

                    IsBusy = false;
                    IsBusyEstimateInfo = false;
                    IsBusyOptionTree = false;
                }
            };

            MRSclient.GetProductImagesAsync(productid, supplierid);
                
        }
        
        public void UpdateItemAcceptance(string estimatedetailsid, int accepted, int userid)
        {
            RetailSystemClient MRSclient = new RetailSystemClient();
            MRSclient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            MRSclient.UpdateItemAcceptanceCompleted += delegate(object o, UpdateItemAcceptanceCompletedEventArgs es)
            {
                if (es.Error == null)
                {
                    if ((App.Current as App).SelectedEstimateAllowToAcceptItem)
                    {
                        SetVisibliltyOfGreenTickOnTabHeader();
                    }
                }
                else
                {
                    ExceptionHandler.PopUpErrorMessage(es.Error, "UpdateItemAcceptanceCompleted");

                    IsBusy = false;
                    IsBusyEstimateInfo = false;
                    IsBusyOptionTree = false;
                }
            };

            MRSclient.UpdateItemAcceptanceAsync(estimatedetailsid, accepted, userid);

            //return bilist;

        }

        public void UpdateStudioMAnswerToAllGroup(string studiomanswer, string productid, string groupname)
        {
            //foreach (EstimateDetails item in PreviewTab.Options)
            foreach (EstimateDetails item in _upgradeSource)
            {
                if (item.ProductId == productid && item.GroupName == groupname)
                {
                    item.StudioMAnswer = studiomanswer;
                    item.StudioMIcon = "./images/green_box.png";
                    item.StudioMTooltips = "Studio M Product. Question answered.";
                }
            }
        }

        void mrsClient_GetPriceRegionByStateCompleted(object sender, GetPriceRegionByStateCompletedEventArgs e)
        {
            SalesRegions.Clear();
            if (m_bRegionIncludeAll)
            {
                SQSSalesRegion region = new SQSSalesRegion();
                region.RegionId = 0;
                region.RegionName = "ALL";
                SalesRegions.Add(region);
            }
            foreach (var item in e.Result)
            {
                SalesRegions.Add(item);
            }
            SelectedRegionId = (App.Current as App).LoginPriceRegionId;
        }

        public void loadPriceRegion(bool includeAll = true)
        {
            RetailSystemClient mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            m_bRegionIncludeAll = includeAll;
            mrsClient.GetPriceRegionByStateCompleted += new EventHandler<GetPriceRegionByStateCompletedEventArgs>(mrsClient_GetPriceRegionByStateCompleted);
            mrsClient.GetPriceRegionByStateAsync((App.Current as App).CurrentUserStateID);
        }

        public void loadHomes()
        {
            RetailSystemClient mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            mrsClient.GetAllAvailableHomeByStateCompleted += new EventHandler<GetAllAvailableHomeByStateCompletedEventArgs>(mrsClient_GetAllAvailableHomeByStateCompleted);
            mrsClient.GetAllAvailableHomeByStateAsync(int.Parse((App.Current as App).CurrentUserStateID), "", true);
        }

        void mrsClient_GetAllAvailableHomeByStateCompleted(object sender, GetAllAvailableHomeByStateCompletedEventArgs e)
        {
            SQSHomes.Clear();
            _sqshomeall.Clear();
            foreach (var item in e.Result)
            {
                SQSHomes.Add(item);
                _sqshomeall.Add(item);
            }
        }

        public void loadHomesFullName()
        {
            RetailSystemClient mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            mrsClient.GetHomeFullNameByStateCompleted += new EventHandler<GetHomeFullNameByStateCompletedEventArgs>(mrsClient_GetHomeFullNameByStateCompleted);
            mrsClient.GetHomeFullNameByStateAsync(int.Parse((App.Current as App).CurrentUserStateID), (App.Current as App).CurrentUserId);
        }

        void mrsClient_GetHomeFullNameByStateCompleted(object sender, GetHomeFullNameByStateCompletedEventArgs e)
        {
            SQSHomes.Clear();
            _sqshomeall.Clear();
            foreach (var item in e.Result)
            {
                SQSHomes.Add(item);
                _sqshomeall.Add(item);
            }
        }

        public void loadAreas()
        {
            RetailSystemClient mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            mrsClient.GetAreaNameWithAllCompleted += new EventHandler<GetAreaNameWithAllCompletedEventArgs>(mrsClient_GetAreaNameWithAllCompleted);
            mrsClient.GetAreaNameWithAllAsync();
        }

        void mrsClient_GetAreaNameWithAllCompleted(object sender, GetAreaNameWithAllCompletedEventArgs e)
        {
            SQSAreas.Clear();
            foreach (var item in e.Result)
            {
                SQSAreas.Add(item);
            }
        }

        public void loadGroups()
        {
            RetailSystemClient mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            mrsClient.GetGroupNameWithAllCompleted += new EventHandler<GetGroupNameWithAllCompletedEventArgs>(mrsClient_GetGroupNameWithAllCompleted);
            mrsClient.GetGroupNameWithAllAsync();
        }

        void mrsClient_GetGroupNameWithAllCompleted(object sender, GetGroupNameWithAllCompletedEventArgs e)
        {
            SQSGroups.Clear();
            foreach (var item in e.Result)
            {
                SQSGroups.Add(item);
            }
        }

        public void FilterHomes(bool displayonly)
        {
            SQSHomes.Clear();
            if (displayonly)
            {
                foreach (SQSHome item in _sqshomeall)
                {
                    if (item.Display)
                    {
                        SQSHomes.Add(item);
                    }
                }
            }
            else
            {
                
                SQSHomes = _sqshomeall;
            }
        }

        public void SearchOtherHomeProducts(string regionid, string homeid)
        {
            IsBusyOptionTree3 = true;
            RetailSystemClient mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            mrsClient.GetOptionTreeFromMasterHomeCompleted += new EventHandler<GetOptionTreeFromMasterHomeCompletedEventArgs>(mrsClient_GetOptionTreeFromMasterHomeCompleted);
            mrsClient.GetOptionTreeFromMasterHomeAsync(regionid, homeid);
        }

        void mrsClient_GetOptionTreeFromMasterHomeCompleted(object sender, GetOptionTreeFromMasterHomeCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                ObservableCollection<EstimateDetails> eds = new ObservableCollection<EstimateDetails>();

                foreach (OptionTreeProducts p in e.Result)
                {
                    EstimateDetails ed = new EstimateDetails();
                    ed.ProductDescription = p.D;
                    ed.ProductName = p.N;
                    ed.Price = p.P;
                    ed.AreaName = p.A;
                    ed.GroupName = p.G;
                    ed.AreaId = p.AI;
                    ed.GroupId = p.GI;
                    ed.StandardOption = p.S;
                    ed.Quantity = p.Q;
                    ed.HomeDisplayOptionId = p.I;
                    ed.SiteWorkItem = p.W;
                    ed.StudioMSortOrder = p.O;
                    ed.Uom = p.U;                    
                    
                    if (p.M == 0) // Not Studio M
                    {
                        ed.StudioMProduct = false;
                    }
                    else if (p.M == 1) // Studio M Manadatory
                    {
                        ed.StudioMProduct = true;
                        ed.StudioMIcon = "./images/color_swatch.png";
                        ed.StudioMTooltips = "Studio M Product. Question not answered yet.";
                    }
                    else if (p.M == 2) // Studio M No Question
                    {
                        ed.StudioMProduct = true;
                        ed.StudioMIcon = "./images/green_box.png";
                        ed.StudioMTooltips = "There are no studio M questions.";
                    }
                    else if (p.M == 3) // Studio M Non-Mandatory
                    {
                        ed.StudioMProduct = true;
                        ed.StudioMIcon = "./images/color_swatch_gray.png";
                        ed.StudioMTooltips = "Studio M Product. Answers are not mandatory.";
                    }

                    if (p.S)
                    {
                        ed.SOSI = "./images/upgrade.png";
                        ed.SOSIToolTips = "Upgrade Option.";
                    }

                    eds.Add(ed);
                }

                this.OtherHomeOptionTree = eds;
            }
 
            IsBusyOptionTree3 = false;
        }

        public void SearchAllProducts(string regionid, string searchText)
        {
            IsBusyOptionTree4 = true;
            RetailSystemClient mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            mrsClient.GetOptionTreeFromAllProductsCompleted += new EventHandler<GetOptionTreeFromAllProductsCompletedEventArgs>(mrsClient_GetOptionTreeFromAllProductsCompleted);
            mrsClient.GetOptionTreeFromAllProductsAsync(regionid, searchText);
        }

        void mrsClient_GetOptionTreeFromAllProductsCompleted(object sender, GetOptionTreeFromAllProductsCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                ObservableCollection<EstimateDetails> eds = new ObservableCollection<EstimateDetails>();

                foreach (OptionTreeProducts p in e.Result)
                {
                    EstimateDetails ed = new EstimateDetails();
                    ed.ProductId = p.PI;
                    ed.ProductName = p.N;
                    ed.ProductDescription = p.D;
                    ed.Price = p.P;
                    ed.AreaName = p.A;
                    ed.GroupName = p.G;
                    ed.AreaId = p.AI;
                    ed.GroupId = p.GI;
                    ed.StandardOption = p.S;
                    ed.Quantity = p.Q;
                    ed.HomeDisplayOptionId = p.I;
                    ed.SiteWorkItem = p.W;
                    ed.StudioMSortOrder = p.O;
                    ed.Uom = p.U;

                    if (p.M == 0) // Not Studio M
                    {
                        ed.StudioMProduct = false;
                    }
                    else if (p.M == 1) // Studio M Manadatory
                    {
                        ed.StudioMProduct = true;
                        ed.StudioMIcon = "./images/color_swatch.png";
                        ed.StudioMTooltips = "Studio M Product. Question not answered yet.";
                    }
                    else if (p.M == 2) // Studio M No Question
                    {
                        ed.StudioMProduct = true;
                        ed.StudioMIcon = "./images/green_box.png";
                        ed.StudioMTooltips = "There are no studio M questions.";
                    }
                    else if (p.M == 3) // Studio M Non-Mandatory
                    {
                        ed.StudioMProduct = true;
                        ed.StudioMIcon = "./images/color_swatch_gray.png";
                        ed.StudioMTooltips = "Studio M Product. Answers are not mandatory.";
                    }

                    if (p.S)
                    {
                        ed.SOSI = "./images/upgrade.png";
                        ed.SOSIToolTips = "Upgrade Option.";
                    }

                    eds.Add(ed);
                }

                this.SearchAllProductsOptionTree = eds;
            }

            IsBusyOptionTree4 = false;
        }

        public void SearchAllProductsExtended(string regionid, int homeid, string productname, string productdesc, int areaid, int groupid)
        {
            IsBusyOptionTree4 = true;
            RetailSystemClient mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            mrsClient.GetOptionTreeFromAllProductsExtendedCompleted += new EventHandler<GetOptionTreeFromAllProductsExtendedCompletedEventArgs>(mrsClient_GetOptionTreeFromAllProductsExtendedCompleted);
            mrsClient.GetOptionTreeFromAllProductsExtendedAsync(int.Parse(((App)(App.Current)).CurrentUserStateID), regionid, homeid, productname, productdesc, areaid, groupid);
        }

        void mrsClient_GetOptionTreeFromAllProductsExtendedCompleted(object sender, GetOptionTreeFromAllProductsExtendedCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                ObservableCollection<EstimateDetails> eds = new ObservableCollection<EstimateDetails>();

                foreach (OptionTreeProducts p in e.Result)
                {
                    EstimateDetails ed = new EstimateDetails();
                    ed.ProductId = p.PI;
                    ed.ProductName = p.N.Replace("<br>", Environment.NewLine);
                    ed.ProductDescription = p.D.Replace("<br>", Environment.NewLine);
                    ed.Quantity = p.Q;
                    ed.Price = p.P;
                    ed.TotalPrice = p.Q * p.P;
                    ed.AreaName = p.A;
                    ed.GroupName = p.G;
                    ed.AreaId = p.AI;
                    ed.GroupId = p.GI;
                    ed.StandardOption = p.S;
                    ed.HomeDisplayOptionId = p.I;
                    ed.SiteWorkItem = p.W;
                    ed.StudioMSortOrder = p.O;
                    ed.Uom = p.U;

                    if (p.M == 0) // Not Studio M
                    {
                        ed.StudioMProduct = false;
                    }
                    else if (p.M == 1) // Studio M Manadatory
                    {
                        ed.StudioMProduct = true;
                        ed.StudioMIcon = "./images/color_swatch.png";
                        ed.StudioMTooltips = "Studio M Product. Question not answered yet.";
                    }
                    else if (p.M == 2) // Studio M No Question
                    {
                        ed.StudioMProduct = true;
                        ed.StudioMIcon = "./images/green_box.png";
                        ed.StudioMTooltips = "There are no studio M questions.";
                    }
                    else if (p.M == 3) // Studio M Non-Mandatory
                    {
                        ed.StudioMProduct = true;
                        ed.StudioMIcon = "./images/color_swatch_gray.png";
                        ed.StudioMTooltips = "Studio M Product. Answers are not mandatory.";
                    }

                    if (p.S)
                    {
                        ed.SOSI = "./images/upgrade.png";
                        ed.SOSIToolTips = "Upgrade Option.";
                    }

                    eds.Add(ed);
                }

                this.SearchAllProductsOptionTree = eds;
            }

            IsBusyOptionTree4 = false;
        }

        private string GetLotAddress(string lotNumber, string streetNumber, string streetAddress, string suburb, string state, string postCode)
        {
            string lotAddress = "";

            if (!string.IsNullOrWhiteSpace(lotNumber))
                lotAddress = "Lot " + lotNumber;
            if (!string.IsNullOrWhiteSpace(streetNumber))
            {
                if (!string.IsNullOrWhiteSpace(lotAddress))
                    lotAddress += ", ";
                lotAddress += streetNumber + " ";
            }
            if (!string.IsNullOrWhiteSpace(streetAddress))
            {
                if (!string.IsNullOrWhiteSpace(streetNumber))
                    lotAddress += " ";
                else if (!string.IsNullOrWhiteSpace(lotAddress))
                    lotAddress += ", ";
                lotAddress += streetAddress;
            }
            if (!string.IsNullOrWhiteSpace(suburb))
            {
                if (!string.IsNullOrWhiteSpace(lotAddress))
                    lotAddress += ", ";
                lotAddress += suburb;
            }
            if (!string.IsNullOrWhiteSpace(state))
            {
                if (!string.IsNullOrWhiteSpace(lotAddress))
                    lotAddress += ", ";
                lotAddress += state;
            }
            if (!string.IsNullOrWhiteSpace(postCode))
            {
                if (!string.IsNullOrWhiteSpace(lotAddress))
                    lotAddress += ", ";
                lotAddress += postCode;
            }
            return lotAddress.ToUpper();
        }

        #endregion

        #region Commands
        public ICommand AddCommand
        {
            get
            {
                if (addCommand == null)
                {
                    addCommand = new DelegateCommand<EstimateDetails>(this.SaveOptionFromTree);
                }

                return addCommand;
            }
        }
        //public ICommand RemoveCommand
        //{
        //    get
        //    {
        //        if (removeCommand == null)
        //        {
        //            removeCommand = new DelegateCommand<EstimateDetails>(this.RemoveOptionFromEstimate);
        //        }

        //        return removeCommand;
        //    }
        //}
        public ICommand CopyCommand
        {
            get
            {
                if (copyCommand == null)
                {
                    copyCommand = new DelegateCommand<EstimateDetails>(this.CopyItemFromOptionTreeToEstimate);
                }

                return copyCommand;
            }
        }

        public ICommand CopyFromHomeCommand
        {
            get
            {
                if (copyfromhomeCommand == null)
                {
                    copyfromhomeCommand = new DelegateCommand<EstimateDetails>(this.CopyItemFromMasterHomeToEstimate);
                }

                return copyfromhomeCommand;
            }
        }
        public ICommand CopyFromAllProductsCommand
        {
            get
            {
                if (copyfromAllProductsCommand == null)
                {
                    copyfromAllProductsCommand = new DelegateCommand<EstimateDetails>(this.CopyItemFromAllProductsToEstimate);
                }

                return copyfromAllProductsCommand;
            }
        }
        public ICommand UpdateDetailsCommand
        {
            get
            {
                if (updatedetailsCommand == null)
                {
                    //updatedetailsCommand = new DelegateCommand<EstimateDetails>(this.UpdateEstimateDetails);
                }

                return updatedetailsCommand;
            }
        }
        public ICommand AddNotesCommand
        {
            get
            {
                if (notesCommand == null)
                {
                    notesCommand = new DelegateCommand<string>(this.AddAddtionalNotesTemplate);
                }

                return notesCommand;
            }
        }
        public ICommand SyncCommand
        {
            get
            {
                if (syncCommand == null)
                {
                    syncCommand = new Command.DelegateCommand(this.SynchronizeCustomerDetails);
                }

                return syncCommand;
            }
        }

        #endregion

        #region public properties
        public Visibility ShowHeaderFields
        {
            get
            {
                return _show;
            }
            set
            {
                _show = value;
                OnPropertyChanged("ShowHeaderFields");
            }
        }

        public Visibility ShowMargin
        {
            get
            {
                return _showmargin;
            }
            set
            {
                _showmargin = value;
                OnPropertyChanged("ShowMargin");
            }
        }
        public bool SetAsPCState
        {
            get
            {
                return _setaspcstate;
            }
        }
        public Visibility ShowSetDocumentType
        {
            get
            {
                return _showsetdocumenttype;
            }
            set
            {
                _showsetdocumenttype = value;
                OnPropertyChanged("ShowSetDocumentType");
            }
        }
        public bool SetAsContractState
        {
            get
            {
                return _setascontractstate;
            }
        }
        public Visibility ShowSetAsContract
        {
            get
            {
                return _showsetascontract;
            }
            set
            {
                _showsetascontract = value;
                OnPropertyChanged("ShowSetAsContract");
            }
        }
        Visibility _showPrintMargin = Visibility.Visible;
        public Visibility ShowPrintMargin
        {
            get
            {
                return _showPrintMargin;
            }
            set
            {
                _showPrintMargin = value;
                OnPropertyChanged("ShowPrintMargin");
            }
        }
        Visibility _showEnterMargin = Visibility.Visible;
        public Visibility ShowEnterMargin
        {
            get
            {
                return _showEnterMargin;
            }
            set
            {
                _showEnterMargin = value;
                OnPropertyChanged("ShowEnterMargin");
            }
        }
        public ObservableCollection<EstimateComments> Comments
        {
            get
            {
                return _comments;
            }
            set
            {
                _comments = value;
                OnPropertyChanged("Comments");
            }
        }

        public ObservableCollection<EstimateDetails> OptionTree
        {
            get
            {
                return _optiontree;
            }
            set
            {
                _optiontree = value;
                OnPropertyChanged("OptionTree");
            }
        }
        public ObservableCollection<EstimateDetails> OtherHomeOptionTree
        {
            get
            {
                return _otherhomeoptiontree;
            }
            set
            {
                _otherhomeoptiontree = value;
                OnPropertyChanged("OtherHomeOptionTree");
            }
        }
        public ObservableCollection<EstimateDetails> SearchAllProductsOptionTree
        {
            get
            {
                return _searchallproductsoptiontree;
            }
            set
            {
                _searchallproductsoptiontree = value;
                OnPropertyChanged("SearchAllProductsOptionTree");
            }
        }
        public ObservableCollection<EstimateDetails> NotesTemplateTree
        {
            get
            {
                return _notetemplate;
            }
            set
            {
                _notetemplate = value;
                OnPropertyChanged("NotesTemplateTree");
            }
        }

        public ObservableCollection<EstimateDetails> SelectedOptions
        {
            get
            {
                return _option;
            }
            set
            {
                _option = value;
                OnPropertyChanged("Option");
            }
        }

        public ObservableCollection<SQSSalesRegion> SalesRegions
        {
            get
            {
                return _salesregion;
            }
            set
            {
                _salesregion = value;
                OnPropertyChanged("SalesRegions");
            }
        }

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

                    RefreshTab();
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
        public bool IsBusyOptionTree
        {
            get
            {
                return _isbusyoptiontree;
            }

            set
            {
                if (_isbusyoptiontree != value)
                {
                    _isbusyoptiontree = value;
                    OnPropertyChanged("IsBusyOptionTree");
                }
            }
        }
        public bool IsBusyOptionTree3
        {
            get
            {
                return _isbusyoptiontree3;
            }

            set
            {
                if (_isbusyoptiontree3 != value)
                {
                    _isbusyoptiontree3 = value;
                    OnPropertyChanged("IsBusyOptionTree3");
                }
            }
        }
        public bool IsBusyOptionTree4
        {
            get
            {
                return _isbusyoptiontree4;
            }

            set
            {
                if (_isbusyoptiontree4 != value)
                {
                    _isbusyoptiontree4 = value;
                    OnPropertyChanged("IsBusyOptionTree4");
                }
            }
        }
        public bool IsBusyEstimateInfo
        {
            get
            {
                return _isBusyEstimateInfo;
            }

            set
            {
                if (_isBusyEstimateInfo != value)
                {
                    _isBusyEstimateInfo = value;
                    OnPropertyChanged("IsBusyEstimateInfo");
                }
            }
        }

        private string _lotAddress;
        public string LotAddress
        {
            get
            {
                return _lotAddress;
            }

            set
            {
                if (_lotAddress != value)
                {
                    _lotAddress = value;
                    OnPropertyChanged("LotAddress");
                }
            }
        }

        public string HomePrice
        {
            get
            {
                return _homePrice;
            }

            set
            {
                if (_homePrice != value)
                {
                    _homePrice = value;
                    OnPropertyChanged("HomePrice");
                }
            }
        }

        public string UpgradeValue
        {
            get
            {
                return _upgradeValue;
            }

            set
            {
                if (_upgradeValue != value)
                {
                    _upgradeValue = value;
                    OnPropertyChanged("UpgradeValue");
                }
            }
        }

        public string PromotionValue
        {
            get
            {
                return _promotionValue;
            }

            set
            {
                if (_promotionValue != value)
                {
                    _promotionValue = value;
                    OnPropertyChanged("PromotionValue");
                }
            }
        }

        public string SiteCost
        {
            get
            {
                return _siteCost;
            }

            set
            {
                if (_siteCost != value)
                {
                    _siteCost = value;
                    OnPropertyChanged("SiteCost");
                }
            }
        }
        public string SubTotalPrice
        {
            get
            {
                return _subTotalPrice;
            }

            set
            {
                if (_subTotalPrice != value)
                {
                    _subTotalPrice = value;
                    OnPropertyChanged("SubTotalPrice");
                }
            }
        }

        public string TotalPrice
        {
            get
            {
                return _totalPrice;
            }

            set
            {
                if (_totalPrice != value)
                {
                    _totalPrice = value;
                    OnPropertyChanged("TotalPrice");
                }
            }
        }

        public string TotalCostBTP
        {
            get
            {
                return _totalcostbtp;
            }

            set
            {
                if (_totalcostbtp != value)
                {
                    _totalcostbtp = value;
                    OnPropertyChanged("TotalCostBTP");
                }
            }
        }
        public string TotalCostDBC
        {
            get
            {
                return _totalcostdbc;
            }

            set
            {
                if (_totalcostdbc != value)
                {
                    _totalcostdbc = value;
                    OnPropertyChanged("TotalCostDBC");
                }
            }
        }

        public string Margin
        {
            get
            {
                return _margin;
            }

            set
            {
                if (_margin != value)
                {
                    _margin = value;
                    OnPropertyChanged("Margin");
                }
            }
        }

        public string TotalMargin
        {
            get
            {
                return _totalmargin;
            }

            set
            {
                if (_totalmargin != value)
                {
                    _totalmargin = value;
                    OnPropertyChanged("TotalMargin");
                }
            }
        }

        public string MarginString
        {
            get
            {
                return _marginstring;
            }

            set
            {
                if (_marginstring != value)
                {
                    _marginstring = value;
                    OnPropertyChanged("MarginString");
                }
            }
        }

        public string TotalPriceExc
        {
            get
            {
                return _totlpriceexc;
            }

            set
            {
                if (_totlpriceexc != value)
                {
                    _totlpriceexc = value;
                    OnPropertyChanged("TotalPriceExc");
                }
            }
        }
        public string TargetMargin
        {
            get
            {
                return _targetmargin;
            }

            set
            {
                if (_targetmargin != value)
                {
                    _targetmargin = value;
                    OnPropertyChanged("TargetMargin");
                }
            }
        }

        public string CustomerNo
        {
            get
            {
                return _customerno;
            }

            set
            {
                if (_customerno != value)
                {
                    _customerno = value;
                    OnPropertyChanged("CustomerNo");
                }
            }
        }

        public string CustomerName
        {
            get
            {
                return _customername;
            }

            set
            {
                if (_customername != value)
                {
                    _customername = value;
                    OnPropertyChanged("CustomerName");
                }
            }
        }

        public string ContractNo
        {
            get
            {
                return _contractno;
            }

            set
            {
                if (_contractno != value)
                {
                    _contractno = value;
                    OnPropertyChanged("ContractNo");
                }
            }
        }

        public string EstimateNo
        {
            get
            {
                return _estimateno;
            }

            set
            {
                if (_estimateno != value)
                {
                    _estimateno = value;
                    OnPropertyChanged("EstimateNo");
                }
            }
        }

        public string HomeName
        {
            get
            {
                return _homename;
            }

            set
            {
                if (_homename != value)
                {
                    _homename = value;
                    OnPropertyChanged("HomeName");
                }
            }
        }

        public string Region
        {
            get
            {
                return _region;
            }

            set
            {
                if (_region != value)
                {
                    _region = value;
                    OnPropertyChanged("Region");
                }
            }
        }
        public string DepositDate
        {
            get
            {
                return _depositdate;
            }

            set
            {
                if (_depositdate != value)
                {
                    _depositdate = value;
                    OnPropertyChanged("DepositDate");
                }
            }
        }
        public string HomeRange
        {
            get
            {
                return _homerange;
            }

            set
            {
                if (_homerange != value)
                {
                    _homerange = value;
                    OnPropertyChanged("HomeRange");
                }
            }
        }

        public string EffectiveDate
        {
            get
            {
                return _effectivedate;
            }

            set
            {
                if (_effectivedate != value)
                {
                    _effectivedate = value;
                    OnPropertyChanged("EffectiveDate");
                }
            }
        }
        public string PriceExpiryDate
        {
            get
            {
                return _priceexpirydate;
            }

            set
            {
                if (_priceexpirydate != value)
                {
                    _priceexpirydate = value;
                    OnPropertyChanged("PriceExpiryDate");
                }
            }
        }
        public int StdPriceHoldDays
        {
            get
            {
                return _stdpriceholddays;
            }

            set
            {
                if (_stdpriceholddays != value)
                {
                    _stdpriceholddays = value;
                    OnPropertyChanged("StdPriceHoldDays");
                }
            }
        }
        public int TitledLandDays
        {
            get
            {
                return _titledlanddays;
            }

            set
            {
                if (_titledlanddays != value)
                {
                    _titledlanddays = value;
                    OnPropertyChanged("TitledLandDays");
                }
            }
        }
        public int RevisedPriceHoldDays
        {
            get
            {
                return _revisedpriceholddays;
            }

            set
            {
                if (_revisedpriceholddays != value)
                {
                    _revisedpriceholddays = value;
                    OnPropertyChanged("RevisedPriceHoldDays");
                }
            }
        }
        public string RevisedPriceExpiryDate
        {
            get
            {
                return _revisedpriceexpirydate;
            }

            set
            {
                if (_revisedpriceexpirydate != value)
                {
                    _revisedpriceexpirydate = value;
                    OnPropertyChanged("RevisedPriceExpiryDate");
                }
            }
        }
        public int BasePriceExtensionDays
        {
            get
            {
                return _basepriceextensiondays;
            }

            set
            {
                if (_basepriceextensiondays != value)
                {
                    _basepriceextensiondays = value;
                    OnPropertyChanged("BasePriceExtensionDays");
                }
            }
        }
        public decimal BasePriceExtensionCharge
        {
            get
            {
                return _basepriceextensioncharge;
            }

            set
            {
                if (_basepriceextensioncharge != value)
                {
                    _basepriceextensioncharge = value;
                    OnPropertyChanged("BasePriceExtensionCharge");
                }
            }
        }
        public decimal RequiredPBE5Percent
        {
            get
            {
                return _requiredbpe5percent;
            }

            set
            {
                if (_requiredbpe5percent != value)
                {
                    _requiredbpe5percent = value;
                    OnPropertyChanged("RequiredPBE5Percent");
                }
            }
        }
        public decimal RequiredPBERollback
        {
            get
            {
                return _requiredbperollback;
            }

            set
            {
                if (_requiredbperollback != value)
                {
                    _requiredbperollback = value;
                    OnPropertyChanged("RequiredPBERollback");
                }
            }
        }
        public decimal RequiredPBETodaysPrice
        {
            get
            {
                return _requiredbpetodaysprice;
            }

            set
            {
                if (_requiredbpetodaysprice != value)
                {
                    _requiredbpetodaysprice = value;
                    OnPropertyChanged("RequiredPBETodaysPrice");
                }
            }
        }
        public string RevisionNo
        {
            get
            {
                return _revisionno;
            }

            set
            {
                if (_revisionno != value)
                {
                    _revisionno = value;
                    OnPropertyChanged("RevisionNo");
                }
            }
        }

        public string JobFlowType
        {
            get
            {
                return _jobFlowType;
            }

            set
            {
                if (_jobFlowType != value)
                {
                    _jobFlowType = value;
                    OnPropertyChanged("JobFlowType");
                }
            }
        }

        public string ContractType
        {
            get
            {
                return _contracttype;
            }

            set
            {
                if (_contracttype != value)
                {
                    _contracttype = value;
                    OnPropertyChanged("ContractType");
                }
            }
        }

        public string SalesConsultant
        {
            get
            {
                return _salesconsultant;
            }

            set
            {
                if (_salesconsultant != value)
                {
                    _salesconsultant = value;
                    OnPropertyChanged("SalesConsultant");
                }
            }
        }

        public string TemplateNameSearchText
        {
            get
            {
                return _templatenamesearchtext;
            }

            set
            {
                if (_templatenamesearchtext != value)
                {
                    _templatenamesearchtext = value;
                    OnPropertyChanged("TemplateNameSearchText");
                }
            }
        }

        public int SelectedRegionId
        {
            get
            {
                return _selectedregionid;
            }

            set
            {
                if (_selectedregionid != value)
                {
                    _selectedregionid = value;
                    OnPropertyChanged("SelectedRegionId");
                }
            }
        }

        public int SelectedHomeId
        {
            get
            {
                return _selectedhomeid;
            }

            set
            {
                if (_selectedhomeid != value)
                {
                    _selectedhomeid = value;
                    OnPropertyChanged("SelectedHomeId");
                }
            }
        }

        string _productname = string.Empty;
        public string ProductName
        {
            get
            {
                return _productname;
            }

            set
            {
                if (_productname != value)
                {
                    _productname = value;
                    OnPropertyChanged("ProductName");
                }
            }
        }

        string _productdescription = string.Empty;
        public string ProductDescription
        {
            get
            {
                return _productdescription;
            }

            set
            {
                if (_productdescription != value)
                {
                    _productdescription = value;
                    OnPropertyChanged("ProductDescription");
                }
            }
        }

        private int _selectedareaid = 0;
        public int SelectedAreaId
        {
            get
            {
                return _selectedareaid;
            }

            set
            {
                if (_selectedareaid != value)
                {
                    _selectedareaid = value;
                    OnPropertyChanged("SelectedAreaId");
                }
            }
        }

        private int _selectedgroupid = 0;
        public int SelectedGroupId
        {
            get
            {
                return _selectedgroupid;
            }

            set
            {
                if (_selectedgroupid != value)
                {
                    _selectedgroupid = value;
                    OnPropertyChanged("SelectedGroupId");
                }
            }
        }

        public string SalesAcceptor
        {
            get
            {
                return _salesacceptor;
            }

            set
            {
                if (_salesacceptor != value)
                {
                    _salesacceptor = value;
                    OnPropertyChanged("SalesAcceptor");
                }
            }
        }
        public string DraftPerson
        {
            get
            {
                return _draftperson;
            }

            set
            {
                if (_draftperson != value)
                {
                    _draftperson = value;
                    OnPropertyChanged("DraftPerson");
                }
            }
        }
        public string SalesEstimator
        {
            get
            {
                return _salesestimator;
            }

            set
            {
                if (_salesestimator != value)
                {
                    _salesestimator = value;
                    OnPropertyChanged("SalesEstimator");
                }
            }
        }
        public string CSC
        {
            get
            {
                return _csc;
            }

            set
            {
                if (_csc != value)
                {
                    _csc = value;
                    OnPropertyChanged("CSC");
                }
            }
        }

        public string SelectedPromotion
        {
            get
            {
                return _selectedpromotion;
            }

            set
            {
                if (_selectedpromotion != value)
                {
                    _selectedpromotion = value;
                    OnPropertyChanged("SelectedPromotion");
                }
            }
        }
        public string CustomerDocumentTypeValue
        {
            get
            {
                return _customerdocumenttypevalue;
            }
        }
        public string CustomerDocumentType
        {
            get
            {
                return _customerdocumenttype;
            }

            set
            {
                if (_customerdocumenttype != value)
                {
                    _customerdocumenttype = value;
                    OnPropertyChanged("CustomerDocumentType");
                }
            }
        }

        public ObservableCollection<SharepointDocumentType> SharepointDocType
        {
            get
            {
                return _doctypelist;
            }

            set
            {
                if (_doctypelist != value)
                {
                    _doctypelist = value;
                    OnPropertyChanged("SharepointDocType");
                }
            }
        }
        public string SharePointFolderUrl
        {
            get
            {
                return _sharePointFolderUrl;
            }

            set
            {
                if (_sharePointFolderUrl != value)
                {
                    _sharePointFolderUrl = value;
                    OnPropertyChanged("SharePointFolderUrl");
                }
            }
        }

        public bool EditVisible
        {
            get
            {
                return _editVisible;
            }

            set
            {
                if (_editVisible != value)
                {
                    _editVisible = value;
                    OnPropertyChanged("EditVisible");
                }
            }
        }

        public bool RejectVisible
        {
            get
            {
                return _rejectVisible;
            }

            set
            {
                if (_rejectVisible != value)
                {
                    _rejectVisible = value;
                    OnPropertyChanged("RejectVisible");
                }
            }
        }

        public int SelectedCategoryId
        {
            get
            {
                return _selectedcategoryId;
            }

            set
            {
                if (_selectedcategoryId != value)
                {
                    _selectedcategoryId = value;
                    OnPropertyChanged("SelectedCategoryId");
                }
            }
        }

        public List<ProductImage> ImageList
        {
            get
            {
                return _imagelist;
            }

            set
            {
                if (_imagelist != value)
                {
                    _imagelist = value;
                    OnPropertyChanged("ImageList");
                }
            }
        }

        public string EstimateHeaderBrief
        {
            get
            {
                return _estimateheaderbrief;
            }

            set
            {
                if (_estimateheaderbrief != value)
                {
                    _estimateheaderbrief = value;
                    OnPropertyChanged("EstimateHeaderBrief");
                }
            }
        }

        public ObservableCollection<NonStandardGroup> EstimateNonStandardGroup
        {
            get
            {
                return _estimatenonstandardgroup;
            }

            set
            {
                if (_estimatenonstandardgroup != value)
                {
                    _estimatenonstandardgroup = value;
                    OnPropertyChanged("EstimateNonStandardGroup");
                }
            }
        }

        public ObservableCollection<SharepointDoc> SharepointDocuments
        {
            get
            {
                return _sharepointdocuments;
            }

            set
            {
                if (_sharepointdocuments != value)
                {
                    _sharepointdocuments = value;
                    OnPropertyChanged("SharepointDocuments");
                }
            }
        }

        public ObservableCollection<SQSHome> SQSHomes
        {
            get
            {
                return _sqshome;
            }

            set
            {
                if (_sqshome != value)
                {
                    _sqshome = value;
                    OnPropertyChanged("SQSHomes");
                }
            }
        }

        public ObservableCollection<SQSArea> SQSAreas
        {
            get
            {
                return _sqsarea;
            }

            set
            {
                if (_sqsarea != value)
                {
                    _sqsarea = value;
                    OnPropertyChanged("SQSAreas");
                }
            }
        }

        public ObservableCollection<SQSGroup> SQSGroups
        {
            get
            {
                return _sqsgroup;
            }

            set
            {
                if (_sqsgroup != value)
                {
                    _sqsgroup = value;
                    OnPropertyChanged("SQSGroups");
                }
            }
        }

        bool _isColMarginPercentAvailable = false;
        public bool IsColMarginPercentAvailable
        {
            get { return _isColMarginPercentAvailable; }
            set
            {
                _isColMarginPercentAvailable = value;
                OnPropertyChanged("IsColMarginPercentAvailable");
            }
        }

        public bool IsRoleAllowtoAccessThisRevision        
        {
            get { return _isRoleAllowtoAccessThisRevision; }
            set
            {
                _isRoleAllowtoAccessThisRevision = value;
                OnPropertyChanged("IsRoleAllowtoAccessThisRevision");
            }
        }
        #endregion
    
    }

    #region tabclass
    public class OptionListTab : ViewModelBase
    {
        public OptionListTab(string name)
        {
            this.Name = name;
            this.Options = new ObservableCollection<EstimateDetails>();
        }
        public string Name { get; set; }
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
                    _count= value;
                    OnPropertyChanged("Count");
                }
            }
        }
        public ObservableCollection<EstimateDetails> Options { get; set; }

    }


    #endregion

}
