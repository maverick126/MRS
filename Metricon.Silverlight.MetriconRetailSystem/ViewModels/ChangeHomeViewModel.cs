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
    public class ChangeHomeViewModel : ViewModelBase
    {
        private ObservableCollection<SQSHome> _list = new ObservableCollection<SQSHome>();
        private ObservableCollection<ValidationErrorMessage> _message = new ObservableCollection<ValidationErrorMessage>();
        private int stateid;
        public ChangeHomeViewModel(int state)
        {
            stateid = state;
            SearchResultVisibility = Visibility.Collapsed;
        }

        public void GetHomeList(string searchText)
        {

            //if (_list.Count == 0)
            //{
                RetailSystemClient MRSclient = new RetailSystemClient();
                MRSclient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

                MRSclient.GetAllAvailableHomeByStateCompleted += new EventHandler<GetAllAvailableHomeByStateCompletedEventArgs>(mrsClient_GetAllAvailableHomeByStateCompleted);
                MRSclient.GetAllAvailableHomeByStateAsync(stateid, searchText, false);
            //}
            //else
            //{
            //    mrsClient_GetAllAvailableHomeByStateCompleted(null, null);
            //}

        }

        void mrsClient_GetAllAvailableHomeByStateCompleted(object sender, GetAllAvailableHomeByStateCompletedEventArgs e)
        {
            _list.Clear();
            if (e != null)
            {
                if (e.Error == null && e.Result!=null)
                {
                    foreach (var p in e.Result)
                    {
                        _list.Add(p);
                    }
                    SearchResultCount = _list.Count;
                    SearchResultVisibility = Visibility.Visible;
                }
                else
                    ExceptionHandler.PopUpErrorMessage(e.Error, "mrsClient_GetAllAvailableHomeByStateCompleted");
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

        private int _searchResultCount;
        public int SearchResultCount
        {
            get
            {
                return _searchResultCount;
            }

            set
            {
                if (_searchResultCount != value)
                {
                    _searchResultCount = value;
                    OnPropertyChanged("SearchResultCount");
                }
            }
        }
        private Visibility _searchResultVisibility;
        public Visibility SearchResultVisibility
        {
            get
            {
                return _searchResultVisibility;
            }
            set
            {
                if (_searchResultVisibility != value)
                {
                    _searchResultVisibility = value;
                    OnPropertyChanged("SearchResultVisibility");
                }
            }
        }
        
        #endregion
    }
}
