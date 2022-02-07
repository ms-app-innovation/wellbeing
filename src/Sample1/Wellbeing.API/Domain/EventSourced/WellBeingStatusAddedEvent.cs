namespace Wellbeing.API.Domain.EventSourced;

public class WellBeingStatusAddedEvent : ISampleEventSourceEvent
{
    public int Rating { get; set; }
    public string Recommendation { get; set; }
}