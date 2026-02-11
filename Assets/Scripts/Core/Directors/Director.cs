namespace TheGuilty.Core.Directors
{
	public abstract class Director : IService
	{
		protected GameEventBus EventBus;

		public void Initialize()
		{
			EventBus = ServiceLocator.Get<GameEventBus>();
			OnInitialize();
		}

		protected abstract void OnInitialize();
	}
}
