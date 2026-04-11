using TheGuilty.Core.GameEvents;
using TheGuilty.Game;
using UnityEngine;
using Debug = UnityEngine.Debug;

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
			Debug.Log("[GameDirector] OnInitialize");
			_eventBus.Subscribe<GameStartedEvent>(OnGameStarted);
		}

		private void OnGameStarted(GameStartedEvent gameStartedEvent)
		{
			Debug.Log("GameDirector: Game started, triggering intro sequence...");
		}

		public void TriggerMannequinEvent(string eventName, IMannequinStrategy strategy)
		{
			Debug.Log("mannequinevent triggered");
			SceneObjectProviderService sceneProvider = ServiceLocator.Get<SceneObjectProviderService>();

			Mannequin mannequin = sceneProvider.Mannequin;
			EventMannequinTransformHolder transformHolder = sceneProvider.TransformHolder;

			if (mannequin == null || transformHolder == null)
			{
				Debug.LogError("GameDirector: Cannot trigger mannequin event - missing references!");
				return;
			}

			Transform position = transformHolder.GetRandomPositionForEvent(eventName);
			if (position != null)
			{
				mannequin.TeleportTo(position);
				mannequin.SetStrategy(strategy);
			}
			else
			{
				Debug.Log($"GameDirector: Event '{eventName}' has no positions defined!");
			}
		}
	}
}
