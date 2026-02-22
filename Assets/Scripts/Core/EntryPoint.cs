using TheGuilty.Core.Audio;
using TheGuilty.Core.Audio.Music;
using TheGuilty.Core.Audio.Sfx;
using TheGuilty.Core.Audio.Voice;
using TheGuilty.Core.Directors;
using TheGuilty.Core.GameEvents;
using UnityEngine;

namespace TheGuilty.Core
{
	public sealed class EntryPoint : MonoBehaviour
	{
		[Header("Audio Sources")]
		[SerializeField] private AudioSource _voiceAudioSource;
		[SerializeField] private AudioSource _sfxAudioSourcePrefab;
		[SerializeField] private AudioSource _musicAudioSource;

		private GameEventBus _eventBus;

		private IVoiceAudioService _voiceService;
		private ISfxAudioService _sfxService;
		private IMusicAudioService _musicService;

		private NarrativeDirector _narrativeDirector;
		private AudioDirector _audioDirector;

		private void Awake()
		{
			InitializeServices();
			InitializeDirectors();
			StartGame();
		}

		private void InitializeServices()
		{
			ServiceLocator.Clear();

			_eventBus = new GameEventBus();

			_voiceService = new VoiceAudioService(_voiceAudioSource);
			_sfxService = new SfxAudioService(_sfxAudioSourcePrefab, transform);
			_musicService = new MusicAudioService(_musicAudioSource);

			ServiceLocator.Register(_eventBus);
			ServiceLocator.Register(_voiceService);
			ServiceLocator.Register(_sfxService);
			ServiceLocator.Register(_musicService);

			_eventBus.Initialize();
			_voiceService.Initialize();
			_sfxService.Initialize();
			_musicService.Initialize();
		}

		private void InitializeDirectors()
		{
			_audioDirector = new AudioDirector(this);
			_narrativeDirector = new NarrativeDirector(this);

			ServiceLocator.Register(_audioDirector);
			ServiceLocator.Register(_narrativeDirector);

			_audioDirector.Initialize();
			_narrativeDirector.Initialize();
		}

		private void StartGame()
		{
			_eventBus.Publish(new GameStartedEvent());
		}
	}
}
