using TheGuilty.Core.Audio;
using TheGuilty.Core.Audio.Music;
using TheGuilty.Core.Audio.Sfx;
using TheGuilty.Core.Audio.Voice;
using TheGuilty.Core.Directors;
using TheGuilty.Core.GameEvents;
using TheGuilty.Core.Narrative;
using UnityEngine;

namespace TheGuilty.Core
{
	public sealed class EntryPoint : MonoBehaviour
	{
		[Header("Audio Sources")]
		[SerializeField] private AudioSource _voiceAudioSource;
		[SerializeField] private AudioSource _sfxAudioSourcePrefab;
		[SerializeField] private AudioSource _musicAudioSource;

		[Header("Narratives")]
		[SerializeField] private PhoneCallNarrativeDefinition[] _phoneCallNarratives;

		private GameEventBus _eventBus;

		private IVoiceAudioService _voiceAudioService;
		private ISfxAudioService _sfxAudioService;
		private IMusicAudioService _musicAudioService;

		private NarrativeDirector _narrativeDirector;
		private TaskDirector _taskDirector;
		private GameDirector _gameDirector;

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
			_voiceAudioService = new VoiceAudioService(_voiceAudioSource);
			_sfxAudioService = new SfxAudioService(_sfxAudioSourcePrefab);
			_musicAudioService = new MusicAudioService(_musicAudioSource);

			ServiceLocator.Register<GameEventBus>(_eventBus);
			ServiceLocator.Register<IVoiceAudioService>(_voiceAudioService);
			ServiceLocator.Register<ISfxAudioService>(_sfxAudioService);
			ServiceLocator.Register<IMusicAudioService>(_musicAudioService);

			_eventBus.Initialize();
			_voiceAudioService.Initialize();
			_sfxAudioService.Initialize();
			_musicAudioService.Initialize();
		}

		private void InitializeDirectors()
		{
			_narrativeDirector = new NarrativeDirector(_voiceAudioService, _phoneCallNarratives);
			_taskDirector = new TaskDirector();
			_gameDirector = new GameDirector(_eventBus, _narrativeDirector);

			_narrativeDirector.Initialize();
			_taskDirector.Initialize();
			_gameDirector.Initialize();
		}

		private void StartGame()
		{
			_eventBus.Publish(new GameStartedEvent());
		}
	}
}
