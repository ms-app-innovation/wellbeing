using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Wellbeing.API.Domain.EventSourced;

namespace Wellbeing.API.Services;

public class SerialisedEvent
{
    private readonly string _customEventType;

    [JsonProperty("id")] public string Id { get; set; }

    [JsonProperty("entityId")] public string EntityId { get; set; }

    public string CustomEventType
    {
        get => _customEventType ?? CustomEvent.GetType().FullName;
        init => _customEventType = value;
    }

    public object CustomEvent { get; set; }
    public int EventIndex { get; set; }

    public ISampleEventSourceEvent SourceEvent()
    {
        if (CustomEvent is JObject jObj)
        {
            return (ISampleEventSourceEvent)jObj.ToObject(Type.GetType(CustomEventType));
        }
        return (ISampleEventSourceEvent)CustomEvent;
    }
}