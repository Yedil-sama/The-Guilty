using TheGuilty.Core.GameEvents;
using UnityEngine;

namespace TheGuilty.Core.Directors
{
	public sealed class GameDirector : Director
	{
		private readonly GameEventBus _eventBus;
		private readonly NarrativeDirector _narrativeDirector;

		public GameDirector(GameEventBus eventBus, NarrativeDirector narrativeDirector)
		{
			_eventBus = eventBus;
			_narrativeDirector = narrativeDirector;
		}

		protected override void OnInitialize()
		{
			_eventBus.Subscribe<GameStartedEvent>(OnGameStarted);
		}

		private void OnGameStarted(GameStartedEvent gameStartedEvent)
		{
			Debug.Log("GameDirector: Game started, triggering intro sequence...");

			_narrativeDirector.StartCall("IntroCall");
		}
	}
}
