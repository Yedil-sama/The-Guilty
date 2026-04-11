using UnityEngine;

namespace TheGuilty.Core.Directors
{
	public abstract class Narrative : INarrative
	{
		protected bool _isRunning = false;
		protected bool _isComplete = false;

		public virtual void Start()
		{
			Debug.Log($"[Narrative] {GetType().Name} started: {_isRunning}");
			_isRunning = true;
			_isComplete = false;
		}

		public abstract void Update();

		public virtual void End()
		{
			Debug.Log($"[Narrative] {GetType().Name} ended: {_isComplete}");
			_isRunning = false;
		}

		public bool IsComplete => _isComplete;
	}
}