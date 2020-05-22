using System;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using Elmah;

namespace Metricon.WCF.MRS.Caching.Services
{
    /// <summary>
    /// Your handler to actually tell ELMAH about the problem.
    /// </summary>
    public class HttpErrorHandler : IErrorHandler
    {
        public bool HandleError(Exception error)
        {
            return false;
        }

        public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
        {
            if (error == null) return;

            ErrorLog.GetDefault(null).Log(new Error(error));
        }
    }
}