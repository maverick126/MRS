using System;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Collections.Generic;
using System.Windows;

using Metricon.Silverlight.MetriconRetailSystem.MRSService;
using Metricon.Silverlight.MetriconRetailSystem.Command;

namespace Metricon.Silverlight.MetriconRetailSystem.ViewModels
{
    public class DocuSignViewModel : ViewModelBase
    {
        public ObservableCollection<DocuSignDocStatusInfo> _docusignstatus;
        public ObservableCollection<Recipient> _contacts;
        public ObservableCollection<DocuSignAction> _action;
        public ObservableCollection<DocuSignMethod> _method;
        public ObservableCollection<UserType> _usertype;
        private bool _isBusy;
        private Visibility _surchargevisible;
        private string _estimateid;
        private string _revisionid;
        private string _revisiontypeid;
        private string _revisionnumber;
        private string _surchargemessage;
        private string _accountid;
        public string _documenttype = "";
        public ObservableCollection<Email> _email;
        public int oldmaxrclientoutingorder = 0;

        public DocuSignViewModel(string revisionid, string estimateid, string revisiontypeid, string revisionnumber, string accountid)
        {
            _revisionid = revisionid;
            _estimateid = estimateid;
            _revisiontypeid = revisiontypeid;
            _revisionnumber = revisionnumber;
            _accountid = accountid;

            DocuSignStatus = new ObservableCollection<DocuSignDocStatusInfo>();
            DocuSignMethods = new ObservableCollection<DocuSignMethod>();
            DocuSignUserType = new ObservableCollection<UserType>();
            Contacts = new ObservableCollection<Recipient>();
            DocuSignActions = new ObservableCollection<DocuSignAction>();
            DocuSignEmail = new ObservableCollection<Email>();

            GetDocumentType();
            ConstructAction();
            GetMethods();
            GetUserType();
            GetAreaSurcharge();
            GetEmail("");

            GetDocumentsFromDocuSign(revisionid, estimateid);

            //GetSingerList(_accountid);
        }
        public void GetEmail(string doctype)
        {
            DocuSignEmail.Clear();
            Email e = new Email();
            e.Subject = "Metricon – Please DocuSign " + (App.Current as App).SelectedContractNumber + " " + doctype + " " + _revisionnumber;
            e.Body = "Please review and sign document via the link above." + Environment.NewLine + Environment.NewLine + "Thanks" + Environment.NewLine + Environment.NewLine + "Metricon Team";
            DocuSignEmail.Add(e);

        }
        public void ConstructAction()
        {
            DocuSignAction a = new DocuSignAction();
            a.ActionID = 0;
            a.ActionName = "Action";

            DocuSignAction a1 = new DocuSignAction();
            a1.ActionID = 1;
            a1.ActionName = "Remote Sign";

            DocuSignAction a2 = new DocuSignAction();
            a2.ActionID = 2;
            a2.ActionName = "Sign in Office";

            DocuSignAction a3 = new DocuSignAction();
            a3.ActionID = 3;
            a3.ActionName = "Recieve a Copy";

            DocuSignActions.Add(a);
            DocuSignActions.Add(a1);
            DocuSignActions.Add(a2);
            DocuSignActions.Add(a3);
        }
        public void GetMethods()
        {
            DocuSignMethod a = new DocuSignMethod();
            a.MethodID = 0;
            a.MethodName = "Send";

            DocuSignMethod a1 = new DocuSignMethod();
            a1.MethodID = 1;
            a1.MethodName = "Void";

            DocuSignMethods.Add(a);
            DocuSignMethods.Add(a1);

        }

        public void GetUserType()
        {
            UserType a = new UserType();
            a.TypeID = 0;
            a.TypeName = "Customer";

            UserType a1 = new UserType();
            a1.TypeID = 1;
            a1.TypeName = "Staff";

            DocuSignUserType.Add(a);
            DocuSignUserType.Add(a1);

        }
        public void PushDocumentToProcessingQueue(string revisionid, string printtype, string documenttype, int userid)
        {
            RetailSystemClient mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());
            mrsClient.DocuSign_PushDocumentToTheProcessQueueCompleted += new EventHandler<DocuSign_PushDocumentToTheProcessQueueCompletedEventArgs>(DocuSign_PushDocumentToTheProcessQueueCompleted);
            mrsClient.DocuSign_PushDocumentToTheProcessQueueAsync(revisionid, printtype, documenttype, userid);
        }
        private void ContructDocumentStatus()
        {
            IsBusy = true;
            if ((!_documenttype.ToUpper().Contains("VARIATION") && !(App.Current as App).SelectedEstimateAllowToViewStudioMTab) || _documenttype.ToUpper().Contains("CONTRACT"))
            {
                DocuSignDocStatusInfo doc1 = new DocuSignDocStatusInfo();
                doc1.deleted = "0";
                //doc1.versiontype = "CUSTOMER";
                doc1.versiontype = "CUSTOMER";
                doc1.estimateid = _estimateid;
                doc1.printtype = "FULLDETAILS";
                doc1.revisionnumber = _revisionnumber;
                doc1.EnableSendViaDocuSign = true;
                doc1.EnableSignInPerson = true;
                doc1.EnableVoid = false;
                doc1.Selected = false;
                doc1.documentnumber = "0";
                doc1.documenttype = _documenttype;

                DocuSignStatus.Add(doc1);

                DocuSignDocStatusInfo doc2 = new DocuSignDocStatusInfo();
                doc2.deleted = "0";
                doc2.versiontype = "CUSTOMER";
                doc2.estimateid = _estimateid;
                doc2.printtype = "LUMPSUM";
                doc2.revisionnumber = _revisionnumber;
                doc2.EnableSendViaDocuSign = true;
                doc2.EnableSignInPerson = true;
                doc2.EnableVoid = false;
                doc2.Selected = false;
                doc2.documentnumber = "0";
                doc2.documenttype = _documenttype;

                DocuSignStatus.Add(doc2);

                DocuSignDocStatusInfo doc3 = new DocuSignDocStatusInfo();
                doc3.deleted = "0";
                doc3.versiontype = "CUSTOMER";
                doc3.estimateid = _estimateid;
                doc3.printtype = "FULLSUMMARY";
                doc3.revisionnumber = _revisionnumber;
                doc3.EnableSendViaDocuSign = true;
                doc3.EnableSignInPerson = true;
                doc3.EnableVoid = false;
                doc3.Selected = false;
                doc3.documentnumber = "0";
                doc3.documenttype = _documenttype;

                DocuSignStatus.Add(doc3);
            }
            if ((App.Current as App).SelectedEstimateAllowToViewStudioMDocuSign)
            {
                DocuSignDocStatusInfo doc4 = new DocuSignDocStatusInfo();
                doc4.deleted = "0";
                doc4.versiontype = "STUDIOM";
                doc4.estimateid = _estimateid;
                doc4.printtype = "";
                doc4.revisionnumber = _revisionnumber;
                doc4.EnableSendViaDocuSign = true;
                doc4.EnableSignInPerson = true;
                doc4.EnableVoid = false;
                doc4.Selected = false;
                doc4.documentnumber = "0";
                doc4.documenttype = _documenttype;

                DocuSignStatus.Add(doc4);
            }

            if (_documenttype.ToUpper().Contains("VARIATION"))
            {
                DocuSignDocStatusInfo doc5 = new DocuSignDocStatusInfo();
                doc5.deleted = "0";
                doc5.versiontype = "CHANGEONLY";
                doc5.estimateid = _estimateid;
                doc5.printtype = "";
                doc5.revisionnumber = _revisionnumber;
                doc5.EnableSendViaDocuSign = true;
                doc5.EnableSignInPerson = true;
                doc5.EnableVoid = false;
                doc5.Selected = false;
                doc5.documentnumber = "0";
                doc5.documenttype = _documenttype;

                DocuSignStatus.Add(doc5);
            }
        }

        public void GetDocumentsFromDocuSign(string revisionid, string estimateid)
        {

            RetailSystemClient mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            mrsClient.DocuSign_GetDocumentInfoCompleted += new EventHandler<DocuSign_GetDocumentInfoCompletedEventArgs>(mrsClient_DocuSign_GetDocumentInfoCompleted);
            mrsClient.DocuSign_GetDocumentInfoAsync(revisionid, estimateid);
        }

        public void GetDocumentType()
        {
            RetailSystemClient MRSclient = new RetailSystemClient();
            MRSclient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            MRSclient.GetCustomerDocumentTypeCompleted += delegate(object o, GetCustomerDocumentTypeCompletedEventArgs es)
            {
                if (es.Error == null)
                {
                    _documenttype = es.Result.ToString();
                }
                else
                    ExceptionHandler.PopUpErrorMessage(es.Error, "GetCustomerDocumentTypeCompleted");

            };

            MRSclient.GetCustomerDocumentTypeAsync(EstimateList.SelectedEstimateRevisionId);
        }

        void DocuSign_PushDocumentToTheProcessQueueCompleted(object sender, DocuSign_PushDocumentToTheProcessQueueCompletedEventArgs e)
        {
            if (e.Error == null)
            {

            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "DocuSign_PushDocumentToTheProcessQueueCompleted");

            IsBusy = false;
        }

        public void RemoveDocuSignDocumentsFromQueue(string integrationid)
        {

            RetailSystemClient mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            mrsClient.DocuSign_RemoveDocumentFromTheProcessQueueCompleted += new EventHandler<DocuSign_RemoveDocumentFromTheProcessQueueCompletedEventArgs>(mrsClient_DocuSign_RemoveDocumentFromTheProcessQueueCompleted);
            mrsClient.DocuSign_RemoveDocumentFromTheProcessQueueAsync(integrationid);
        }

        void mrsClient_DocuSign_RemoveDocumentFromTheProcessQueueCompleted(object sender, DocuSign_RemoveDocumentFromTheProcessQueueCompletedEventArgs e)
        {
            if (e.Error == null)
            {

            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "DocuSign_RemoveDocumentFromTheProcessQueueCompleted");

        }

        void mrsClient_DocuSign_GetDocumentInfoCompleted(object sender, DocuSign_GetDocumentInfoCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                ContructDocumentStatus();
                foreach (DocuSignDocStatusInfo dv in DocuSignStatus)
                {
                    foreach (DocuSignDocStatusInfo item in e.Result)
                    {
                        if (dv.versiontype == item.versiontype && dv.printtype == item.printtype && dv.revisionnumber == item.revisionnumber)
                        {
                            dv.statusChangedDateTime = item.statusChangedDateTime;
                            dv.status = item.status;
                            dv.EnableSendViaDocuSign = item.EnableSendViaDocuSign;
                            dv.EnableSignInPerson = item.EnableSignInPerson;
                            dv.EnableVoid = item.EnableVoid;
                            dv.integrationid = item.integrationid;
                            dv.envelopeId = item.envelopeId;
                            dv.estimateid = item.estimateid;
                            dv.documentnumber = item.documentnumber;
                        }

                        dv.documenttype = item.documenttype;
                    }
                    if (dv.versiontype == "CUSTOMER")
                    {
                        dv.versiontype = "Customer (" + dv.printtype.Replace("FULLDETAILS", "Full Details").Replace("FULLSUMMARY", "Full Summary").Replace("LUMPSUM", "Lump Sum") + ")";
                    }
                    else if (dv.versiontype == "STUDIOM")
                    {
                        dv.versiontype = "Studio M";
                    }
                    else
                    {
                        dv.versiontype = "Variation";
                    }
                }
                GetSingerList(_accountid);
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "DocuSign_GetDocumentInfoCompleted");

            //IsBusy = false;
        }


        public void GetSingerList(string accountid)
        {

            RetailSystemClient mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            mrsClient.GetCRMContactForAccountAsSignerCompleted += new EventHandler<GetCRMContactForAccountAsSignerCompletedEventArgs>(_mrsClient_GetCRMContactForAccountAsSignerCompleted);
            mrsClient.GetCRMContactForAccountAsSignerAsync(new Guid(accountid));
        }
        public int GetMaxClientRoutingOrder()
        {
            int result = 0;
            foreach (Recipient ep in Contacts)
            {
                if (ep.RecipientType.ToLower() == "customer")
                {
                    if (ep.RoutingOrder > result)
                    {
                        result = ep.RoutingOrder;
                    }
                }
            }

            return result;
        }

        public void UpdateStaffRoutingOrder(int maxclientorder)
        {
            foreach (Recipient ep in Contacts)
            {
                if (ep.RecipientType.ToLower() != "customer")
                {
                     ep.RoutingOrder=ep.RoutingOrder+maxclientorder-1;
                }
            }
        }
        public void AddUserToSingerList(string username, string email, string type)
        {
            int maxsortorder = GetMaxSortOrderForRecipients();

            Recipient contact = new Recipient();
            contact.RoutingOrder = GetMaxClientRoutingOrder()+1;
            contact.RecipientName = username;
            contact.RecipientEmail = email;
            contact.RecipientType = type;
            contact.ManualAdded = true;
            contact.SelectedAction = 1;
            
            if (type.ToLower() == "customer")
            {

                contact.IconImage = "../images/user_go.png";
            }
            else
            {
                if (maxsortorder < 100)
                {
                    contact.SortOrder = 101;
                }
                else
                {
                    contact.SortOrder = maxsortorder + 1;
                }
                contact.IconImage = "../images/met16X16.jpg";
            }
            Contacts.Add(contact);
        }

        public int GetMaxSortOrderForRecipients()
        {
            int maxsortorder = 0;
            foreach (Recipient r in Contacts)
            {
                if (maxsortorder < r.SortOrder)
                {
                    maxsortorder = r.SortOrder;
                }
            }

            return maxsortorder;

        }

        void _mrsClient_GetCRMContactForAccountAsSignerCompleted(object sender, GetCRMContactForAccountAsSignerCompletedEventArgs e)
        {
            int j = 1;
            if (e.Error == null)
            {
                foreach (CRMContact cc in e.Result)
                {
                    Recipient rc = new Recipient();
                    rc.RecipientName = cc.FullName;
                    rc.RecipientEmail = cc.Email;
                    rc.RoutingOrder = 1;
                    rc.Selected = false;
                    rc.SelectedAction = 1;
                    rc.SortOrder = cc.SortOrder;
                    rc.RecipientType = "Customer";
                    rc.IconImage = "../images/user_go.png";
                    rc.ManualAdded = false;
                    Contacts.Add(rc);

                    oldmaxrclientoutingorder = GetMaxClientRoutingOrder();
                }
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetAreaSurchargeCompleted");

            IsBusy = false;
        }


        public void GetAreaSurcharge()
        {
            RetailSystemClient _mrsClient = new RetailSystemClient();
            _mrsClient = new RetailSystemClient();
            _mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());
            _mrsClient.GetAreaSurchargeCompleted += new EventHandler<GetAreaSurchargeCompletedEventArgs>(_mrsClient_GetAreaSurchargeCompleted);
            _mrsClient.GetAreaSurchargeAsync(int.Parse(_revisionid));
        }
        void _mrsClient_GetAreaSurchargeCompleted(object sender, GetAreaSurchargeCompletedEventArgs e)
        {

            if (e.Error == null)
            {
                if (e.Result[0] == 1)
                {
                    SurchargeMessage = "There is a " + e.Result[1].ToString("c") + @" surcharge, merge it into home price";
                    SurchargeVisible = Visibility.Visible;
                }
                else
                {
                    SurchargeMessage = "";
                    SurchargeVisible = Visibility.Collapsed;
                }

            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetAreaSurchargeCompleted");
        }


        public ObservableCollection<DocuSignDocStatusInfo> DocuSignStatus
        {
            get
            {
                return _docusignstatus;
            }
            set
            {
                _docusignstatus = value;
                OnPropertyChanged("DocuSignStatus");
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

        public Visibility SurchargeVisible
        {
            get
            {
                return _surchargevisible;
            }

            set
            {
                if (_surchargevisible != value)
                {
                    _surchargevisible = value;
                    OnPropertyChanged("SurchargeVisible");
                }
            }
        }
        public string SurchargeMessage
        {
            get
            {
                return _surchargemessage;
            }

            set
            {
                if (_surchargemessage != value)
                {
                    _surchargemessage = value;
                    OnPropertyChanged("SurchargeMessage");
                }
            }
        }

        public ObservableCollection<Recipient> Contacts
        {
            get
            {
                return _contacts;
            }

            set
            {
                if (_contacts != value)
                {
                    _contacts = value;
                    OnPropertyChanged("Contacts");
                }
            }
        }

        public ObservableCollection<DocuSignAction> DocuSignActions
        {
            get
            {
                return _action;
            }

            set
            {
                if (_action != value)
                {
                    _action = value;
                    OnPropertyChanged("DocuSignActions");
                }
            }
        }

        public ObservableCollection<DocuSignMethod> DocuSignMethods
        {
            get
            {
                return _method;
            }

            set
            {
                if (_method != value)
                {
                    _method = value;
                    OnPropertyChanged("DocuSignMethods");
                }
            }
        }
        public ObservableCollection<UserType> DocuSignUserType
        {
            get
            {
                return _usertype;
            }

            set
            {
                if (_usertype != value)
                {
                    _usertype = value;
                    OnPropertyChanged("DocuSignUserType");
                }
            }
        }
        public ObservableCollection<Email> DocuSignEmail
        {
            get
            {
                return _email;
            }

            set
            {
                if (_email != value)
                {
                    _email = value;
                    OnPropertyChanged("DocuSignEmail");
                }
            }
        }
 
    }

    public class Recipient
    {
        public string RecipientName { get; set; }
        public string RecipientEmail { get; set; }
        public int RoutingOrder { get; set; }
        public bool Selected { get; set; }
        public int SelectedAction { get; set; }
        public int SortOrder { get; set; }
        public string RecipientType { get; set; }
        public string IconImage { get; set; }
        public bool ManualAdded { get; set; }
    }

    public class DocuSignAction
    {
        public int ActionID { get; set; }
        public string ActionName { get; set; }
    }

    public class DocuSignMethod
    {
        public int MethodID { get; set; }
        public string MethodName { get; set; }
    }
    public class Email
    {
        public string Subject { get; set; }
        public string Body { get; set; }
    }
    public class UserType
    {
        public int TypeID { get; set; }
        public string TypeName { get; set; }
    }
    public class DocumentAttributes
    {
        public string VersionType { get; set; }
        public string PrintType { get; set; }
        public string VersionNumber { get; set; }
        public string Status { get; set; }
        public bool EnableSendViaDocuSign { get; set; }
        public bool EnableSignInPerson { get; set; }
        public bool EnableVoid { get; set; }
        public string EnvelopeID { get; set; }
        public string IntegrationID { get; set; }
    }
}
