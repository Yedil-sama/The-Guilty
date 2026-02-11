using System;
using System.Collections.Generic;

namespace TheGuilty.Core
{
	public sealed class GameEventBus : IService
	{
		private readonly Dictionary<Type, Delegate> _eventTable = new Dictionary<Type, Delegate>();

		public void Initialize()
		{
		}

		public void Subscribe<TEvent>(Action<TEvent> callback)
		{
			Type eventType = typeof(TEvent);

			if (_eventTable.TryGetValue(eventType, out Delegate existingDelegate))
			{
				_eventTable[eventType] = Delegate.Combine(existingDelegate, callback);
			}
			else
			{
				_eventTable.Add(eventType, callback);
			}
		}

		public void Unsubscribe<TEvent>(Action<TEvent> callback)
		{
			Type eventType = typeof(TEvent);

			if (!_eventTable.TryGetValue(eventType, out Delegate existingDelegate))
			{
				return;
			}

			Delegate currentDelegate = Delegate.Remove(existingDelegate, callback);

			if (currentDelegate == null)
			{
				_eventTable.Remove(eventType);
			}
			else
			{
				_eventTable[eventType] = currentDelegate;
			}
		}

		public void Publish<TEvent>(TEvent eventData)
		{
			Type eventType = typeof(TEvent);

			if (!_eventTable.TryGetValue(eventType, out Delegate existingDelegate))
			{
				return;
			}

			Action<TEvent> callback = existingDelegate as Action<TEvent>;
			callback?.Invoke(eventData);
		}
	}
}
