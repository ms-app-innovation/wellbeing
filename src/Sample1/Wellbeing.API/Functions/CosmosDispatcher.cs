using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Wellbeing.API.Domain;
using Wellbeing.API.Services;

namespace Wellbeing.API.Functions;

public class CosmosDispatcher
{
    private readonly CorrespondenceService _correspondenceService;
    private readonly ILogger<CosmosDispatcher> _logger;

    public CosmosDispatcher(ILogger<CosmosDispatcher> logger, CorrespondenceService correspondenceService)
    {
        _logger = logger;
        _correspondenceService = correspondenceService;
    }

    /// <summary>
    ///     Side effect of using Cosmos Change feed is that we will receive 2 changes for every message
    ///     The 1st is the document with the messages.
    ///     The 2nd is the document with an empty outbox.
    ///     This has RU implications
    /// </summary>
    /// <remarks>
    ///     https://docs.microsoft.com/en-us/azure/cosmos-db/sql/change-feed-pull-model#using-feedrange-for-parallelization
    ///     Currently you get one feed-range for a physical partition. If you had multiple partitions and one didn't have much
    ///     throughput,
    ///     you would have a Kafka style scenario where one function instances would sit not doing much, whilst the other did
    ///     all the work.
    ///     Contrast that to a competing-consumer pattern like you'd get with ServiceBus where load would be evenly spread
    ///     across instances.
    ///     Same page says: "transaction scope is preserved when reading items from the Change Feed. As a result, the number of
    ///     items received could be higher than the specified
    ///     I don't specify this but would be interesting to see if we push lots of load through as single transactions, if the
    ///     number of items ever goes > 1.
    /// </remarks>
    [FunctionName("CosmosDispatcher")]
    // Cosmos trigger purposefully moves on if there are user errors. In this scenario I don't want that so I use a Function attribute to retry until I process the messages.
    [ExponentialBackoffRetry(-1, "00:00:01", "00:05:00")]
    public async Task Dispatcher(
        [CosmosDBTrigger(
            "wellbeing-db",
            "recommendation",
            Connection = "CosmosDBConnectionString",
            LeaseContainerName = "leases",
            CreateLeaseContainerIfNotExists = true,
            LeaseContainerPrefix = "v3")]
        IReadOnlyList<WellBeingStatus> documents,
        [CosmosDB("wellbeing-db", "recommendation", Connection = "CosmosDBConnectionString")]
        CosmosClient cosmosClient,
        [DurableClient] IDurableOrchestrationClient durableOrchestrationClient
    )
    {
        if (documents != null && documents.Count > 0)
            foreach (var document in documents)
                while (document.Outbox?.Any() ?? false)
                {
                    var message = document.Outbox.Last();

                    await Dispatch(
                        durableOrchestrationClient,
                        _correspondenceService,
                        document,
                        message);

                    document.Outbox.Remove(message);

                    //Always update the state after successfully dispatching a message
                    await DataService.SaveStateAsync(cosmosClient, document);
                }
    }

    private async Task Dispatch(
        IDurableOrchestrationClient durableOrchestrationClient,
        CorrespondenceService correspondenceService,
        WellBeingStatus entity,
        OutgoingMessage message)
    {
        if (message.Target == "CorrespondenceService")
        {
            if (Random.Shared.NextDouble() < 0.2)
            {
                _logger.LogWarning("Introducing random failure");
                throw new ArgumentException("Introducing range exception to test function retry policy");
            }

            await correspondenceService.SendEmailAsync(
                entity.Email,
                message.Data["ResponseMessage"],
                message.Data.ContainsKey("MessageType") ? message.Data["MessageType"] : "Unknown Type"
            );
        }
        else if (message.Target == "Orchestrator")
        {
            if (message.Data.ContainsKey("OrchestratorName"))
                await durableOrchestrationClient.StartNewAsync(message.Data["OrchestratorName"], message);
            else
                _logger.LogWarning(
                    "Dispatcher received Orchestrator message but was missing OrchestratorName property inside the data bag.");
        }
    }
}