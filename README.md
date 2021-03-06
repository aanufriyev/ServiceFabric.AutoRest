# ServiceFabric.AutoRest
![Build Status](https://mny.visualstudio.com/_apis/public/build/definitions/31e43162-b5bb-4610-b124-237497e9785b/21/badge)

**ServiceFabric.AutoRest** is a small client communication library, created to simplify using [AutoRest](https://github.com/Azure/autorest) generated client libraries in Azure Service Fabric reliable services. The primary use-case is service-to-service communication over HTTP leveraging the strongly typed client code generated by AutoRest.

Extends the capabilities found in the `Microsoft.ServiceFabric.Services.Communication.Client` namespace, such as automatic service uri resolution, retry logic, endpoint caching and more.

## Getting started

1. Add a Stateless (or Statefull) Web API service to your Service Fabric application. The service will be called by other Service Fabric services.
2. Add Swagger to the Web API service.
3. Generate client code from the Swagger specification exposed by the Web API service.
3. Install **ServiceFabric.AutoRest** from [nuget.org](https://www.nuget.org/packages/ServiceFabric.AutoRest) in another Service Fabric service that needs to communicate with the Web API service.
5. Create a shared instance of `RestCommunicationClientFactory`: 

		static RestCommunicationClientFactory<StatelessClient> communicationClientFactory;
		static ClientService()
		{  
			communicationClientFactory = new RestCommunicationClientFactory<StatelessClient>();            
		}
6. Create an instance of `RestServicePartitionClient<StatelessClient>` and use it to call the Web API Service:

		public async Task<IEnumerable<string>> GetValuesFromOtherService()
		{
			Uri serviceUri = new Uri("fabric:/ServiceFabric.AutoRest.Clients/WebApi");
			IRestServicePartitionClient<StatelessClient> partitionClient = new RestServicePartitionClient<StatelessClient>(
				communicationClientFactory, serviceUri, ServicePartitionKey.Singleton);

			var result = await partitionClient.InvokeWithRetryAsync(
				async c => await c.RestApi.Values.GetAllAsync());

			return result;
		}

## Features
* Dependency Injection support, through interfaces.
* Call multiple service partitions in Statefull services using the `RestServicePartitionClientFactory` class
* Leverages Service Fabric retry strategy (**IMPORTANT:** AutoRest' default retry strategy is disabled as it interferes with SF build-in retry strategy)
* Extendable exception handling
* Extendable http delegating handler pipeline
* Support for client credentials
* **New extension points** through events to better control client creation and validation

## Sample
[walk-through of sample to be added]


## Links
* [AutoRest - Generate client code from Swagger](https://github.com/Azure/autorest)
* [Swashbuckle - Add Swagger to your WebApi](https://github.com/domaindrivendev/Swashbuckle)
* [Communicate with a Service Fabric Reliable Service](https://docs.microsoft.com/en-us/azure/service-fabric/service-fabric-reliable-services-communication#communicating-with-a-service)