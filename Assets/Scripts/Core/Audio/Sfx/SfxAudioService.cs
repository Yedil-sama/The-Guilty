using UnityEngine;

namespace TheGuilty.Core.Audio.Sfx
{
	public sealed class SfxAudioService : ISfxAudioService
	{
		private readonly SfxAudioPool _pool;
		private AudioSource _loopingSource;

		public SfxAudioService(AudioSource sfxAudioSourcePrefab, Transform parent = null)
		{
			var factory = new SfxAudioFactory(sfxAudioSourcePrefab, parent);
			_pool = new SfxAudioPool(factory, initialSize: 10);
		}

		public void Initialize() { }

		public void Play(SfxAudioMaterial material, bool loop)
		{
			if (material == null || material.Clip == null)
				return;

			if (loop)
			{
				if (_loopingSource != null)
					Stop();

				_loopingSource = _pool.Get();
				_loopingSource.clip = material.Clip;
				_loopingSource.volume = material.Volume;
				_loopingSource.pitch = 1f;
				_loopingSource.loop = true;
				_loopingSource.Play();
			}
			else
			{
				var source = _pool.Get();
				source.clip = material.Clip;
				source.volume = material.Volume;
				source.pitch = Random.Range(material.MinPitch, material.MaxPitch);
				source.loop = false;
				source.Play();

				source.GetComponent<MonoBehaviour>()
					.StartCoroutine(ReleaseAfterPlay(source, material.Clip.length));
			}
		}

		public void Stop()
		{
			if (_loopingSource == null)
				return;

			_loopingSource.Stop();
			_loopingSource.loop = false;
			_pool.Release(_loopingSource);
			_loopingSource = null;
		}

		private System.Collections.IEnumerator ReleaseAfterPlay(AudioSource source, float delay)
		{
			yield return new WaitForSeconds(delay);
			_pool.Release(source);
		}
	}
}
