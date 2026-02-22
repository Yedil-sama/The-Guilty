using System.Collections;
using TheGuilty.Core.Audio;
using TheGuilty.Core.GameEvents;
using TheGuilty.Core.Narrative;
using UnityEngine;

namespace TheGuilty.Core.Directors
{
	public sealed class AudioDirector : Director
	{
		private readonly MonoBehaviour _coroutineHost;

		private IMusicAudioService _musicService;
		private IVoiceAudioService _voiceService;
		private ISfxAudioService _sfxService;

		private PhoneCallNarrativeDefinition[] _narratives;

		private AudioMaterial _backgroundMusic;

		private const float MusicStartDelay = 5f;
		private const float MusicVolume = 1f;

		private const float FadeOutTime = 3f;
		private const float FadeInTime = 5f;

		public AudioDirector(MonoBehaviour coroutineHost)
		{
			_coroutineHost = coroutineHost;
		}

		protected override void OnInitialize()
		{
			_musicService = ServiceLocator.Get<IMusicAudioService>();
			_voiceService = ServiceLocator.Get<IVoiceAudioService>();
			_sfxService = ServiceLocator.Get<ISfxAudioService>();

			LoadAssets();

			EventBus.Subscribe<GameStartedEvent>(OnGameStarted);
		}

		private void LoadAssets()
		{
			_narratives = Resources.LoadAll<PhoneCallNarrativeDefinition>("Audio/Voice");

			AudioClip musicClip = Resources.Load<AudioClip>("Audio/Music/Main Background Music");

			if (musicClip != null && !musicClip.preloadAudioData)
				musicClip.LoadAudioData(); // Preload asynchronously

			_backgroundMusic = new AudioMaterial(musicClip, MusicVolume);
		}

		private void OnGameStarted(GameStartedEvent _)
		{
			_coroutineHost.StartCoroutine(StartMusicRoutine());
		}

		private IEnumerator StartMusicRoutine()
		{
			if (MusicStartDelay > 0f)
				yield return new WaitForSeconds(MusicStartDelay);

			_musicService.Play(_backgroundMusic, true);
		}

		public void PlayPhoneCall(string callId)
		{
			var narrative = FindNarrative(callId);

			if (narrative == null)
			{
				Debug.LogWarning($"Narrative not found: {callId}");
				return;
			}

			_coroutineHost.StartCoroutine(PlayNarrativeRoutine(narrative));
		}

		private PhoneCallNarrativeDefinition FindNarrative(string callId)
		{
			foreach (var n in _narratives)
			{
				if (n.CallId == callId)
					return n;
			}

			return null;
		}

		private IEnumerator PlayNarrativeRoutine(PhoneCallNarrativeDefinition narrative)
		{
			EventBus.Publish(new PhoneCallRequestedEvent(narrative.CallId));
			EventBus.Publish(new PhoneCallStartedEvent(narrative.CallId));

			yield return _musicService.FadeOut(FadeOutTime);

			foreach (var line in narrative.Lines)
			{
				if (line.Voice == null)
					continue;

				Debug.Log(line.Text);

				_voiceService.Play(line.Voice);

				while (_voiceService.IsPlaying)
					yield return null;
			}

			yield return _musicService.FadeIn(FadeInTime);

			EventBus.Publish(new PhoneCallEndedEvent(narrative.CallId));
		}
	}
}
