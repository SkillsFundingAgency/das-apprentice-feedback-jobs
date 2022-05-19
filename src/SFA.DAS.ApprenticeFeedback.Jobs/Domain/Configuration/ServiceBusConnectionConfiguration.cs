using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Domain.Configuration
{
    public class ServiceBusConnectionConfiguration
    {
        private const string fullyQualifiedNamespaceConfigSetting = "AzureWebJobsServiceBus:fullyQualifiedNamespace";
        private const string serviceBusConnectionConfigSetting = "AzureWebJobsServiceBus";
        
        public string ConnectionString { get; }
        public ConnectionAuthenticationType ConnectionType { get; }

        public ServiceBusConnectionConfiguration(string connectionString, ConnectionAuthenticationType connectionType)
        {
            ConnectionString = connectionString;
            ConnectionType = connectionType;
        }

        public enum ConnectionAuthenticationType
        {
            Unknown = 0,
            ManagedIdentity = 1,
            SharedAccessKey = 2,
        }

        public static ServiceBusConnectionConfiguration GetServiceBusConnectionString(IConfiguration configuration)
        {
            var connectionString = configuration.GetValue<string>(fullyQualifiedNamespaceConfigSetting);
            var connectionType = ConnectionAuthenticationType.ManagedIdentity;

            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = configuration.GetValue<string>(serviceBusConnectionConfigSetting);
                connectionType = ConnectionAuthenticationType.SharedAccessKey;
            }

            if (connectionString == null)
            {
                throw new InvalidOperationException("ConnectionString must be set against AzureWebJobServiceBus for Local or AzureWebJobServiceBus__fullyQualifiedNamespace for environments");
            }

            return new ServiceBusConnectionConfiguration(connectionString, connectionType);
        }

    }
}
