using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents.Client;
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
/// This version uses an Outbox pattern, and a dispatcher, to handle the call to the Correspondence Service
/// </summary>
public class Wellbeingv3
{
    private readonly ILogger<Wellbeingv3> _logger;


    public Wellbeingv3(ILogger<Wellbeingv3> log)
    {
        _logger = log;
    }


    [FunctionName("Wellbeing-v2")]
    [OpenApiOperation("Run", new[] { "name" })]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
    [OpenApiRequestBody("application/json", typeof(Parameters), Description = "Parameters", Required = true)]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "text/plain", typeof(string), Description = "The OK response")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
        HttpRequest req,
        [CosmosDB("wellbeing-db", "wellbeing", ConnectionStringSetting = "CosmosDBConnectionString")]
        DocumentClient cosmosClient)
    {
        _logger.LogInformation("Wellbeing API has been called.");

        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        dynamic data = JsonConvert.DeserializeObject(requestBody);
        string name = data["name"];
        int score = Convert.ToInt32(data["score"]);
        string email = data["email"];

        var responseMessage = string.IsNullOrEmpty(email)
            ? "Invalid Inputs."
            : new RecommendationProvider(name, score).Recommendation;

        await DataService.SaveStateAsync(cosmosClient, name, score, email, responseMessage);

        _logger.LogInformation("Wellbeing API has been processed the recommendation using the Outbox pattern");

        return new JsonResult(responseMessage);
    }
}