using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        var containerResponse = await database.CreateContainerIfNotExistsAsync($"{typeof(T).Name}-events", "/entityId");

        var allEvents = new List<ISampleEventSourceEvent>();

        var events = containerResponse.Container.GetItemLinqQueryable<SerialisedEvent>(
            true,
            requestOptions: new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(id)
            });

        foreach (var evt in events)
        {
            //This code is not production grade! Just getting a sample up and running.
            var customEventObject = (JObject)evt.CustomEvent;
            var eventObject = (ISampleEventSourceEvent)customEventObject.ToObject(Type.GetType(evt.CustomEventType));
            allEvents.Add(eventObject);
        }

        if (allEvents.Any())
        {
            return EventSourcedDomainObject.Create<T>(allEvents);
        }

        return null;
    }

    public async Task Save(T entity)
    {
        var raisedEvents = entity.RaisedEvents;
        if (raisedEvents.Any())
        {
            var database = _cosmosClient.GetDatabase("wellbeing-db");
            var containerResponse = await database.CreateContainerIfNotExistsAsync($"{typeof(T).Name}-events", "/entityId");
            var transaction = containerResponse.Container.CreateTransactionalBatch(new PartitionKey(entity.EntityId));
            foreach (var raisedEvent in raisedEvents)
            {
                transaction = transaction.CreateItem(raisedEvent);
            }
            var result = await transaction.ExecuteAsync();
            if (!result.IsSuccessStatusCode)
                throw new InvalidOperationException(result.ErrorMessage);
        }
    }
}