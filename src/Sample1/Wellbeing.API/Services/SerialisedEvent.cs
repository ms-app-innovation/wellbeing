using Newtonsoft.Json;

namespace Wellbeing.API.Services;

public class SerialisedEvent
{
    private object _customEvent;

    [JsonProperty("id")] public int Id { get; set; }

    [JsonProperty("entityId")] public string EntityId { get; set; }

    public string CustomEventType { get; set; }

    public object CustomEvent
    {
        get => _customEvent;
        set
        {
            _customEvent = value;
            CustomEventType = value.GetType().FullName;
        }
    }
}