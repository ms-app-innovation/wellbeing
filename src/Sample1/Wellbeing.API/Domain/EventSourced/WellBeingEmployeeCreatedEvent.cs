namespace Wellbeing.API.Domain.EventSourced;

public class WellBeingEmployeeCreatedEvent : ISampleEventSourceEvent
{
    public string EmailAddress { get; set; }
}