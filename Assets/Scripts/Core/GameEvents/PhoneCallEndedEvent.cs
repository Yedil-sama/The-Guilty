namespace TheGuilty.Core.GameEvents
{
	public readonly struct PhoneCallEndedEvent
	{
		public readonly string CallId;

		public PhoneCallEndedEvent(string callId)
		{
			CallId = callId;
		}
	}
}