using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Wellbeing.API.Domain.EventSourced;

namespace Wellbeing.API.Services;

public class EventSourcedObjectRepository<T> where T : EventSourcedDomainObject
{
    private readonly CosmosClient _cosmosClient;

    public EventSourcedObjectRepository(CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
    }

    public async Task<T> Get(string id)
    {
        var database = _cosmosClient.GetDatabase("wellbeing-db");
        var containerResponse = await database.CreateContainerIfNotExistsAsync(nameof(T), "/entityId");

        var allEvents = new List<ISampleEventSourceEvent>();

        var events = containerResponse.Container.GetItemLinqQueryable<SerialisedEvent>(
            requestOptions: new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(id)
            });

        foreach (var evt in events)
            //This code is not production grade! Just getting a sample up and running.
            allEvents.Add((ISampleEventSourceEvent)JsonConvert.DeserializeObject(
                JsonConvert.SerializeObject(evt.CustomEvent), Type.GetType(evt.CustomEventType)));

        return EventSourcedDomainObject.Create<T>(allEvents);
    }

    public async Task Save(T entity)
    {
        var raisedEvents = entity.RaisedEvents;
        if (raisedEvents.Any())
        {
            var database = _cosmosClient.GetDatabase("wellbeing-db");
            var containerResponse = await database.CreateContainerIfNotExistsAsync(nameof(T), "/entityId");
            var transaction = containerResponse.Container.CreateTransactionalBatch(new PartitionKey(entity.EntityId));
            foreach (var raisedEvent in raisedEvents)
            {
                transaction.CreateItem(raisedEvent);
            }
            await transaction.ExecuteAsync();
        }
    }
}