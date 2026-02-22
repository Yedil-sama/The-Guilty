using System;
using UnityEngine;

namespace TheGuilty.Core.Audio
{
	[Serializable]
	public class AudioMaterial
	{
		[SerializeField] private AudioClip _clip;
		[SerializeField, Range(0f, 1f)] private float _volume = 1f;

		public AudioClip Clip => _clip;
		public float Volume => _volume;

		public AudioMaterial(AudioClip clip, float volume)
		{
			_clip = clip;
			_volume = volume;
		}
	}
}
