﻿using Microsoft.Rest;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceFabric.AutoRest.Communication
{
    public class AutoRestCommunicationClientFactory<TServiceClient> : CommunicationClientFactoryBase<AutoRestCommunicationClient<TServiceClient>>
        where TServiceClient : ServiceClient<TServiceClient>
    {
        private readonly Func<IEnumerable<DelegatingHandler>> delegatingHandlers;

        public AutoRestCommunicationClientFactory(
            IServicePartitionResolver resolver = null,
            IEnumerable<IExceptionHandler> exceptionHandlers = null,
            Func<IEnumerable<DelegatingHandler>> delegatingHandlers = null)
            : base(resolver, CreateExceptionHandlers(exceptionHandlers))
        {
            this.delegatingHandlers = delegatingHandlers;
        }

        protected override void AbortClient(AutoRestCommunicationClient<TServiceClient> client)
        {            
            // HTTP clients don't hold persistent connections, so no action is taken.
        }

        protected override Task<AutoRestCommunicationClient<TServiceClient>> CreateClientAsync(string endpoint, CancellationToken cancellationToken)
        {
            var baseUri = new Uri(endpoint);
            var handlers = delegatingHandlers?.Invoke()?.ToArray() ?? new DelegatingHandler[0];

            var client = (TServiceClient) Activator.CreateInstance(typeof(TServiceClient), baseUri, handlers);
            
            // disabling AutoRest retry policy since Service Fabric has own retry logic.
            client.SetRetryPolicy(null);

            return Task.FromResult(new AutoRestCommunicationClient<TServiceClient>(client));                        
        }

        protected override bool ValidateClient(AutoRestCommunicationClient<TServiceClient> client)
        {            
            // HTTP clients don't hold persistent connections, so no validation needs to be done.
            return true;
        }

        protected override bool ValidateClient(string endpoint, AutoRestCommunicationClient<TServiceClient> client)
        {            
            // HTTP clients don't hold persistent connections, so no validation needs to be done.
            return true;
        }

        private static IEnumerable<IExceptionHandler> CreateExceptionHandlers(IEnumerable<IExceptionHandler> userDefinedHandlers)
        {
            var list = new List<IExceptionHandler>();

            if (userDefinedHandlers != null)
            {
                list.AddRange(userDefinedHandlers);
            }

            return list.Union(new[] {new DefaultHttpOperationExceptionHandler()});
        }
    }    
}
