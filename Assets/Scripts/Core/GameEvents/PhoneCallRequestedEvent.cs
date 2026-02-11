namespace TheGuilty.Core.GameEvents
{
	public readonly struct PhoneCallRequestedEvent
	{
		public readonly string CallId;

		public PhoneCallRequestedEvent(string callId)
		{
			CallId = callId;
		}
	}
}
