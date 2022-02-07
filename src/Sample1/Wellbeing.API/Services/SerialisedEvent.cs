using Newtonsoft.Json;

namespace Wellbeing.API.Services;

public class SerialisedEvent
{
    private string _customEventType;

    [JsonProperty("id")] public string Id { get; set; }

    [JsonProperty("entityId")] public string EntityId { get; set; }

    public string CustomEventType
    {
        get => _customEventType ?? CustomEvent.GetType().FullName;
        init => _customEventType = value;
    }

    public object CustomEvent { get; set; }
}