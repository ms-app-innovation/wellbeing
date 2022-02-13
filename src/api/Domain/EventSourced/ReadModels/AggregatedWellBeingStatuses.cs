using System.Collections.Generic;
using Newtonsoft.Json;

namespace Wellbeing.API.Domain.EventSourced.ReadModels;

public class AggregatedWellBeingStatuses
{
    [JsonProperty("id")]public string Id { get; set; }
    [JsonProperty("reportName")]public string ReportName { get; set; }
    public int WeekOfYear { get; set; }
    public int Year { get; set; }
    public Dictionary<int, int> Aggregations { get; set; } = new Dictionary<int, int>();
}