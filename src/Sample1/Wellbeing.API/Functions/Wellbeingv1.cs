using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
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

public class Wellbeingv1
{
    private readonly ILogger<Wellbeingv1> _logger;
    private readonly CorrespondenceService _correspondenceService;


    public Wellbeingv1(ILogger<Wellbeingv1> log, CorrespondenceService correspondenceService)
    {
        _logger = log;
        _correspondenceService = correspondenceService;
    }


    [FunctionName("Wellbeing-v1")]
    [OpenApiOperation("Run", "name")]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
    [OpenApiRequestBody("application/json", typeof(Parameters), Description = "Parameters", Required = true)]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "text/plain", typeof(string), Description = "The OK response")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
        [CosmosDB("wellbeing-db", "recommendation", Connection = "CosmosDBConnectionString")] CosmosClient cosmosClient)
    {
        _logger.LogInformation("Wellbeing API has been called");

        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        dynamic data = JsonConvert.DeserializeObject(requestBody);
        string name = data["name"];
        int score = Convert.ToInt32(data["score"]);
        string email = data["email"];

        var responseMessage = string.IsNullOrEmpty(email) ? "Invalid Inputs.": new RecommendationProvider(name, score).Recommendation;

        await _correspondenceService.SendEmailAsync(email, responseMessage);

        await DataService.SaveStateAsync(cosmosClient, new WellBeingStatus
        {
            Name = name,
            Score = score,
            Email = email,
            Recommendation = responseMessage
        });


        _logger.LogInformation("Wellbeing API has been processed the recommendation.");

        return new JsonResult(responseMessage);
    }
}