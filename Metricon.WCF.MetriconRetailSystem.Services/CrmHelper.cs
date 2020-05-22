using System;
using System.Web;
using System.ServiceModel.Description;
using System.Configuration;

// Namespaces in Microsoft.Xrm.Sdk.dll
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;

namespace Metricon.WCF.MetriconRetailSystem.Services
{
    public static class CrmHelper
    {
        public static OrganizationServiceProxy Connect()
        {
            OrganizationServiceProxy serviceProxy;
            Uri OrganizationUri = new Uri(ConfigurationManager.AppSettings["OrganizationUri"]);
            Uri HomeRealmUri = null;
            ClientCredentials Credentails = new ClientCredentials();
            ClientCredentials DeviceCredentials = null;
            Credentails.Windows.ClientCredential = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["UserName"],
                ConfigurationManager.AppSettings["Password"], 
                ConfigurationManager.AppSettings["Domain"]);

            serviceProxy = new OrganizationServiceProxy(OrganizationUri, HomeRealmUri, Credentails, DeviceCredentials);

            // This statement is required to enable early-bound type support.
            serviceProxy.ServiceConfiguration.CurrentServiceEndpoint.Behaviors.Add(new ProxyTypesBehavior());

            return serviceProxy;
        }
    }
}