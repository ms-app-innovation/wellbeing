# Event Driven Architecture on Azure - WellBeing Sample

## Demo Site

[Click here to open the demo site](https://gentle-pebble-07d15961e.1.azurestaticapps.net/)

## Quickstart

[![Deploy to Azure](https://aka.ms/deploytoazurebutton)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fms-app-innovation%2Fwellbeing%2Fmain%2Fsrc%2Finfra%2Fazuredeploy.json)

### Setup Local Development Environment
The following section lets you create your own local development environment for testing, debugging, and contributing. 

### Prerequisites
1. [VS Code](https://code.visualstudio.com/) or [Visual Studio 2022](https://visualstudio.microsoft.com/vs/)
2. [.NET 6.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
3. [Azure Functions Core Tools](https://github.com/Azure/azure-functions-core-tools)
4. [Azure Subscription](https://portal.azure.com/)
5. [Azurite Storage Emulator](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-azurite)
6. [NodeJS](https://nodejs.org/en/download/)
7. [Azure Functions Extension for VS Code - Optional](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azurefunctions)
8. [Azure Static Web Apps Extension for VS Code - Optional](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azurestaticwebapps)
9. [Azure Static Web Apps CLI - Optional](https://github.com/Azure/static-web-apps-cli)

### Clone the repository

``` git clone https://github.com/ms-app-innovation/wellbeing ```

### Setup Instructions
1. If you are using Visual Studio 2022, Open 'EDA-Samples.sln'.
2. If you are using VS Code, Open the folder.
3. To run the Azure functions locally using Functions Core tool, navigate to src\api and <br />
``` 
func start 
```
4. To run the React-App Front end locally using NodeJS and SWA CLI, navigate to src\react-app <br />
``` 
npm install 
npm start
swa start http://localhost:3000 --api-location ./api 
```


## Patterns Explored
### Temporal Coupling
#### Single App calls external correspondence service
![Temporal Coupling](https://github.com/ms-app-innovation/wellbeing/raw/main/docs/images/1.png)

#### Pros
- Simple architecture
- asdasd 
#### Cons
- Tight coupling to downstream services
- Lacking resilience 
- Dependency on downstream system can inhibit scalability


### Simple decoupling
#### Introduce a queue. Send a message.
![Decoupling using a queue](https://github.com/ms-app-innovation/wellbeing/raw/main/docs/images/2.png)

#### Pros
- Increased resiliency to downstream failures
- Can scale the correspondence for high load

#### Cons
- Additional services to manage
- Queue being down
- Potential to send the message multiple times

#### Recommendations
- Use an Azure queue
- Simple, cheap, no need for sessions, ordering, etc.
- Simple poison message handling
- Peek lock for competing consumers





