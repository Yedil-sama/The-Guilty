using System.Collections;
using UnityEngine;

namespace TheGuilty.Core.Directors
{
	public abstract class Narrative : INarrative
	{
		protected bool _isRunning = false;
		protected bool _isComplete = false;

		protected MonoBehaviour _coroutineHost;

		protected Narrative(MonoBehaviour coroutineHost)
		{
			_coroutineHost = coroutineHost;
		}

		protected Coroutine StartRoutine(IEnumerator routine)
		{
			if (_coroutineHost == null)
			{
				Debug.LogError($"[Narrative] Coroutine host is null in {GetType().Name}");
				return null;
			}

			return _coroutineHost.StartCoroutine(routine);
		}

		public virtual void Start()
		{
			Debug.Log($"[Narrative] {GetType().Name} started");
			_isRunning = true;
			_isComplete = false;
		}

		public abstract void Update();

		public virtual void End()
		{
			Debug.Log($"[Narrative] {GetType().Name} ended");
			_isRunning = false;
		}

		public bool IsComplete => _isComplete;
	}
}