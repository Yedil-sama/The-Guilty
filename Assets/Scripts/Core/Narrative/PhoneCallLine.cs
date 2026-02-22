using System;
using TheGuilty.Core.Audio;
using UnityEngine;

namespace TheGuilty.Core.Narrative
{
	[Serializable]
	public sealed class PhoneCallLine
	{
		[SerializeField, TextArea(5, 15)] private string _text;
		[SerializeField] private AudioMaterial _voice;

		public string Text => _text;
		public AudioMaterial Voice => _voice;
	}
}
