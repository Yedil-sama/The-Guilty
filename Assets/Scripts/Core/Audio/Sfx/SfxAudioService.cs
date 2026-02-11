using System.Threading.Tasks;
using UnityEngine;

namespace TheGuilty.Core.Audio.Sfx
{
	public sealed class SfxAudioService : ISfxAudioService
	{
		private readonly SfxAudioPool _pool;

		public SfxAudioService(AudioSource sfxAudioSourcePrefab, Transform parent = null)
		{
			SfxAudioFactory factory = new SfxAudioFactory(sfxAudioSourcePrefab, parent);
			_pool = new SfxAudioPool(factory, initialSize: 10);
		}

		public void Initialize()
		{
		}

		public void Play(SfxAudioMaterial material)
		{
			if (material == null || material.Clip == null) return;

			AudioSource source = _pool.Get();

			source.clip = material.Clip;
			source.volume = material.Volume;
			source.pitch = Random.Range(material.MinPitch, material.MaxPitch);

			source.Play();

			_ = ReleaseAfterPlay(source, material.Clip.length);
		}

		private async Task ReleaseAfterPlay(AudioSource source, float delay)
		{
			await Task.Delay((int)(delay * 1000));
			_pool.Release(source);
		}
	}
}
