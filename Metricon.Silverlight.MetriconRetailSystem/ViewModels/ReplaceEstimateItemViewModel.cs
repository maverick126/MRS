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
    public class ReplaceEstimateItemViewModel : ViewModelBase
    {
        #region variables declaration
        private bool _isBusy;
        private bool _isbusyoptiontree;
        private bool _isbusyoptiontree3;
        private bool _isbusyoptiontree4;
        private bool _isBusyEstimateInfo;

        private RetailSystemClient mrsClient;

        private BackgroundWorker optionTreeWorker = new BackgroundWorker();

        #endregion

        #region public properties

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
        #endregion

        #region methods of the class

        public ReplaceEstimateItemViewModel()
        {
    
        }

        void optionTreeWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
        }

        #endregion

    
    }


}
