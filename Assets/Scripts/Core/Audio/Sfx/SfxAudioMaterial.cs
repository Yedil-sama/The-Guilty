using System;
using UnityEngine;

namespace TheGuilty.Core.Audio.Sfx
{
	[Serializable]
	public sealed class SfxAudioMaterial : AudioMaterial
	{
		[SerializeField] private float _minPitch = 1f;
		[SerializeField] private float _maxPitch = 1f;

		public float MinPitch => _minPitch;
		public float MaxPitch => _maxPitch;
	}
}
