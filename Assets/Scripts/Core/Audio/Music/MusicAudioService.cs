using UnityEngine;

namespace TheGuilty.Core.Audio.Music
{
	public sealed class MusicAudioService : IMusicAudioService
	{
		private readonly AudioSource _audioSource;

		public MusicAudioService(AudioSource audioSource)
		{
			_audioSource = audioSource;
		}

		public void Initialize()
		{
			_audioSource.playOnAwake = false;
		}

		public void Play(AudioMaterial material, bool loop)
		{
			if (material == null || material.Clip == null)
				return;

			_audioSource.clip = material.Clip;
			_audioSource.volume = material.Volume;
			_audioSource.loop = loop;
			_audioSource.pitch = 1f;
			_audioSource.Play();
		}

		public void Stop()
		{
			_audioSource.Stop();
		}
	}
}
