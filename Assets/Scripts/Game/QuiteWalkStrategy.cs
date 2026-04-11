using UnityEngine;

namespace TheGuilty.Game
{
	public class QuiteWalkStrategy : IMannequinStrategy
	{
		private Transform _playerTransform;

		public QuiteWalkStrategy()
		{
			GameObject player = GameObject.FindGameObjectWithTag("Player");
			if (player != null)
			{
				_playerTransform = player.transform;
			}
		}

		public void Execute(Mannequin mannequin)
		{
			if (_playerTransform == null || mannequin.NavMeshAgent == null || mannequin.Animator == null) return;

			Vector3 targetPosition = _playerTransform.position;
			mannequin.NavMeshAgent.isStopped = false;
			mannequin.NavMeshAgent.SetDestination(targetPosition);

			Vector3 desiredVelocity = mannequin.NavMeshAgent.desiredVelocity;
			if (desiredVelocity.sqrMagnitude < 0.01f)
			{
				desiredVelocity = (targetPosition - mannequin.transform.position).normalized;
			}

			float angle = Vector3.Angle(mannequin.transform.forward, desiredVelocity);
			Quaternion targetRotation = Quaternion.LookRotation(desiredVelocity);
			mannequin.transform.rotation = Quaternion.Slerp(
				mannequin.transform.rotation,
				targetRotation,
				Time.deltaTime * mannequin.RotationSpeed / 180f
			);

			if (angle > 45f && mannequin.NavMeshAgent.hasPath)
			{
				mannequin.SetMovementState(true, false, false, false);
			}
			else
			{
				mannequin.NavMeshAgent.speed = mannequin.QuiteWalkSpeed;
				mannequin.SetMovementState(false, false, false, true);
			}
		}
	}
}