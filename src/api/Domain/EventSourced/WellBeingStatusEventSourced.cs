using System.Collections.Generic;

namespace Wellbeing.API.Domain.EventSourced;

public class WellBeingStatusEventSourced : EventSourcedDomainObject
{
    public WellBeingStatusEventSourced(string emailAddress)
    {
        var createEvent = new WellBeingEmployeeCreatedEvent { EmailAddress = emailAddress };
        RaiseEvent(createEvent);
    }

    // ReSharper disable once UnusedMember.Global
    protected WellBeingStatusEventSourced()
    {
        
    }

    public string Recommendation { get; set; }

    public int Score { get; set; }

    public string Email { get; set; }

    public string Name => Name;

    private void Dispatch(WellBeingEmployeeCreatedEvent evt)
    {
        Email = evt.EmailAddress;
    }

    private void Dispatch(WellBeingStatusAddedEvent evt)
    {
        Score = evt.Rating;
        Recommendation = evt.Recommendation;
    }

    public override string EntityId => Email;

    public void RecordNewWellbeingStatus(string recommendation, int rating)
    {
        //check invariants
        RaiseEvent(new WellBeingStatusAddedEvent()
        {
            Rating = rating,
            Recommendation = recommendation
        });
    }
}