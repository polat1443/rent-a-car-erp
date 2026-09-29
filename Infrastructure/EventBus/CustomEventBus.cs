using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AracKiralamaApp.Infrastructure.EventBus
{
    public interface IEvent { }

    public interface IIntegrationEventHandler<in TEvent> where TEvent : IEvent
    {
        Task Handle(TEvent @event);
    }

    public class CustomEventBus
    {
        private static readonly Lazy<CustomEventBus> _instance = new(() => new CustomEventBus());
        public static CustomEventBus Instance => _instance.Value;

        private readonly Dictionary<Type, List<object>> _handlers = new();

        private CustomEventBus() { }

        public void Subscribe<TEvent>(IIntegrationEventHandler<TEvent> handler) where TEvent : IEvent
        {
            var eventType = typeof(TEvent);
            if (!_handlers.ContainsKey(eventType))
            {
                _handlers[eventType] = new List<object>();
            }
            _handlers[eventType].Add(handler);
        }

        public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : IEvent
        {
            var eventType = typeof(TEvent);
            if (_handlers.TryGetValue(eventType, out var handlers))
            {
                foreach (var handler in handlers)
                {
                    if (handler is IIntegrationEventHandler<TEvent> typedHandler)
                    {
                        await typedHandler.Handle(@event);
                    }
                }
            }
        }
    }
}
