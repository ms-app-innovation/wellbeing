using Microsoft.Azure.Documents;
using Newtonsoft.Json;

namespace Wellbeing.API.Services;

public class WellBeingStatus
{
    [JsonProperty("id")]
    public string Id => Email;
    public string Name { get; set; }
    public string Email { get; set; }
    public int Score { get; set; }
    public string Recommendation { get; set; }
    public OutgoingMessage[] Outbox { get; set; }
}