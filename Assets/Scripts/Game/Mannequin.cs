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
		[SerializeField] private float _rotationSpeed = 180f; // degrees per second

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
			_currentStrategy = new IdleStrategy(); // Default strategy

			if (_navMeshAgent == null)
			{
				_navMeshAgent = GetComponent<NavMeshAgent>();
			}

			// Configure NavMeshAgent
			if (_navMeshAgent != null)
			{
				_navMeshAgent.updatePosition = true;
				_navMeshAgent.updateRotation = false; // We handle rotation manually
				_navMeshAgent.stoppingDistance = 0.1f;
				_navMeshAgent.autoBraking = false;
			}

			// Disable root motion for NavMesh movement
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

		public void TeleportTo(Transform targetTransform)
		{
			transform.position = targetTransform.position;
			transform.rotation = targetTransform.rotation;
			if (_navMeshAgent != null)
			{
				_navMeshAgent.Warp(targetTransform.position);
			}
		}

		public void SetWalkingAnimation(bool isWalking, bool isQuiteWalking = false)
		{
			_animator.SetBool("IsWalking", isWalking);
			_animator.SetBool("IsQuiteWalking", isQuiteWalking);
		}
	}
}