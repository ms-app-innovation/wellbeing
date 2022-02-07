using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using Wellbeing.API.Services;

namespace Wellbeing.API.Domain.EventSourced;

public abstract class EventSourcedDomainObject
{
    private readonly List<SerialisedEvent> _raisedEvents;
    protected bool Replaying { get; private set; }

    protected EventSourcedDomainObject()
    {
        _raisedEvents = new List<SerialisedEvent>();
    }

    public void RaiseEvent(ISampleEventSourceEvent @event)
    {
        _raisedEvents.Add(new SerialisedEvent()
        {
            Id = _eventCount,
            CustomEvent = @event,
            EntityId = EntityId,
            CustomEventType = @event.GetType().FullName
        });
        Dispatch(@event);
    }

    private bool _dispatching;
    private int _eventCount;

    private void Dispatch(ISampleEventSourceEvent evt)
    {
        if (_dispatching)
            throw new InvalidOperationException(
                $"Could not find method to dispatch {evt.GetType().Name} to on type {GetType().Name}");
        _dispatching = true;
        try
        {
            Dispatch((dynamic)evt);
            _eventCount++;
        }
        finally
        {
            _dispatching = false;
        }
    }

    public IReadOnlyList<SerialisedEvent> RaisedEvents => _raisedEvents.ToImmutableArray();

    public static T Create<T>(List<ISampleEventSourceEvent> events) where T : EventSourcedDomainObject
    {
        var entity = (T)Activator.CreateInstance(typeof(T), BindingFlags.NonPublic, events);
        entity.Replay(events);
        return entity;
    }

    public abstract string EntityId { get; }

    private void Replay(List<ISampleEventSourceEvent> events)
    {
        try
        {
            Replaying = true;
            foreach (var @evt in events)
            {
                Dispatch(evt);
            }
        }
        finally
        {
            {
                Replaying = false;
            }
        }
    }
}