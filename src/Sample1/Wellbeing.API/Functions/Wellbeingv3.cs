using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Wellbeing.API.Providers;
using Wellbeing.API.Services;

namespace Wellbeing.API.Functions;

/// <summary>
///     This version uses an Outbox pattern, and a dispatcher, to handle the call to the Correspondence Service
/// </summary>
public class Wellbeingv3
{
    private readonly ILogger<Wellbeingv3> _logger;
    private readonly CorrespondenceService _correspondenceService;

    public Wellbeingv3(ILogger<Wellbeingv3> log, CorrespondenceService correspondenceService)
    {
        _logger = log;
        _correspondenceService = correspondenceService;
    }


    [FunctionName("Wellbeing-v3")]
    [OpenApiOperation("Run", "name")]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
    [OpenApiRequestBody("application/json", typeof(Parameters), Description = "Parameters", Required = true)]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "text/plain", typeof(string), Description = "The OK response")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
        HttpRequest req,
        [CosmosDB("wellbeing-db", "recommendation", Connection = "CosmosDBConnectionString")]
        CosmosClient cosmosClient)
    {
        _logger.LogInformation("Wellbeing API has been called");

        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        dynamic data = JsonConvert.DeserializeObject(requestBody);
        string name = data["name"];
        int score = Convert.ToInt32(data["score"]);
        string email = data["email"];

        var responseMessage = string.IsNullOrEmpty(email)
            ? "Invalid Inputs."
            : new RecommendationProvider(name, score).Recommendation;

        await DataService.SaveStateAsync(
            cosmosClient,
            new WellBeingStatus
            {
                Name = name,
                Score = score,
                Email = email,
                Recommendation = responseMessage,
                Outbox = new []
                {
                    new OutgoingMessage()
                    {
                        Target = "CorrespondenceService",
                        Data = new Dictionary<string, string>
                        {
                            ["Email"] = email,
                            ["ResponseMessage"] = responseMessage
                        }
                    }
                }
            });

        _logger.LogInformation("Wellbeing API has been processed the recommendation using the Outbox pattern");

        return new JsonResult(responseMessage);
    }

    /// <summary>
    ///     Side effect of using Cosmos Change feed is that we will receive 2 changes for every message
    ///     The 1st is the document with the messages.
    ///     The 2nd is the document with an empty outbox.
    ///     This has RU implications
    /// </summary>
    [FunctionName("CosmosDispatcher")]
    public async Task Dispatcher(
        [CosmosDBTrigger(
            "wellbeing-db",
            "recommendation",
            Connection = "CosmosDBConnectionString",
            LeaseContainerName = "leases",
            CreateLeaseContainerIfNotExists = true)]
        IReadOnlyList<WellBeingStatus> documents,
        [CosmosDB("wellbeing-db", "recommendation", Connection = "CosmosDBConnectionString")]
        CosmosClient cosmosClient
    )
    {
        if (documents != null && documents.Count > 0)
        {
            foreach (var document in documents)
                if (document.Outbox?.Length > 0)
                {
                    foreach (var message in document.Outbox) await Dispatch(_correspondenceService, message);
                    document.Outbox = Array.Empty<OutgoingMessage>();

                    //This is at least once messaging.
                    await DataService.SaveStateAsync(cosmosClient, document);
                }
        }
    }

    private Task Dispatch(CorrespondenceService correspondenceService, OutgoingMessage message)
    {
        if (message.Target == "CorrespondenceService")
        {
            return correspondenceService.SendEmailAsync(
                message.Data["Email"],
                message.Data["ResponseMessage"]);
        }

        return Task.CompletedTask;
    }
}