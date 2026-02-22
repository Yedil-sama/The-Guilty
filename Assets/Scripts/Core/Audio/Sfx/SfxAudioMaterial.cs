using System;
using UnityEngine;

namespace TheGuilty.Core.Audio.Sfx
{
	[Serializable]
	public sealed class SfxAudioMaterial : AudioMaterial
	{
		[SerializeField, Range(0.5f, 1f)] private float _minPitch = 1f;
		[SerializeField, Range(0.5f, 2f)] private float _maxPitch = 1f;

		public float MinPitch => _minPitch;
		public float MaxPitch => _maxPitch;

		public SfxAudioMaterial(AudioClip clip, float volume, float minPitch, float maxPitch) : base(clip, volume)
		{
			_minPitch = minPitch;
			_maxPitch = maxPitch;
		}
	}
}
