using System;
using System.ComponentModel;

namespace Metricon.Silverlight.MetriconRetailSystem.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public bool IsDesignTime
        {
            get { return DesignerProperties.IsInDesignTool; }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propName)
        {
            var propChanged = PropertyChanged;
            if (propChanged != null)
            {
                propChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        #endregion
    }
}
