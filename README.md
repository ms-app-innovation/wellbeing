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


### Outbox Pattern
#### Use the transactional outbox pattern.
![Outbox pattern](https://github.com/ms-app-innovation/wellbeing/raw/main/docs/images/3.png)

#### Pros
- Removes distributed transaction
- Improves SLA of customer interaction

#### Cons
- More complex to build and operate

#### Recommendations
- If it’s vital to transactionally send messages, and to keep a high level of service for the end user, then this pattern is a great solution.



### Broadcast events
#### Broadcast events from the Wellbeing app.
![Broadcast events](https://github.com/ms-app-innovation/wellbeing/raw/main/docs/images/4.png)

#### Pros
- Decouples Wellbeing app from downstream consumers

#### Cons
- Lacks 'Statement of overall system behavior'

#### Recommendations
- Event Grid / Service Bus Topic



### Event-carried state transfer
#### Event-carried State Transfer vs thin events.
![Event state](https://github.com/ms-app-innovation/wellbeing/raw/main/docs/images/5.png)

#### Pros
- Reduces # of recievers who need to call the producer for data

#### Cons
- Need to deal Eventual Consistency
- Increases the size of data sent from the application

#### Recommendations
- Think about Event-Grid Schema vs Cloud-Event Schema


### Saga: Process Manager
#### Choice 1: Process Manager using Durable Functions.
![Process Manager](https://github.com/ms-app-innovation/wellbeing/raw/main/docs/images/6.png)

#### Pros
- Can model complex business processes
- Can version for new / roll out canaries
#### Cons
- Difficult to version in-process workflows
- Boiler plate code hides logic
- Business logic can creep in (ESB effect)
- Grows quickly in complexity
#### Recommendation
- Great for pipelines within a service


### Saga: Choreography
#### Choice 2: Switch to a choreography approach.
![Choreography](https://github.com/ms-app-innovation/wellbeing/raw/main/docs/images/7.png)

#### Pros
- Complex behaviour arises through events without central coordination
#### Cons
- Difficult to version long running processes
- Our scenario required emails to be sent. We can’t know for sure if they were
- Complex processes become difficult to follow and report on
- Complexity increases when actions involve more and more events
#### Recommendation
- Great when the business processes being driven are unrelated


### Routing Slip Pattern
#### Choice 3: Routing Slip.
![Routing Slip](https://github.com/ms-app-innovation/wellbeing/raw/main/docs/images/8.png)

#### Pros
- Simple to implement
- Requires no overarching orchestration
#### Cons
- Difficult to version long running in-progress workflows
#### Recommendation
- Great for sequential business processes where you may need to compensate.


### Third Party Solutions
#### Choice 4: Third party such as NServiceBus.
![NServiceBus](https://github.com/ms-app-innovation/wellbeing/raw/main/docs/images/9.png)

#### Pros
Can model complex business processes
Easy to version / roll out canaries
Handles poison messages
Message de-duplication at service bus
#### Cons
Tendency for business logic to sneak into the process manager
#### Recommendation
Use for complex business processes
Buy, don’t build



### CQRS
#### Option 1: “As simple as possible, but no simpler”.
![CQRS](https://github.com/ms-app-innovation/wellbeing/raw/main/docs/images/10.png)

#### Pros
- Very simple
- Can achieve massive scale by scaling out reads to replicas
#### Cons
- _May_ hit a natural scale limit
- Some reports may become very complicated to generate from the model used to store the data
#### Recommendation
- Great if it fits your needs
- Be wary of querying a NOSQL Database as-if it was SQL


### Event Sourcing
#### Option 2: use event sourcing and pre-build the report.
![Eventsourcing](https://github.com/ms-app-innovation/wellbeing/raw/main/docs/images/11.png)

#### Pros
- Elegant model
#### Cons
- More complex to build
- More complex to operate
- Hotfixes are complicated
#### Recommendation
- Great with domains that are event based (e.g., financial transactions, share price changes)
- Use an abstraction like NEventStore
- Don’t let external services couple to your internal event sourcing events. 



## Considerations
![Considerations](https://github.com/ms-app-innovation/wellbeing/raw/main/docs/images/12.png)