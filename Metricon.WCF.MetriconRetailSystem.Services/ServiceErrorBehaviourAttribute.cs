using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Metricon.WCF.MetriconRetailSystem.Services
{
    /// <summary>
    /// So we can decorate Services with the [ServiceErrorBehaviour(typeof(HttpErrorHandler))]
    /// ...and errors reported to ELMAH
    /// </summary>
    public class ServiceErrorBehaviourAttribute : Attribute, IServiceBehavior
    {
        readonly Type _errorHandlerType;

        public ServiceErrorBehaviourAttribute(Type errorHandlerType)
        {
            this._errorHandlerType = errorHandlerType;
        }

        public void Validate(ServiceDescription description, ServiceHostBase serviceHostBase)
        {
        }

        public void AddBindingParameters(ServiceDescription description, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection parameters)
        {
        }

        public void ApplyDispatchBehavior(ServiceDescription description, ServiceHostBase serviceHostBase)
        {
            var errorHandler = (IErrorHandler)Activator.CreateInstance(_errorHandlerType);
            foreach (var channelDispatcher in serviceHostBase.ChannelDispatchers.OfType<ChannelDispatcher>())
            {
                channelDispatcher.ErrorHandlers.Add(errorHandler);
            }
        }
    }
}