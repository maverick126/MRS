namespace Metricon.Silverlight.MetriconRetailSystem.Host.Internal
{
    using System;
    using System.Web;

    /// <summary>
    /// The internal utilities.
    /// </summary>
    internal static class Utilities
    {
        /// <summary>
        /// Gets the Metricon SQS Sales web service URL.
        /// </summary>
        /// <returns>The URL.</returns>
        public static string GetMetriconSqsSalesWebServiceUrl()
        {
            return GetMetriconSqsWebServiceBaseUrl() + "MetriconSales.asmx";
        }

        /// <summary>
        /// Gets the Metricon SQS DocuSign web service URL.
        /// </summary>
        /// <returns>The URL.</returns>
        public static string GetMetriconSqsDocuSignWebServiceUrl()
        {
            return GetMetriconSqsWebServiceBaseUrl() + "DocuSignWebService.asmx";
        }

        /// <summary>
        /// Gets the Metricon SQS web service base URL, based on the current URL in the browser address bar.
        /// </summary>
        /// <returns>The base URL.</returns>
        private static string GetMetriconSqsWebServiceBaseUrl()
        {
            // Note: the URL used to be defined in web.config, which is static.
            //       In order to neutralize local debugging and publishing (not making
            //       manual changes after publish), this helper was invented to make
            //       URL switch dynamic. David L April 2020
            return IsDebuggingLocally() ? "http://localhost:50085/" :
                "http://localhost/SQSWS/"; // This URL matches the folder name in publish profile. Change both if required.
        }

        /// <summary>
        /// Determines if local debugging is in process, otherwise in deployment environment.
        /// </summary>
        /// <param name="currentPageUrl">The current page URL.</param>
        /// <returns></returns>
        private static bool IsDebuggingLocally()
        {
            var currentPageUrl = HttpContext.Current.Request.Url;
            return currentPageUrl.Host.StartsWith("localhost", StringComparison.OrdinalIgnoreCase)
                && currentPageUrl.Port != 80 && currentPageUrl.Port != 443; // Default HTTP and HTTPS port
        }
    }
}