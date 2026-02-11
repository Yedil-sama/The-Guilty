using UnityEngine;

namespace TheGuilty.Core.Audio.Voice
{
	public sealed class VoiceAudioService : IVoiceAudioService
	{
		private readonly AudioSource _audioSource;

		public VoiceAudioService(AudioSource audioSource)
		{
			_audioSource = audioSource;
		}

		public void Initialize()
		{
			_audioSource.loop = false;
			_audioSource.playOnAwake = false;
		}

		public void Play(AudioMaterial material)
		{
			if (material == null || material.Clip == null)
				return;

			_audioSource.clip = material.Clip;
			_audioSource.volume = material.Volume;
			_audioSource.pitch = 1f;
			_audioSource.Play();
		}

		public void Stop()
		{
			_audioSource.Stop();
		}
	}
}
