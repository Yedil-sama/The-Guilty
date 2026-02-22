using System.Collections;
using UnityEngine;

namespace TheGuilty.Core.Audio.Music
{
	public sealed class MusicAudioService : IMusicAudioService
	{
		private readonly AudioSource _audioSource;
		private float _baseVolume;

		public MusicAudioService(AudioSource audioSource)
		{
			_audioSource = audioSource;
		}

		public void Initialize()
		{
			_audioSource.playOnAwake = false;
			_baseVolume = _audioSource.volume;
		}

		public bool IsPlaying => _audioSource.isPlaying;

		public void Play(AudioMaterial material, bool loop)
		{
			if (material == null || material.Clip == null)
				return;

			_audioSource.clip = material.Clip;
			_audioSource.volume = material.Volume;
			_baseVolume = material.Volume;
			_audioSource.loop = loop;
			_audioSource.pitch = 1f;
			_audioSource.Play();
		}

		public void Stop()
		{
			_audioSource.Stop();
		}

		public IEnumerator FadeOut(float duration)
		{
			float startVolume = _audioSource.volume;
			float time = 0f;

			while (time < duration)
			{
				time += Time.deltaTime;
				_audioSource.volume = Mathf.Lerp(startVolume, 0f, time / duration);
				yield return null;
			}

			_audioSource.volume = 0f;
		}

		public IEnumerator FadeIn(float duration)
		{
			float time = 0f;

			while (time < duration)
			{
				time += Time.deltaTime;
				_audioSource.volume = Mathf.Lerp(0f, _baseVolume, time / duration);
				yield return null;
			}

			_audioSource.volume = _baseVolume;
		}
	}
}
