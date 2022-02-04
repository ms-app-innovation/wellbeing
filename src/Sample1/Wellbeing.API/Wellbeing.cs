using System;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

namespace Wellbeing.API
{
    public class Wellbeing
    {
        private readonly ILogger<Wellbeing> _logger;

        public Wellbeing(ILogger<Wellbeing> log)
        {
            _logger = log;
        }

        [FunctionName("Wellbeing")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(Parameters), Description = "Parameters", Required = true)]

        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string name = data["name"];
            int score = Convert.ToInt32(data["score"]);
            string email = data["email"];
            

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : new Recommendations(name, score).Recommendation;
            

            return new OkObjectResult(responseMessage);
        }
    }

    public class Parameters
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public int Score { get; set; }
    }

    public class Recommendations
    {
        public string Recommendation { get; set; }
        public Recommendations(string name, int score)
        {
            Recommendation = score switch
            {
                0 => $"Oh no {name}! You seem to be very upset!",
                1 => $"Oh {name}! You don't sound very well!",
                2 => $"Hello {name}! You seem to be doing fine!",
                3 => $"Hi {name}! You are feeling great!",
                _ => $"Hey {name}! You are feeling on top of the world!!",
            };
        }
    }

}

