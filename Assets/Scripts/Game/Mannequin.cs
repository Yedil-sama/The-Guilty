using UnityEngine;
using UnityEngine.AI;

namespace TheGuilty.Game
{
	public class Mannequin : MonoBehaviour
	{
		[Header("Components")]
		[SerializeField] private Animator _animator;
		[SerializeField] private BoxCollider _hitbox;
		[SerializeField] private NavMeshAgent _navMeshAgent;

		[Header("Movement")]
		[SerializeField] private float _walkSpeed = 2f;
		[SerializeField] private float _quiteWalkSpeed = 1f;
		[SerializeField] private float _rotationSpeed = 180f;

		[Header("Teleport")]
		[SerializeField] private float _navMeshSampleDistance = 2.5f;

		public Animator Animator => _animator;
		public BoxCollider Hitbox => _hitbox;
		public NavMeshAgent NavMeshAgent => _navMeshAgent;
		public float WalkSpeed => _walkSpeed;
		public float QuiteWalkSpeed => _quiteWalkSpeed;
		public float RotationSpeed => _rotationSpeed;

		private MannequinStateMachine _stateMachine;
		private IMannequinStrategy _currentStrategy;
		private string _currentMovementAnimationState = "Idle";

		private void Awake()
		{
			_stateMachine = new MannequinStateMachine(this);
			_currentStrategy = new IdleStrategy();

			if (_navMeshAgent == null)
			{
				_navMeshAgent = GetComponent<NavMeshAgent>();
			}

			if (_navMeshAgent != null)
			{
				_navMeshAgent.updatePosition = true;
				_navMeshAgent.updateRotation = false;
				_navMeshAgent.stoppingDistance = 0.1f;
				_navMeshAgent.autoBraking = false;
			}

			if (_animator != null)
			{
				_animator.applyRootMotion = false;
			}
		}

		private void Update()
		{
			_stateMachine.Update();
			_currentStrategy?.Execute(this);
		}

		public void SetStrategy(IMannequinStrategy strategy)
		{
			if (strategy == null) return;

			if (_currentStrategy != null && _currentStrategy.GetType() == strategy.GetType())
			{
				return;
			}

			_currentStrategy = strategy;
		}

		public void SetAnimatorBool(string parameter, bool value)
		{
			if (_animator == null) return;

			if (_animator.GetBool(parameter) == value) return;
			_animator.SetBool(parameter, value);
		}

		public void SetMovementState(bool isTurning, bool isRunning, bool isWalking, bool isQuiteWalking)
		{
			string targetState = "Idle";
			if (isTurning) targetState = "Turning";
			else if (isRunning) targetState = "Running";
			else if (isQuiteWalking) targetState = "QuiteWalking";
			else if (isWalking) targetState = "Walking";

			if (_currentMovementAnimationState == targetState) return;

			_currentMovementAnimationState = targetState;
			SetAnimatorBool("IsTurning", isTurning);
			SetAnimatorBool("IsRunning", isRunning);
			SetAnimatorBool("IsWalking", isWalking);
			SetAnimatorBool("IsQuiteWalking", isQuiteWalking);

			if (_animator != null && _animator.HasState(0, Animator.StringToHash(targetState)))
			{
				_animator.CrossFade(targetState, 0f);
			}
		}

		public void ChangeState(MannequinState newState)
		{
			_stateMachine.ChangeState(newState);
		}

		public void SafeResetPath()
		{
			if (_navMeshAgent == null)
				return;

			if (!_navMeshAgent.enabled)
				return;

			if (!_navMeshAgent.isOnNavMesh)
				return;

			_navMeshAgent.ResetPath();
			_navMeshAgent.isStopped = true;
			_navMeshAgent.velocity = Vector3.zero;
		}

		public void SafeResumeAgent()
		{
			if (_navMeshAgent == null)
				return;

			if (!_navMeshAgent.enabled)
				return;

			if (!_navMeshAgent.isOnNavMesh)
				return;

			_navMeshAgent.isStopped = false;
		}

		public bool TeleportTo(Transform targetTransform)
		{
			if (targetTransform == null)
			{
				Debug.LogWarning("[Mannequin] TeleportTo failed: targetTransform is null.");
				return false;
			}

			return TeleportToPosition(targetTransform.position, targetTransform.rotation);
		}

		public bool TeleportToPosition(Vector3 targetPosition, Quaternion targetRotation)
		{
			Vector3 finalPosition = targetPosition;

			// Try to find a valid point on NavMesh near the target.
			if (NavMesh.SamplePosition(targetPosition, out NavMeshHit navHit, _navMeshSampleDistance, NavMesh.AllAreas))
			{
				finalPosition = navHit.position;
			}
			else
			{
				Debug.LogWarning($"[Mannequin] Teleport target is not near NavMesh. Target: {targetPosition}");
			}

			// Move transform first so visual position is always updated.
			transform.position = finalPosition;
			transform.rotation = targetRotation;

			if (_navMeshAgent == null || !_navMeshAgent.enabled)
				return true;

			// Warp only if the agent is currently on a NavMesh or can be snapped close enough.
			if (_navMeshAgent.isOnNavMesh)
			{
				bool warped = _navMeshAgent.Warp(finalPosition);
				if (!warped)
				{
					Debug.LogWarning($"[Mannequin] NavMeshAgent.Warp failed at position {finalPosition}");
					return false;
				}

				_navMeshAgent.ResetPath();
				_navMeshAgent.isStopped = true;
				_navMeshAgent.velocity = Vector3.zero;
				return true;
			}

			// If the agent is not on NavMesh, try enabling after moving onto sampled point.
			// Sometimes just moving transform onto the mesh is enough for next frame.
			Debug.LogWarning("[Mannequin] Agent is not currently on NavMesh after teleport. Transform moved, but agent warp was skipped.");
			return false;
		}

		public void SetWalkingAnimation(bool isWalking, bool isQuiteWalking = false)
		{
			if (_animator == null) return;

			_animator.SetBool("IsWalking", isWalking);
			_animator.SetBool("IsQuiteWalking", isQuiteWalking);
		}
	}
}