using System;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Collections.Generic;
using System.Windows;
using Metricon.Silverlight.MetriconRetailSystem.MRSService;

namespace Metricon.Silverlight.MetriconRetailSystem.ViewModels
{
    
    public class DocusignHistoryViewModel: ViewModelBase
    {
        public ObservableCollection<DocuSignHistory> _docusignhistory;
        private string _envelopeid="";
        private bool _isBusy;

        public DocusignHistoryViewModel(string envelopeid, string revisionid, string versiontype, string printtype)
        {
            _envelopeid = envelopeid;

            DocuSignHistoryList = new ObservableCollection<DocuSignHistory>();
            //GetDocuSignHistory(_envelopeid);
            GetDocuSignHistoryByRevision(revisionid, versiontype, printtype);
        }

        public void GetDocuSignHistory(string envelopeid)
        {
            RetailSystemClient mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());
            mrsClient.DocuSign_GetEnvelopeHistoryCompleted += new EventHandler<DocuSign_GetEnvelopeHistoryCompletedEventArgs>(DocuSign_DocuSign_GetEnvelopeHistoryCompleted);
            mrsClient.DocuSign_GetEnvelopeHistoryAsync(envelopeid);            
        }

        void DocuSign_DocuSign_GetEnvelopeHistoryCompleted(object sender, DocuSign_GetEnvelopeHistoryCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                DocuSignHistoryList = e.Result;
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "DocuSign_DocuSign_GetEnvelopeHistoryCompleted");

            IsBusy = false;
        }


        public void GetDocuSignHistoryByRevision(string revisionid, string versiontype, string printtype)
        {
            RetailSystemClient mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());
            mrsClient.DocuSign_GetEnvelopeHistoryByRevisionCompleted += new EventHandler<DocuSign_GetEnvelopeHistoryByRevisionCompletedEventArgs>(DocuSign_GetEnvelopeHistoryByRevisionCompleted);
            mrsClient.DocuSign_GetEnvelopeHistoryByRevisionAsync(revisionid,versiontype,printtype);
        }

        void DocuSign_GetEnvelopeHistoryByRevisionCompleted(object sender, DocuSign_GetEnvelopeHistoryByRevisionCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                DocuSignHistoryList = e.Result;
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "DocuSign_GetEnvelopeHistoryByRevision");

            IsBusy = false;
        }

        public ObservableCollection<DocuSignHistory> DocuSignHistoryList
        {
            get
            {
                return _docusignhistory;
            }

            set
            {
                if (_docusignhistory != value)
                {
                    _docusignhistory = value;
                    OnPropertyChanged("DocuSignHistoryList");
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
    }
}
