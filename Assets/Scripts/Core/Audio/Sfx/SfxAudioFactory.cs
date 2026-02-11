using UnityEngine;

namespace TheGuilty.Core.Audio.Sfx
{
	public sealed class SfxAudioFactory
	{
		private readonly AudioSource _prefab;
		private readonly Transform _parent;

		public SfxAudioFactory(AudioSource prefab, Transform parent = null)
		{
			_prefab = prefab;
			_parent = parent;
		}

		public AudioSource Create()
		{
			AudioSource source = Object.Instantiate(_prefab, _parent);
			source.playOnAwake = false;
			source.loop = false;
			return source;
		}
	}
}
