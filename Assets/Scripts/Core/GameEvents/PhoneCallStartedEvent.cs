namespace TheGuilty.Core.GameEvents
{
	public readonly struct PhoneCallStartedEvent
	{
		public readonly string CallId;

		public PhoneCallStartedEvent(string callId)
		{
			CallId = callId;
		}
	}
}