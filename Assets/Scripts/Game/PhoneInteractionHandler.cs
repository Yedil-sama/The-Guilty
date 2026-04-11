using TheGuilty.Core;
using TheGuilty.Core.GameEvents;
using UHFPS.Runtime;
using UnityEngine;

namespace TheGuilty.Game
{
	public class PhoneInteractionHandler : MonoBehaviour
	{
		[SerializeField] private string _callId = "IntroCall";

		private GameEventBus _eventBus;

		private void Start()
		{
			_eventBus = ServiceLocator.Get<GameEventBus>();

			InteractableItem interactableItem = GetComponent<InteractableItem>();
			if (interactableItem != null)
			{
				interactableItem.OnExamineStartEvent.AddListener(OnPhoneExamined);
				interactableItem.OnExamineEndEvent.AddListener(OnPhoneExaminationEnded);
			}
			else
			{
				Debug.LogError("[PhoneInteractionHandler] No InteractableItem found on phone!");
			}
		}

		private void OnPhoneExamined()
		{
			_eventBus.Publish(new PhoneCallStartedEvent(_callId));
		}

		private void OnPhoneExaminationEnded()
		{
			_eventBus.Publish(new PhoneCallEndedEvent(_callId));
		}
	}
}