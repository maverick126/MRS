namespace Metricon.Silverlight.MetriconRetailSystem.Internal
{
    using System;
    using System.Windows;
    using System.Windows.Browser;

    /// <summary>
    /// The internal utilities.
    /// </summary>
    internal static class Utilities
    {
        /// <summary>
        /// The production base URL.
        /// </summary>
        /// <remarks>This URL 'MRSWCF' matches the folder name in publish profile. Change both if required.</remarks>
        private static readonly string ProdoctionBaseUrl = $"{Application.Current.Host.Source.AbsoluteUri.Replace(Application.Current.Host.Source.AbsolutePath, string.Empty)}/MRSWCF/";

        /// <summary>
        /// Gets the Metricon RSM WCF endpoint URL.
        /// </summary>
        /// <returns>The URL.</returns>
        public static string GetMetriconRetailSystemWcfClientEndpointUrl()
        {
            return $"{GetMetriconRsmWcfServiceBaseUrl()}RetailSystemService.svc";
        }

        /// <summary>
        /// Gets the Metricon RSM WCF service base URL, based on the current URL in the browser address bar.
        /// </summary>
        /// <returns>The base URL.</returns>
        private static string GetMetriconRsmWcfServiceBaseUrl()
        {
            // Note: the URL used to be defined in web.config, which is static.
            //       In order to neutralize local debugging and publishing (not making
            //       manual changes after publish), this helper was invented to make
            //       URL switch dynamic. David L April 2020
            var currentUri = HtmlPage.Document.DocumentUri;
            return IsDebuggingLocally(currentUri) ? "http://localhost:3638/" : ProdoctionBaseUrl;
        }

        /// <summary>
        /// Determines if local debugging is in process, otherwise in deployment environment.
        /// </summary>
        /// <param name="currentPageUrl">The current page URL.</param>
        /// <returns>True if currently debugging, otherwise false.</returns>
        private static bool IsDebuggingLocally(Uri currentUrl)
        {
            return currentUrl.Host.StartsWith("localhost", StringComparison.OrdinalIgnoreCase)
                && currentUrl.Port != 80 && currentUrl.Port != 443; // Default HTTP and HTTPS port
        }
    }
}