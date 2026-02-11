using TheGuilty.Core.GameEvents;
using UnityEngine;

namespace TheGuilty.Core.Directors
{
	public sealed class TaskDirector : Director
	{
		private const string FirstTaskId = "TurnOnBathWater";

		protected override void OnInitialize()
		{
			EventBus.Subscribe<PhoneCallEndedEvent>(OnPhoneCallEnded);
		}

		private void OnPhoneCallEnded(PhoneCallEndedEvent phoneCallEndedEvent)
		{
			if (phoneCallEndedEvent.CallId == "IntroCall")
			{
				StartTask("TurnOnBathWater");
			}
		}

		private void StartTask(string taskId)
		{
			Debug.Log($"Task Started: {taskId}");
			EventBus.Publish(new TaskStartedEvent(taskId));
		}
	}
}
