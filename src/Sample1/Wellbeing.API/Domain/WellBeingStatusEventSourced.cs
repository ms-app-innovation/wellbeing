using System.Collections.Generic;
using Newtonsoft.Json;

namespace Wellbeing.API.Domain;

public class WellBeingStatusEventSourced
{
    private WellBeingStatusEventSourced(params ISampleEventSourceEvent[] events)
    {
        foreach (var evt in events)
        {
            Dispatch((dynamic)evt);
        }
    }

    private void Dispatch(WellBeingEmployeeCreatedEvent evt)
    {
        this.EmailAddress = evt.EmailAddress;
    }

    public string EmailAddress { get; set; }

    public static WellBeingStatusEventSourced New(string name)
    {
        
    }
    public static WellBeingStatusEventSourced Build(IEnumerable<ISampleEventSourceEvent> events) {}
}

public class WellBeingEmployeeCreatedEvent : ISampleEventSourceEvent
{
    public string EmailAddress { get; set; }
}

public interface ISampleEventSourceEvent
{
}

abstract class EventSourcedDomainObject
{
    public abstract string Id { get; }
    public List<ISampleEventSourceEvent> CollectedEvents { get; }
}

public class WellBeingStatusAddedEvent : ISampleEventSourceEvent
{
    public int Id { get; set; }
    public string Name => EmailAddress;
    public string EmailAddress { get; set; }
}
