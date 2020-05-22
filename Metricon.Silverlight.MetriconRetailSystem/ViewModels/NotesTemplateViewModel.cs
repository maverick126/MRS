using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using Metricon.Silverlight.MetriconRetailSystem.Command;
using Metricon.Silverlight.MetriconRetailSystem.MRSService;
using Telerik.Windows.Controls;

namespace Metricon.Silverlight.MetriconRetailSystem.ViewModels
{
    public class NotesTemplateViewModel : ViewModelBase
    {
        #region variables declaration

        private string _templatenamesearchtext;
        private int _selectedregionid;
        private int _originalselectedregionid;
        private int userid;
        private int _selectedstatus;
        private string _newtemplatename = "";

        private ObservableCollection<EstimateDetails> _notetemplate = new ObservableCollection<EstimateDetails>();
        private ObservableCollection<EstimateDetails> _availablenotetemplateitem = new ObservableCollection<EstimateDetails>();
        private ObservableCollection<NoteTemplate> _managernotetemplate = new ObservableCollection<NoteTemplate>();
        private ObservableCollection<SQSSalesRegion> _salesregion = new ObservableCollection<SQSSalesRegion>();
        private ObservableCollection<TemplateStatus> _templatestatus = new ObservableCollection<TemplateStatus>();

        private Metricon.Silverlight.MetriconRetailSystem.Command.DelegateCommand searchnotestempalteCommand;
        private Metricon.Silverlight.MetriconRetailSystem.Command.DelegateCommand _clearsearchtextcommand;

        private DelegateCommand<EstimateDetails> removenotetemplateitemCommand;
        private Metricon.Silverlight.MetriconRetailSystem.Command.DelegateCommand addnotetemplateCommand;
        private DelegateCommand<NoteTemplate> removenotetemplateCommand;
        private DelegateCommand<EstimateDetails> updatenotetemplateitemCommand;
        private RetailSystemClient mrsClient,mrsClient2, mrsClient3;
        #endregion
        #region functions
        public NotesTemplateViewModel()
        {
            userid = (App.Current as App).CurrentUserId;
            
            if (!IsDesignTime)
            {
                loadData();
                loadStatusData();
            }
        }

        public void loadData()
        {
            mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            mrsClient.GetSalesRegionByStateCompleted += new EventHandler<GetSalesRegionByStateCompletedEventArgs>(mrsClient_GetSalesRegionByStateCompleted);
            mrsClient.GetSalesRegionByStateAsync((App.Current as App).CurrentUserStateID);
            mrsClient = null;

        }
        public void loadStatusData()
        {
            TemplateStatus a;
            TemplateStatusArray.Clear();
            a = new TemplateStatus();
            a.StatusID = 2;
            a.StatusName = "All";
            TemplateStatusArray.Add(a);

            a = new TemplateStatus();
            a.StatusID = 1;
            a.StatusName = "Active";
            TemplateStatusArray.Add(a);

            a = new TemplateStatus();
            a.StatusID = 0;
            a.StatusName = "Inactive";
            TemplateStatusArray.Add(a);

            SelectedStatus = 1;
        }
        public void SearchNotesTemplate()
        {
            mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());
            
            mrsClient.GetAdditionalNotesTemplateAndProductsByRegionCompleted += new EventHandler<GetAdditionalNotesTemplateAndProductsByRegionCompletedEventArgs>(mrsClient_AdditionalNotesTemplateAndProductsByRegionCompleted);
            mrsClient.GetAdditionalNotesTemplateAndProductsByRegionAsync(TemplateNameSearchText, SelectedRegionId.ToString(), userid, SelectedStatus, (App.Current as App).CurrentUserRoleId);
            mrsClient = null;
        }

        public void RemoveNoteTemplateItemFromTemplate(EstimateDetails pag)
        {
            mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            mrsClient.RemoveItemFromNotesTemplateCompleted += delegate(object o, RemoveItemFromNotesTemplateCompletedEventArgs es)
            {
                bool result = es.Result;
            };
            mrsClient.RemoveItemFromNotesTemplateAsync(pag.TemplateID, pag.ProductAreaGroupID.ToString(), userid);
            mrsClient = null;
            RefreshTemplateByID(pag);
        }

        public void RefreshTemplateByID(EstimateDetails pag)
        {
            foreach (var temp in ManagerNotesTemplate)
            {
                if (temp.TemplateID.ToString() == pag.TemplateID)
                {
                    temp.NoteTemplateItem.Remove(pag);
                    break;
                }
            }
        }

        public void RemoveNoteTemplate(NoteTemplate template)
        {
            mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            mrsClient.RemoveNotesTemplateCompleted += new EventHandler<RemoveNotesTemplateCompletedEventArgs>(mrsClient_RemoveNotesTemplateCompleted);
            mrsClient.RemoveNotesTemplateAsync(template.TemplateID.ToString(), userid);
            mrsClient = null;
        }

        //public void UpdateNoteTemplateItem(string templateid, string productareagroupid, decimal quantity, decimal price, string extradescription, int userid)
        //{
        //    mrsClient = new RetailSystemClient();
        //    mrsClient.UpdateNotesTemplateItemCompleted += new EventHandler<UpdateNotesTemplateItemCompletedEventArgs>(mrsClient_UpdateNotesTemplateItemCompleted);
        //    mrsClient.UpdateNotesTemplateItemAsync(templateid,productareagroupid, quantity, price, extradescription, userid);
        //    mrsClient = null;
        //}
        public void UpdateNoteTemplateItem(EstimateDetails pag)
        {
            mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            mrsClient.UpdateNotesTemplateItemCompleted += new EventHandler<UpdateNotesTemplateItemCompletedEventArgs>(mrsClient_UpdateNotesTemplateItemCompleted);
            mrsClient.UpdateNotesTemplateItemAsync(pag.TemplateID, pag.ProductAreaGroupID.ToString(), pag.Quantity, pag.Price, pag.ExtraDescription, pag.InternalDescription,pag.AdditionalNotes, userid, pag.UseDefaultQuantity);
            mrsClient = null;
        }
        void mrsClient_UpdateNotesTemplateItemCompleted(object sender, UpdateNotesTemplateItemCompletedEventArgs e)
        {
            if (e.Result)
            {
                //SearchNotesTemplate();
            }
        }
        void mrsClient_RemoveNotesTemplateCompleted(object sender, RemoveNotesTemplateCompletedEventArgs e)
        {
            if (e.Result)
            {
                SearchNotesTemplate();
            }
        }
        public void AddNewNoteTemplate()
        {
            if (NewTemplateName != "")
            {
                mrsClient = new RetailSystemClient();
                mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

                mrsClient.AddNewNotesTemplateCompleted += new EventHandler<AddNewNotesTemplateCompletedEventArgs>(mrsClient_AddNewNotesTemplateCompleted);
                mrsClient.AddNewNotesTemplateAsync(NewTemplateName, SelectedRegionId.ToString(), userid);
                mrsClient = null;
            }

        }
        void mrsClient_AddNewNotesTemplateCompleted(object sender, AddNewNotesTemplateCompletedEventArgs e)
        {
            if (e.Result)
            {
                SearchNotesTemplate();
            }
        }
        void mrsClient_AdditionalNotesTemplateAndProductsByRegionCompleted(object sender, GetAdditionalNotesTemplateAndProductsByRegionCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                RadWindow.Alert(e.Error.Message);
            }
            else
            {
                if (e.Result.Count>0 && e.Result[0].ProductAreaGroupID == -1)
                {
                    RadWindow.Alert(e.Result[0].ProductDescription.ToString());
                }
                else
                {
                    ManagerNotesTemplate.Clear();
                    foreach (var item in e.Result)
                    {
                        bool exists = false;
                        foreach (var header in ManagerNotesTemplate)
                        {
                            if (header.TemplateID.ToString() == item.TemplateID)
                            {
                                exists = true;
                                if (!header.NoteTemplateItem.Contains(item) && item.ProductAreaGroupID > 0)
                                {
                                    header.NoteTemplateItem.Add(item);
                                }
                                break;
                            }
                        }
                        if (!exists)
                        {
                            NoteTemplate note = new NoteTemplate();
                            note.RegionName = item.RegionName;
                            note.TemplateID = int.Parse(item.TemplateID);
                            note.TemplateName = item.TemplateName;
                            note.Active = item.TemplateActive;
                            note.OwnerName = item.OwnerName;
                            if ((App.Current as App).CurrentUserRoleId == 6)// if estimate manager, it's open
                            {
                                note.IsReadOnly = false;
                            }
                            else
                            {
                                note.IsReadOnly = (App.Current as App).CurrentUserFullName == item.OwnerName ? false : true;
                            }
                            note.IsEnabled = !note.IsReadOnly;
                            note.IsPrivate = item.IsPrivate;
                            if (item.ProductAreaGroupID > 0)
                            {
                                note.NoteTemplateItem.Add(item);
                            }
                            ManagerNotesTemplate.Add(note);
                        }
                    }
                }
            }
        }
        
        void mrsClient_GetSalesRegionByStateCompleted(object sender, GetSalesRegionByStateCompletedEventArgs e)
        {
            SalesRegions.Clear();
            SalesRegions.Add(new SQSSalesRegion() { RegionId = 0, RegionName = "All" });
            foreach (var item in e.Result)
            {
                SalesRegions.Add(item);
            }
            SelectedRegionId = (App.Current as App).CurrentRegionId;

            SearchNotesTemplate();
        }

        private void ClearSearchText()
        {
            this.TemplateNameSearchText = null;
            SelectedRegionId = (App.Current as App).CurrentRegionId;
            SelectedStatus = 1;
            SearchNotesTemplate();
        }

        public void UpdateNoteTemplateStatus(int templateid, int status, int userid, string action)
        {
            mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            mrsClient.UpdateNoteTemplateCompleted += new EventHandler<UpdateNoteTemplateCompletedEventArgs>(mrsClient_UpdateNoteTemplateCompleted);
            mrsClient.UpdateNoteTemplateAsync(templateid,"",status, userid,action);
            mrsClient = null;          
        }

        public void UpdateNoteTemplateName(int templateid, int userid, string templatename, string action)
        {
            mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            mrsClient.UpdateNoteTemplateCompleted += new EventHandler<UpdateNoteTemplateCompletedEventArgs>(mrsClient_UpdateNoteTemplateCompleted);
            mrsClient.UpdateNoteTemplateAsync(templateid, templatename, 1, userid, action);
            mrsClient = null;
        }

        void mrsClient_UpdateNoteTemplateCompleted(object sender, UpdateNoteTemplateCompletedEventArgs e)
        {
            if (e.Result)
            {
                SearchNotesTemplate();
            }
        }
        #endregion


        #region command region
        public ICommand RemoveNoteTemplateItemCommand
        {
            get
            {
                if (removenotetemplateitemCommand == null)
                {
                    removenotetemplateitemCommand = new DelegateCommand<EstimateDetails>(this.RemoveNoteTemplateItemFromTemplate);
                }

                return removenotetemplateitemCommand;
            }
        }
        public ICommand UpdateNoteTemplateItemCommand
        {
            get
            {
                if (updatenotetemplateitemCommand == null)
                {
                    updatenotetemplateitemCommand = new DelegateCommand<EstimateDetails>(this.UpdateNoteTemplateItem);
                }

                return updatenotetemplateitemCommand;
            }
        }
        public ICommand RemoveNoteTemplateCommand
        {
            get
            {
                if (removenotetemplateCommand == null)
                {
                    removenotetemplateCommand = new DelegateCommand<NoteTemplate>(this.RemoveNoteTemplate);
                }

                return removenotetemplateCommand;
            }
        }
        public ICommand SaveNewTemplateCommand
        {
            get
            {
                if (addnotetemplateCommand == null)
                {
                    addnotetemplateCommand = new Metricon.Silverlight.MetriconRetailSystem.Command.DelegateCommand(this.AddNewNoteTemplate);
                }

                return addnotetemplateCommand;
            }
        }
        public ICommand SearchNotesTemplateCommand
        {
            get
            {
                if (searchnotestempalteCommand == null)
                {
                    searchnotestempalteCommand = new Metricon.Silverlight.MetriconRetailSystem.Command.DelegateCommand(this.SearchNotesTemplate);
                }

                return searchnotestempalteCommand;
            }
        }
        public ICommand ClearSearchTextCommand
        {
            get
            {
                if (_clearsearchtextcommand == null)
                {
                    _clearsearchtextcommand = new Metricon.Silverlight.MetriconRetailSystem.Command.DelegateCommand(this.ClearSearchText);
                }

                return _clearsearchtextcommand;
            }
        }
        #endregion

        #region public propeties
        public string NewTemplateName
        {
            get
            {
                return _newtemplatename;
            }
            set
            {
                _newtemplatename = value;
                OnPropertyChanged("NewTemplateName");
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
        public ObservableCollection<NoteTemplate> ManagerNotesTemplate
        {
            get
            {
                return _managernotetemplate;
            }
            set
            {
                _managernotetemplate = value;
                OnPropertyChanged("ManagerNotesTemplate");
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
        public int SelectedStatus
        {
            get
            {
                return _selectedstatus;
            }

            set
            {
                if (_selectedstatus != value)
                {
                    _selectedstatus = value;
                    OnPropertyChanged("SelectedStatus");
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
        public ObservableCollection<EstimateDetails> AvailableNoteTemplateItem
        {
            get
            {
                return _availablenotetemplateitem;
            }
            set
            {
                _availablenotetemplateitem = value;
                OnPropertyChanged("AvailableNoteTemplateItem");
            }
        }

        public ObservableCollection<TemplateStatus> TemplateStatusArray
        {
            get
            {
                return _templatestatus;
            }
            set
            {
                _templatestatus = value;
                OnPropertyChanged("TemplateStatusArray");
            }
        }
        #endregion

        #region public class
        public class NoteTemplate
        {
            public NoteTemplate()
            {
                this.NoteTemplateItem = new ObservableCollection<EstimateDetails>();
            }
            public int TemplateID { get; set; }
            public string TemplateName { get; set; }
            public string RegionName { get; set; }
            public string OwnerName { get; set; }
            public bool Active { get; set; }
            public bool IsPrivate { get; set; }
            public bool IsReadOnly { get; set; }
            public bool IsEnabled { get; set; }
            public ObservableCollection<EstimateDetails> NoteTemplateItem { get; set; }

        }
        public class TemplateStatus
        {
            public int StatusID { get; set; }
            public string StatusName { get; set; }
        }
        #endregion
    }
}
