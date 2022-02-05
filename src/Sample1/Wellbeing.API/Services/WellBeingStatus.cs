using System;
using System.Collections.Generic;
using Microsoft.Azure.Documents;
using Newtonsoft.Json;

namespace Wellbeing.API.Services;

public class WellBeingStatus
{
    [JsonProperty("id")]
    public string Id => Email;

    [JsonProperty("_etag")]
    public string ETag { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }
    public string Email { get; set; }
    public int Score { get; set; }
    public string Recommendation { get; set; }
    public List<OutgoingMessage> Outbox { get; set; }

    public void RecordNewWellbeingStatus(string responseMessage, int score, params OutgoingMessage[] outboxMessages)
    {
        Recommendation = responseMessage;
        Score = score;
        Outbox ??= new List<OutgoingMessage>();
        Outbox.AddRange(outboxMessages);
    }
}