using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Data;

using Metricon.Silverlight.MetriconRetailSystem.MRSService;
namespace Metricon.Silverlight.MetriconRetailSystem.ViewModels
{
    public class PAGViewModel : INotifyPropertyChanged
    {
        ObservableCollection<PAG> _pags = new ObservableCollection<PAG>();
        ObservableCollection<PAG> _options = new ObservableCollection<PAG>();
        ObservableCollection<EstimateComments> _comments = new ObservableCollection<EstimateComments>();
        public ObservableCollection<PAG> PAGs
        {
            get
            {
                return _pags;
            }
            set
            {
                _pags = value;
                OnPropertyChanged("PAGs");
            }
        }

        public ObservableCollection<PAG> Options
        {
            get
            {
                return _options;
            }
            set
            {
                _options = value;
                OnPropertyChanged("Options");
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

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }
}
