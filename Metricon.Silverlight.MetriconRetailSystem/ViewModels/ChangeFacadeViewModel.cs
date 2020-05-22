using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Telerik.Windows.Controls;
using Metricon.Silverlight.MetriconRetailSystem.Command;
using Metricon.Silverlight.MetriconRetailSystem.MRSService;
using Metricon.Silverlight.MetriconRetailSystem.ChildWindows;

namespace Metricon.Silverlight.MetriconRetailSystem.ViewModels
{
    public class ChangeFacadeViewModel : ViewModelBase
    {
        private ObservableCollection<SQSHome> _list = new ObservableCollection<SQSHome>();
        private ObservableCollection<ValidationErrorMessage> _message = new ObservableCollection<ValidationErrorMessage>();
        private int revid;
        public ChangeFacadeViewModel(int revisionid)
        {
            revid = revisionid;
            GetHomeList();
        }

        public void GetHomeList()
        {

            if (_list.Count == 0)
            {
                RetailSystemClient MRSclient = new RetailSystemClient();
                MRSclient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

                MRSclient.GetAllFacadeFromRevisonIDCompleted += new EventHandler<GetAllFacadeFromRevisonIDCompletedEventArgs>(mrsClient_GetAllFacadeFromRevisonIDCompleted);
                MRSclient.GetAllFacadeFromRevisonIDAsync(revid);
            }
            else
            {
                mrsClient_GetAllFacadeFromRevisonIDCompleted(null, null);
            }

        }

        void mrsClient_GetAllFacadeFromRevisonIDCompleted(object sender, GetAllFacadeFromRevisonIDCompletedEventArgs e)
        {
            if (e != null)
            {
                if (e.Error == null)
                {
                    foreach (var p in e.Result)
                    {
                        _list.Add(p);
                    }
                }
                else
                    ExceptionHandler.PopUpErrorMessage(e.Error, "GetAllFacadeFromRevisonIDCompleted");
            }
        }



        #region public properties
        public ObservableCollection<SQSHome> HomeList
        {
            get
            {
                return _list;
            }

            set
            {
                if (_list != value)
                {
                    _list = value;
                    OnPropertyChanged("HomeList");
                }
            }
        }

        public ObservableCollection<ValidationErrorMessage> ErrorMessageList
        {
            get
            {
                return _message;
            }

            set
            {
                if (_message != value)
                {
                    _message = value;
                    OnPropertyChanged("ErrorMessageList");
                }
            }
        }
        #endregion
    }
}
