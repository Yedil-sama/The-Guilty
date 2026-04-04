using TheGuilty.Core.Directors;
using TheGuilty.Core.GameEvents;
using UnityEngine;
using UnityEngine.Rendering;

namespace TheGuilty.Core.VisualEffects
{
	public sealed class VisualEffectsDirector : Director
	{
		private readonly MonoBehaviour _coroutineHost;
		private readonly Volume _postProcessVolume;
		private readonly Camera _playerCamera;

		private WakeUpEffectController _wakeUpEffect;

		public VisualEffectsDirector(MonoBehaviour coroutineHost, Volume postProcessVolume, Camera playerCamera)
		{
			_coroutineHost = coroutineHost;
			_postProcessVolume = postProcessVolume;
			_playerCamera = playerCamera;
		}

		protected override void OnInitialize()
		{
			if (_postProcessVolume == null || _playerCamera == null)
			{
				Debug.LogWarning("VisualEffectsDirector: Post-process volume or player camera not assigned. Wake-up effect will not play.");
				return;
			}

			CreateWakeUpEffect();

			EventBus.Subscribe<GameStartedEvent>(OnGameStarted);
		}

		private void CreateWakeUpEffect()
		{
			GameObject effectObject = new GameObject("WakeUpEffectController");
			effectObject.transform.SetParent(_coroutineHost.transform);

			_wakeUpEffect = effectObject.AddComponent<WakeUpEffectController>();
			_wakeUpEffect.Initialize(_postProcessVolume, _playerCamera);
		}

		private void OnGameStarted(GameStartedEvent _)
		{
			if (_wakeUpEffect != null)
			{
				Debug.Log("VisualEffectsDirector: Starting wake-up effect...");
				_wakeUpEffect.Play();
			}
		}

		public void PlayWakeUpEffect()
		{
			if (_wakeUpEffect != null && !_wakeUpEffect.IsPlaying)
			{
				_wakeUpEffect.Play();
			}
		}
	}
}
