using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using Metricon.Silverlight.MetriconRetailSystem.Command;
using Metricon.Silverlight.MetriconRetailSystem.MRSService;
using Metricon.Silverlight.MetriconRetailSystem.ChildWindows;

namespace Metricon.Silverlight.MetriconRetailSystem.ViewModels
{
    public class NotesTemplateItemsViewModel : ViewModelBase
    {
        #region variables declaration
        private ObservableCollection<EstimateDetails> _availablenotetemplateitem = new ObservableCollection<EstimateDetails>();
        private ObservableCollection<EstimateDetails> _selectednotetemplateitem = new ObservableCollection<EstimateDetails>();
        private RetailSystemClient mrsClient2;
        //private DelegateCommand saveCommand;
        private DelegateCommand _clearCommand;
        private string selecteditemids = "";
        private string _itemsearchtext = "";
       
        #endregion

        public NotesTemplateItemsViewModel()
        {
            if (!IsDesignTime)
            {
                SearchItem();
            }
            
        }
        public void SearchItem()
        {
            mrsClient2 = new RetailSystemClient();
            mrsClient2.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            mrsClient2.GetAvailableItemsForNotesTemplateCompleted += new EventHandler<GetAvailableItemsForNotesTemplateCompletedEventArgs>(mrsClient2_GetAvailableItemsForNotesTemplateCompleted);
            mrsClient2.GetAvailableItemsForNotesTemplateAsync(AddNotesTempateItems._templateid, ItemSearchText);
            //mrsClient2 = null;
        }

        public void mrsClient2_GetAvailableItemsForNotesTemplateCompleted(object sender, GetAvailableItemsForNotesTemplateCompletedEventArgs e)
        {
            this.AvailableNoteTemplateItem = e.Result;
        }

        //public void SaveItemsToNoteTempLate()
        //{
        //    int userid=(App.Current as App).CurrentUserId;
        //    foreach (var item in AvailableNoteTemplateItem)
        //    {
        //        if (item.PromotionProduct) // here reuse this column to hold the selection of check box
        //        {
        //            if (selecteditemids == "")
        //            {
        //                selecteditemids = item.ProductAreaGroupID.ToString();
        //            }
        //            else
        //            {
        //                selecteditemids = selecteditemids+","+ item.ProductAreaGroupID.ToString();
        //            }
        //            _selectednotetemplateitem.Add(item);
        //        }
        //    }
        //    mrsClient2 = new RetailSystemClient();
        //    mrsClient2.AddItemToNotesTemplateCompleted += new EventHandler<AddItemToNotesTemplateCompletedEventArgs>(mrsClient2_AddItemToNotesTemplateCompleted);
        //    mrsClient2.AddItemToNotesTemplateAsync(AddNotesTempateItems._templateid, selecteditemids,userid);
        //    mrsClient2 = null;
        //}
        public void mrsClient2_AddItemToNotesTemplateCompleted(object sender, AddItemToNotesTemplateCompletedEventArgs e)
        {
            if (e.Result)
            {
                foreach (var item in _selectednotetemplateitem)
                {
                    AvailableNoteTemplateItem.Remove(item);
                }
            }
            
        }
        public void ClearSearchFilter()
        {
            ItemSearchText = "";
            SearchItem();
        }

        #region public properties
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
        public string ItemSearchText
        {
            get
            {
                return _itemsearchtext;
            }

            set
            {
                if (_itemsearchtext != value)
                {
                    _itemsearchtext = value;
                    OnPropertyChanged("ItemSearchText");
                }
            }
        }
        #endregion
        #region command
        //public ICommand SaveCommand
        //{
        //    get
        //    {
        //        if (saveCommand == null)
        //        {
        //            saveCommand = new DelegateCommand(this.SaveItemsToNoteTempLate);
        //        }

        //        return saveCommand;
        //    }
        //}
        public ICommand ClearCommand
        {
            get
            {
                if (_clearCommand == null)
                {
                    _clearCommand = new DelegateCommand(this.ClearSearchFilter);
                }

                return _clearCommand;
            }
            //private set;
        }
        #endregion
    }
}
