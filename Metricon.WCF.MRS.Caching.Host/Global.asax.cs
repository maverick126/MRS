using Autofac;
using Autofac.Integration.Wcf;
using Metricon.WCF.MRS.Caching.Services.Services;
using Metricon.WCF.MRS.Caching.Services.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace Metricon.WCF.MRS.Caching.Host
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            var builder = new ContainerBuilder();

            // Register your service implementations.
            builder.RegisterType<Services.RetailSystemCache>();
            builder.RegisterType<RedisCacheService>().As<ICacheService>();

            // Set the dependency resolver.
            AutofacHostFactory.Container = builder.Build(); ;
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}