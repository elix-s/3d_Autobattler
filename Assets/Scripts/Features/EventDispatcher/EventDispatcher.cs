using EventBus;

public class EventsDispatcher 
{
    public interface IGameEvent : IDispatchableEvent { }
    public Dispatcher<IGameEvent> GameDispatcher { get; } = new("GameDispatcher");
}
