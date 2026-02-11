using System;
using System.Collections.Generic;

namespace TheGuilty.Core
{
	public sealed class ServiceLocator
	{
		private static readonly Dictionary<Type, IService> _services = new Dictionary<Type, IService>();

		public static void Register<TService>(TService service) where TService : class, IService
		{
			Type serviceType = typeof(TService);

			if (_services.ContainsKey(serviceType))
			{
				throw new InvalidOperationException($"Service of type {serviceType.Name} is already registered.");
			}

			_services.Add(serviceType, service);
		}

		public static TService Get<TService>() where TService : class, IService
		{
			Type serviceType = typeof(TService);

			if (!_services.TryGetValue(serviceType, out IService service))
			{
				throw new InvalidOperationException($"Service of type {serviceType.Name} is not registered.");
			}

			return service as TService;
		}

		public static void Clear()
		{
			_services.Clear();
		}
	}
}
