using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Metricon.Silverlight.MetriconRetailSystem.Command
{
    class DelegateCommand<T> : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private readonly Action<T> executeMethod = null;
        private readonly Func<T, bool> canExecuteMethod = null;


        public DelegateCommand(Action<T> executeMethod)
            : this(executeMethod, null)
        {
        }

        public DelegateCommand(Action<T> executeMethod, Func<T, bool> canExecuteMethod)
        {
            if (executeMethod == null && canExecuteMethod == null)
                throw new ArgumentNullException("executeMethod");

            this.executeMethod = executeMethod;
            this.canExecuteMethod = canExecuteMethod;
        }

        public void Execute(T player)
        {
            if (executeMethod == null) return;
            executeMethod(player);
        }

        public bool CanExecute(T player)
        {
            if (canExecuteMethod == null) return true;
            return canExecuteMethod(player);
        }


        bool ICommand.CanExecute(object parameter)
        {
            return CanExecute((T)parameter);
        }

        void ICommand.Execute(object parameter)
        {
            Execute((T)parameter);
        }

    }
    class DelegateCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private readonly Action executeMethod = null;
        private readonly Func<bool> canExecuteMethod = null;

        public DelegateCommand(Action executeMethod)
            : this(executeMethod, null)
        {
        }

        public DelegateCommand(Action executeMethod, Func<bool> canExecuteMethod)
        {
            if (executeMethod == null && canExecuteMethod == null)
                throw new ArgumentNullException("executeMethod");

            this.executeMethod = executeMethod;
            this.canExecuteMethod = canExecuteMethod;
        }

        public void Execute()
        {
            if (executeMethod == null) return;
            executeMethod();
        }

        public bool CanExecute()
        {
            if (canExecuteMethod == null) return true;
            return canExecuteMethod();
        }


        bool ICommand.CanExecute(object parameter)
        {
            return CanExecute();
        }

        void ICommand.Execute(object parameter)
        {
            Execute();
        }

    }

}
