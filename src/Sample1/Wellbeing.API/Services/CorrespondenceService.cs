using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Wellbeing.API.Services;

public class CorrespondenceService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CorrespondenceService> _logger;

    public CorrespondenceService(HttpClient httpClient, ILogger<CorrespondenceService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task CorrespondAsync(
        string emailId, 
        string responseMessage, 
        string messagingType = "Unknown Type", 
        string correlationId = null,
        string targetSystem = null)
    {
        var emailObject = new
        {
            EmailAddress = emailId,
            EmailSubject = "Your Wellbeing",
            EmailMessage = $@"Sent using {messagingType}<br>
-----------------------<br>
{responseMessage}",
            CorrelationId = correlationId,
            TargetSystem = targetSystem
        };
        var content = new StringContent(JsonConvert.SerializeObject(emailObject), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(GetEnvironmentVariable("EmailServiceUrl"), content);
        _logger.LogInformation("Corresponded to {Employee}", emailId);
        if (response.StatusCode != HttpStatusCode.Accepted) throw new Exception("Unable to send email");
    }

    private static string GetEnvironmentVariable(string name)
    {
        return Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
    }
}