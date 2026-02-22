using System.Collections;
using TheGuilty.Core.GameEvents;
using UnityEngine;

namespace TheGuilty.Core.Directors
{
	public sealed class NarrativeDirector : Director
	{
		private const string IntroCallId = "Hey it's me";
		private const float IntroDelay = 15;

		private readonly MonoBehaviour _coroutineHost;

		private AudioDirector _audioDirector;

		public NarrativeDirector(MonoBehaviour coroutineHost)
		{
			_coroutineHost = coroutineHost;
		}

		protected override void OnInitialize()
		{
			_audioDirector = ServiceLocator.Get<AudioDirector>();

			EventBus.Subscribe<GameStartedEvent>(OnGameStarted);
		}

		private void OnGameStarted(GameStartedEvent _)
		{
			_coroutineHost.StartCoroutine(StartIntroDelayed());
		}

		private IEnumerator StartIntroDelayed()
		{
			if (IntroDelay > 0f)
				yield return new WaitForSeconds(IntroDelay);

			_audioDirector.PlayPhoneCall(IntroCallId);
		}

		public void StartCall(string callId)
		{
			_audioDirector.PlayPhoneCall(callId);
		}
	}
}
