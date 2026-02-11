using UnityEngine;

namespace TheGuilty.Core.Narrative
{
	[CreateAssetMenu(menuName = "The Guilty/Narrative/PhoneCall")]
	public sealed class PhoneCallNarrativeDefinition : ScriptableObject
	{
		[SerializeField] private string _callId;
		[SerializeField] private PhoneCallLine[] _lines;

		public string CallId => _callId;
		public PhoneCallLine[] Lines => _lines;
	}
}
