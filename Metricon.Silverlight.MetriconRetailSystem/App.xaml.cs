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
using Telerik.Windows.Controls;

namespace Metricon.Silverlight.MetriconRetailSystem
{
    public partial class App : Application
    {
        public int CurrentUserId { get; set; }
        public int CurrentRegionId { get; set; }
        public int LoginPriceRegionId { get; set; }
        public int CurrentUserRoleId { get; set; }
        public int CurrentUserPrimaryRoleId { get; set; }
        public bool IsManager { get; set; }
        public bool UserValidationFailed { get; set; }
        public int SelectedEstimateRevisionId { get; set; }
        public int SelectedTab { get; set; }
        public int SelectedTabIndexEstimateDetails { get; set; }
        public int SelectedRevisionTypeId { get; set; }
        public int SelectedStatusId { get; set; }
        public string CurrentAction { get; set; }
        public string CurrentUserFullName { get; set; }
        public string CurrentUserLoginName { get; set; }
        public string CurrentUserStateID { get; set; }

        // search filters
        public string CurrentCustomerNumber { get; set; }
        public string CurrentContractNumber { get; set; }
        public int CurrentSelectedSalesConsultantId { get; set; }
        public string CurrentLotNumber { get; set; }
        public string CurrentStreetName { get; set; }
        public string CurrentSuburb { get; set; }
        public int CurrentSelectedUserId { get; set; }

        public int SelectedEstimateRevisionTypeID { get; set; }
        public string SelectedContractType { get; set; }
        public string OpportunityID { get; set; }
        public string SelectedContractNumber { get; set; }

        #region role access region
            public MRSService.RoleAccessModule CurrentRoleAccessModule { get; set; }
            public bool SelectedEstimateAllowToAddNSR { get; set; }
            public bool SelectedEstimateValidateAccept { get; set; }
            public bool SelectedEstimateValidateStandardInclusion { get; set; }
            public bool SelectedEstimateAllowToAcceptItem { get; set; }
            public bool SelectedEstimateAllowToViewStudioMTab { get; set; }
            public bool SelectedEstimateAllowToViewStudioMDocuSign { get; set; }
        #endregion

        public App()
        {
            this.Startup += this.Application_Startup;
            this.Exit += this.Application_Exit;
            this.UnhandledException += this.Application_UnhandledException;
            StyleManager.ApplicationTheme = new Office_BlueTheme();
            InitializeComponent();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //Add InitParams to App Resources
            if (e.InitParams != null)
            {
                foreach (var data in e.InitParams)
                {
                    this.Resources.Add(data.Key, data.Value);
                }
            }  
            this.RootVisual = new FirstPage();

            if (Application.Current.IsRunningOutOfBrowser)
            {
                Application.Current.CheckAndDownloadUpdateAsync();
                Application.Current.CheckAndDownloadUpdateCompleted += new CheckAndDownloadUpdateCompletedEventHandler(Current_CheckAndDownloadUpdateCompleted);
            }

            //Always start at UserValidation page
            //((FirstPage)this.RootVisual).MainFrame.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
        }

        void Current_CheckAndDownloadUpdateCompleted(object sender, CheckAndDownloadUpdateCompletedEventArgs e)
        {
            if (e.UpdateAvailable)
            {
                MessageBox.Show("An application update has been downloaded. " +
                    "Restart the application to run the new version.");
            }
            else if (e.Error != null &&
                e.Error is PlatformNotSupportedException)
            {
                MessageBox.Show("An application update is available, " +
                    "but it requires a new version of Silverlight. " +
                    "Visit the application home page to upgrade.");
            }
            else
            {
                //no new version available
            }
        }

        private void Application_Exit(object sender, EventArgs e)
        {

        }

        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            // If the app is running outside of the debugger then report the exception using
            // the browser's exception mechanism. On IE this will display it a yellow alert 
            // icon in the status bar and Firefox will display a script error.
            if (!System.Diagnostics.Debugger.IsAttached)
            {

                // NOTE: This will allow the application to continue running after an exception has been thrown
                // but not handled. 
                // For production applications this error handling should be replaced with something that will 
                // report the error to the website and stop the application.
                e.Handled = true;
                Deployment.Current.Dispatcher.BeginInvoke(delegate { ReportErrorToDOM(e); });
            }
        }

        private void ReportErrorToDOM(ApplicationUnhandledExceptionEventArgs e)
        {
            try
            {
                string errorMsg = e.ExceptionObject.Message + e.ExceptionObject.StackTrace;
                errorMsg = errorMsg.Replace('"', '\'').Replace("\r\n", @"\n");

                System.Windows.Browser.HtmlPage.Window.Eval("throw new Error(\"Unhandled Error in Silverlight Application " + errorMsg + "\");");
            }
            catch (Exception)
            {
            }
        }
    }
}
