using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Navigation;

using Metricon.Silverlight.MetriconRetailSystem.MRSService;

namespace Metricon.Silverlight.MetriconRetailSystem
{
    public partial class UserValidation : Page
    {
        public UserValidation()
        {
            InitializeComponent();

            BusyIndicator1.IsBusy = true;

            if ((App.Current as App).UserValidationFailed)
            {
                txtValidationFailed.Visibility = System.Windows.Visibility.Visible;
                BusyIndicator1.IsBusy = false;
            }
        }

        // Executes when the user navigates to this page.
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }
    }
}
