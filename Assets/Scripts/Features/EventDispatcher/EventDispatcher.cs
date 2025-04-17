using EventBus;

namespace Features.EventDispatcher
{
    public class EventsDispatcher 
    {
        public interface IGameEvent : IDispatchableEvent { }
        public Dispatcher<IGameEvent> GameDispatcher { get; } = new("GameDispatcher");
    }
}

