using TheGuilty.Core.Audio;
using TheGuilty.Core.GameEvents;
using TheGuilty.Core.Narrative;
using UnityEngine;

namespace TheGuilty.Core.Directors
{
	public sealed class NarrativeDirector : Director
	{
		private const string IntroCallId = "IntroCall";

		private IVoiceAudioService _voiceAudioService;
		private PhoneCallNarrativeDefinition[] _narratives;

		protected override void OnInitialize()
		{
			EventBus.Subscribe<GameStartedEvent>(OnGameStarted);
		}

		public NarrativeDirector(IVoiceAudioService voiceAudioService, PhoneCallNarrativeDefinition[] narratives)
		{
			_voiceAudioService = voiceAudioService;
			_narratives = narratives;
		}

		private void OnGameStarted(GameStartedEvent gameStartedEvent)
		{
			StartCall(IntroCallId);
		}

		public void StartCall(string callId)
		{
			PhoneCallNarrativeDefinition narrative = null;

			if (_narratives != null)
			{
				foreach (var n in _narratives)
				{
					if (n.CallId == callId)
					{
						narrative = n;
						break;
					}
				}
			}

			if (narrative == null)
			{
				Debug.LogWarning($"Narrative not found: {callId}");
				return;
			}

			EventBus.Publish(new PhoneCallRequestedEvent(callId));
			EventBus.Publish(new PhoneCallStartedEvent(callId));

			foreach (var line in narrative.Lines)
			{
				Debug.Log(line.Text);

				if (line.Voice != null && _voiceAudioService != null)
				{
					_voiceAudioService.Play(line.Voice);
				}
			}

			EventBus.Publish(new PhoneCallEndedEvent(callId));
		}
	}
}
