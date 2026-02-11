using System.Collections.Generic;
using UnityEngine;

namespace TheGuilty.Core.Audio.Sfx
{
	public sealed class SfxAudioPool
	{
		private readonly SfxAudioFactory _factory;
		private readonly Queue<AudioSource> _available = new Queue<AudioSource>();

		public SfxAudioPool(SfxAudioFactory factory, int initialSize = 5)
		{
			_factory = factory;

			for (int i = 0; i < initialSize; i++)
			{
				_available.Enqueue(_factory.Create());
			}
		}

		public AudioSource Get()
		{
			if (_available.Count > 0)
			{
				return _available.Dequeue();
			}
			return _factory.Create();
		}

		public void Release(AudioSource source)
		{
			source.Stop();
			source.clip = null;
			_available.Enqueue(source);
		}
	}
}
