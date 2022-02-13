using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Wellbeing.API.Domain.EventSourced;
using Wellbeing.API.Domain.EventSourced.ReadModels;
using Wellbeing.API.Services;

namespace Wellbeing.API.Functions.EventSourcingReadModelPopulators;

public static class WellBeingStatusByWeekPopulator
{
    [FunctionName("WellBeingStatusByWeekPopulator")]
    public static async Task RunAsync([CosmosDBTrigger(
            databaseName: "wellbeing-db",
            containerName: "WellBeingStatusEventSourced-events",
            Connection = "CosmosDBConnectionString",
            LeaseContainerName = "leases",
            LeaseContainerPrefix = "wellbeingstatusbyweek")]
        IReadOnlyList<SerialisedEvent> input, 
        ILogger log,
        [CosmosDB(Connection = "CosmosDBConnectionString")]CosmosClient client 
        )
    {
        if (input != null && input.Count > 0)
        {
            var database = client.GetDatabase("wellbeing-db");
            var containerResponse = await database.CreateContainerIfNotExistsAsync("Reports", "/reportName");
            var container = containerResponse.Container;

            // await container.DeleteContainerAsync(); 
            // containerResponse = await database.CreateContainerIfNotExistsAsync("Reports", "/reportName");
            // container = containerResponse.Container;

            var id = $"{DateTime.Now.Year}-{ISOWeek.GetWeekOfYear(DateTime.Now)}";
            var report = await container.ReadItemAsync<AggregatedWellBeingStatuses>(
                    id,
                    new PartitionKey("WellBeingByWeek")).NoThrow();

            report ??= new AggregatedWellBeingStatuses()
            {
                Id = id,
                Year = DateTime.Now.Year,
                WeekOfYear = ISOWeek.GetWeekOfYear(DateTime.Now),
                ReportName = "WellBeingByWeek"
            };

            foreach (var serialisedEvent in input)
            {
                var sourceEvent = serialisedEvent.SourceEvent();
                if (sourceEvent is WellBeingStatusAddedEvent wellBeingStatusAddedEvent)
                {
                    
                    if (!report.Aggregations.ContainsKey(wellBeingStatusAddedEvent.Rating))
                    {
                        report.Aggregations.Add(wellBeingStatusAddedEvent.Rating, 1);
                    }
                    else
                    {
                        report.Aggregations[wellBeingStatusAddedEvent.Rating] += 1;
                    }

                }
            }
            var status = await container.UpsertItemAsync(report);
        }
    }

}