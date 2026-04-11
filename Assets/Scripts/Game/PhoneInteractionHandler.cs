using TheGuilty.Core;
using TheGuilty.Core.GameEvents;
using UHFPS.Runtime;
using UnityEngine;

namespace TheGuilty.Game
{
	public class PhoneInteractionHandler : MonoBehaviour
	{
		private GameEventBus _eventBus;

		private void Start()
		{
			_eventBus = TheGuilty.Core.ServiceLocator.Get<GameEventBus>();
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
			Debug.Log("[PhoneInteractionHandler] Phone examined, publishing PhoneCallStartedEvent");
			_eventBus.Publish(new PhoneCallStartedEvent());
		}

		private void OnPhoneExaminationEnded()
		{
			Debug.Log("[PhoneInteractionHandler] Phone examination ended, publishing PhoneCallEndedEvent");
			_eventBus.Publish(new PhoneCallEndedEvent());
		}
	}
}