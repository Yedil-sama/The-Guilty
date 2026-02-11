namespace TheGuilty.Core.GameEvents
{
	public struct TaskStartedEvent
	{
		public readonly string TaskId;

		public TaskStartedEvent(string taskId)
		{
			TaskId = taskId;
		}
	}
}