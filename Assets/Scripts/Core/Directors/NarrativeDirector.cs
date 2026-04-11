using System.Collections.Generic;
using TheGuilty.Core.GameEvents;
using TheGuilty.Game;
using UHFPS.Runtime;
using UnityEngine;

using Debug = UnityEngine.Debug;

namespace TheGuilty.Core.Directors
{
	public sealed class NarrativeDirector : Director
	{
		private readonly MonoBehaviour _coroutineHost;
		private List<Narrative> _narratives = new List<Narrative>();
		private int _currentNarrativeIndex = -1;
		private Narrative _currentNarrative;

		public NarrativeDirector(MonoBehaviour coroutineHost)
		{
			_coroutineHost = coroutineHost;
		}

		protected override void OnInitialize()
		{
			Debug.Log("NarrativeDirector: Initializing narratives...");
			InitializeNarratives();
			EventBus.Subscribe<GameStartedEvent>(OnGameStarted);
		}

		private void InitializeNarratives()
		{
			_narratives.Clear();

			// Get references from SceneObjectProviderService
			SceneObjectProviderService sceneProvider = ServiceLocator.Get<SceneObjectProviderService>();

			Mannequin mannequin = sceneProvider.Mannequin;
			Transform phoneTransform = sceneProvider.Phone;
			EventMannequinTransformHolder transformHolder = sceneProvider.TransformHolder;
			PlayerManager player = sceneProvider.Player;

			if (mannequin == null || phoneTransform == null || transformHolder == null || player == null)
			{
				Debug.LogError("NarrativeDirector: Missing scene references! Mannequin=" + (mannequin != null) + " Phone=" + (phoneTransform != null) + " TransformHolder=" + (transformHolder != null) + " Player=" + (player != null));
				return;
			}

			// Load phone ring audio clip
			AudioClip phoneRingClip = Resources.Load<AudioClip>("Audio/Sfx/PhoneRing");
			if (phoneRingClip == null)
			{
				Debug.Log("NarrativeDirector: PhoneRing audio clip not found at Resources/Audio/Sfx/PhoneRing");
				return;
			}

			// Load voiceline audio clip
			AudioClip voicelineClip = Resources.Load<AudioClip>("Audio/Voice/ItsMe");
			if (voicelineClip == null)
			{
				Debug.Log("NarrativeDirector: Voiceline audio clip not found at Resources/Audio/Voice/ItsMe");
				return;
			}

			// Get player camera from scene provider
			Camera playerCamera = sceneProvider.PlayerCamera;
			if (playerCamera == null)
			{
				Debug.LogError("NarrativeDirector: Player camera not found!");
				return;
			}

			// Get event bus
			GameEventBus eventBus = ServiceLocator.Get<GameEventBus>();

			// Create narratives with their dependencies
			_narratives.Add(new MannequinPhoneCallNarrative(
				mannequin: mannequin,
				phonePosition: phoneTransform.position,
				phoneRingClip: phoneRingClip,
				voicelineClip: voicelineClip,
				playerCamera: playerCamera,
				transformHolder: transformHolder,
				eventBus: eventBus,
				phoneRingVolume: 0.8f
			));

			// Add more narratives here as needed
		}

		private void OnGameStarted(GameStartedEvent _)
		{
			StartNextNarrative();
		}

		private void StartNextNarrative()
		{
			Debug.Log("[NarrativeDirector]: started next narrative");
			_currentNarrativeIndex++;

			if (_narratives.Count == 0)
			{
				Debug.LogError("NarrativeDirector: Narrative list is empty. No narrative will start.");
				return;
			}

			if (_currentNarrativeIndex >= _narratives.Count)
			{
				Debug.Log("NarrativeDirector: No more narratives found, gg?");
				return;
			}

			Debug.Log("NarrativeDirector: Starting narrative index " + _currentNarrativeIndex);

			_currentNarrative = _narratives[_currentNarrativeIndex];
			_currentNarrative.Start();
			_coroutineHost.StartCoroutine(RunNarrativeCoroutine());
		}

		private System.Collections.IEnumerator RunNarrativeCoroutine()
		{
			while (_currentNarrative != null && !_currentNarrative.IsComplete)
			{
				_currentNarrative.Update();
				yield return null;
			}

			if (_currentNarrative != null)
			{
				_currentNarrative.End();
			}

			StartNextNarrative();
		}

		public void StartCall(string callId)
		{
			AudioDirector audioDirector = ServiceLocator.Get<AudioDirector>();
			audioDirector.PlayPhoneCall(callId);
		}
	}
}
