using System.Collections;
using TheGuilty.Game;
using UHFPS.Runtime;
using UnityEngine;
using UnityEngine.AI;

namespace TheGuilty.Core.Directors
{
	public class BedroomHideNarrative : Narrative
	{
		private enum BedroomHideState
		{
			WaitingToStart,
			EnteringRoom,
			Patrolling,
			LeavingRoom,
			Chasing,
			Finished
		}

		private readonly Mannequin _mannequin;
		private readonly EventMannequinTransformHolder _holder;
		private readonly HideInteract _hideSpot;
		private readonly DynamicObject _bedroomDoor;
		private readonly Transform _player;

		private AudioClip _dunClip;
		private AudioSource _sfxSource;

		private BedroomHideState _state;

		private float _startDelayTimer;
		private float _patrolTimer;
		private float _pointTimer;
		private int _currentPatrolIndex;

		private const float StartDelay = 20f;
		private const float PatrolDuration = 12f;
		private const float PatrolPointInterval = 3.5f;
		private const float DoorPauseTime = 0.8f;
		private const float LeaveTimeout = 4f;

		private const string BedroomDoorEventName = "BedroomDoor";
		private const string BedroomPatrol0 = "BedroomPatrol0";
		private const string BedroomPatrol1 = "BedroomPatrol1";
		private const string BedroomPatrol2 = "BedroomPatrol2";
		private const string FarAwayEventName = "FarAway";

		public BedroomHideNarrative(
			Mannequin mannequin,
			EventMannequinTransformHolder holder,
			HideInteract hideSpot,
			DynamicObject bedroomDoor,
			Transform player,
			MonoBehaviour coroutineHost)
			: base(coroutineHost)
		{
			_mannequin = mannequin;
			_holder = holder;
			_hideSpot = hideSpot;
			_bedroomDoor = bedroomDoor;
			_player = player;
		}

		public override void Start()
		{
			base.Start();

			_state = BedroomHideState.WaitingToStart;
			_startDelayTimer = 0f;
			_patrolTimer = 0f;
			_pointTimer = 0f;
			_currentPatrolIndex = 0;

			if (_mannequin == null || _holder == null || _hideSpot == null || _bedroomDoor == null || _player == null)
			{
				Debug.LogError("[BedroomHideNarrative] Missing required references.");
				_isComplete = true;
				return;
			}

			_dunClip = Resources.Load<AudioClip>("Audio/Sfx/dun");
			CreateAudioSource();

			TeleportFarAwayAndHide();
		}

		public override void Update()
		{
			if (!_isRunning || _isComplete)
				return;

			switch (_state)
			{
				case BedroomHideState.WaitingToStart:
					UpdateWaitingToStart();
					break;

				case BedroomHideState.EnteringRoom:
					UpdateEnteringRoom();
					break;

				case BedroomHideState.Patrolling:
					UpdatePatrolling();
					break;

				case BedroomHideState.LeavingRoom:
					UpdateLeavingRoom();
					break;

				case BedroomHideState.Chasing:
					UpdateChasing();
					break;

				case BedroomHideState.Finished:
					break;
			}
		}

		private void UpdateWaitingToStart()
		{
			_startDelayTimer += Time.deltaTime;

			if (_startDelayTimer < StartDelay)
				return;

			_state = BedroomHideState.EnteringRoom;
			StartRoutine(EnterBedroomRoutine());
		}

		private void UpdateEnteringRoom()
		{
			// Wait for coroutine to move state forward.
		}

		private void UpdatePatrolling()
		{
			// If player is not hidden at any point after mannequin enters bedroom -> chase immediately.
			if (!_hideSpot.IsHidden)
			{
				StartChase();
				return;
			}

			_patrolTimer += Time.deltaTime;
			_pointTimer += Time.deltaTime;

			if (_pointTimer >= PatrolPointInterval)
			{
				_pointTimer = 0f;
				_currentPatrolIndex = (_currentPatrolIndex + 1) % 3;
				MoveToPatrolIndex(_currentPatrolIndex);
			}

			if (_patrolTimer >= PatrolDuration)
			{
				_state = BedroomHideState.LeavingRoom;
				StartRoutine(LeaveBedroomRoutine());
			}
		}

		private void UpdateLeavingRoom()
		{
			// Wait for coroutine.
		}

		private void UpdateChasing()
		{
			// Chase continues until the mannequin catches player with hitbox / jumpscare logic.
			// No auto-complete here on purpose.
		}

		private IEnumerator EnterBedroomRoutine()
		{
			Transform bedroomDoorPoint = _holder.GetRandomPositionForEvent(BedroomDoorEventName);
			if (bedroomDoorPoint == null)
			{
				Debug.LogWarning($"[BedroomHideNarrative] Event '{BedroomDoorEventName}' not found.");
				_isComplete = true;
				yield break;
			}

			_mannequin.TeleportTo(bedroomDoorPoint);
			FacePlayer();
			SetIdle();
			DisableHitbox();

			OpenDoor();
			PlayOneShot(_dunClip);

			yield return new WaitForSeconds(DoorPauseTime);

			// If player is already not hidden when mannequin arrives -> chase immediately.
			if (!_hideSpot.IsHidden)
			{
				StartChase();
				yield break;
			}

			_patrolTimer = 0f;
			_pointTimer = 0f;
			_currentPatrolIndex = 0;

			MoveToPatrolIndex(_currentPatrolIndex);
			_state = BedroomHideState.Patrolling;
		}

		private IEnumerator LeaveBedroomRoutine()
		{
			Transform bedroomDoorPoint = _holder.GetRandomPositionForEvent(BedroomDoorEventName);
			if (bedroomDoorPoint == null)
			{
				Debug.LogWarning($"[BedroomHideNarrative] Event '{BedroomDoorEventName}' not found.");
				TeleportFarAwayAndHide();
				_state = BedroomHideState.Finished;
				_isComplete = true;
				yield break;
			}

			MoveAgentToPoint(bedroomDoorPoint.position, false);

			float leaveTimer = 0f;
			while (leaveTimer < LeaveTimeout)
			{
				if (!_hideSpot.IsHidden)
				{
					StartChase();
					yield break;
				}

				if (HasReachedDestination())
					break;

				leaveTimer += Time.deltaTime;
				yield return null;
			}

			SetIdle();
			CloseDoor();

			yield return new WaitForSeconds(DoorPauseTime);

			TeleportFarAwayAndHide();
			_state = BedroomHideState.Finished;
			_isComplete = true;
		}

		private void StartChase()
		{
			_state = BedroomHideState.Chasing;

			_mannequin.SafeResumeAgent();
			_mannequin.SetStrategy(new RunningStrategy());
			_mannequin.ChangeState(MannequinState.Following);
			_mannequin.SetMovementState(false, true, false, false);

			EnableHitbox();
			PlayOneShot(_dunClip);
		}

		private void MoveToPatrolIndex(int patrolIndex)
		{
			string eventName = patrolIndex switch
			{
				0 => BedroomPatrol0,
				1 => BedroomPatrol1,
				_ => BedroomPatrol2
			};

			Transform patrolPoint = _holder.GetRandomPositionForEvent(eventName);
			if (patrolPoint == null)
			{
				Debug.LogWarning($"[BedroomHideNarrative] Event '{eventName}' not found.");
				return;
			}

			MoveAgentToPoint(patrolPoint.position, true);
		}

		private void MoveAgentToPoint(Vector3 position, bool patrolWalk)
		{
			if (_mannequin == null || _mannequin.NavMeshAgent == null)
				return;

			if (NavMesh.SamplePosition(position, out NavMeshHit navHit, 2f, NavMesh.AllAreas))
			{
				position = navHit.position;
			}

			_mannequin.SetStrategy(new IdleStrategy());
			_mannequin.ChangeState(MannequinState.Idle);

			_mannequin.SafeResumeAgent();

			NavMeshAgent agent = _mannequin.NavMeshAgent;
			agent.isStopped = false;
			agent.SetDestination(position);

			if (patrolWalk)
			{
				agent.speed = _mannequin.QuiteWalkSpeed;
				_mannequin.SetMovementState(false, false, false, true);
			}
			else
			{
				agent.speed = _mannequin.WalkSpeed;
				_mannequin.SetMovementState(false, false, true, false);
			}

			FaceMovementDirection(position);
		}

		private bool HasReachedDestination()
		{
			if (_mannequin == null || _mannequin.NavMeshAgent == null)
				return true;

			NavMeshAgent agent = _mannequin.NavMeshAgent;

			if (agent.pathPending)
				return false;

			if (agent.remainingDistance > agent.stoppingDistance + 0.05f)
				return false;

			return !agent.hasPath || agent.velocity.sqrMagnitude < 0.01f;
		}

		private void SetIdle()
		{
			if (_mannequin == null)
				return;

			_mannequin.SetStrategy(new IdleStrategy());
			_mannequin.ChangeState(MannequinState.Idle);
			_mannequin.SetMovementState(false, false, false, false);
			_mannequin.SafeResetPath();
		}

		private void FacePlayer()
		{
			if (_mannequin == null || _player == null)
				return;

			Vector3 lookDir = _player.position - _mannequin.transform.position;
			lookDir.y = 0f;

			if (lookDir.sqrMagnitude > 0.001f)
			{
				_mannequin.transform.rotation = Quaternion.LookRotation(lookDir.normalized);
			}
		}

		private void FaceMovementDirection(Vector3 targetPosition)
		{
			if (_mannequin == null)
				return;

			Vector3 lookDir = targetPosition - _mannequin.transform.position;
			lookDir.y = 0f;

			if (lookDir.sqrMagnitude > 0.001f)
			{
				_mannequin.transform.rotation = Quaternion.LookRotation(lookDir.normalized);
			}
		}

		private void OpenDoor()
		{
			if (_bedroomDoor == null)
				return;

			_bedroomDoor.SetOpenState();
		}

		private void CloseDoor()
		{
			if (_bedroomDoor == null)
				return;

			_bedroomDoor.SetCloseState();
		}

		private void TeleportFarAwayAndHide()
		{
			Transform farAway = _holder.GetRandomPositionForEvent(FarAwayEventName);
			if (farAway != null)
			{
				_mannequin.TeleportTo(farAway);
			}
			else
			{
				Debug.LogWarning($"[BedroomHideNarrative] Event '{FarAwayEventName}' not found.");
			}

			SetIdle();
			DisableHitbox();
		}

		private void EnableHitbox()
		{
			if (_mannequin?.Hitbox != null)
				_mannequin.Hitbox.enabled = true;
		}

		private void DisableHitbox()
		{
			if (_mannequin?.Hitbox != null)
				_mannequin.Hitbox.enabled = false;
		}

		private void CreateAudioSource()
		{
			GameObject go = new GameObject("BedroomHideNarrativeSFX");
			_sfxSource = go.AddComponent<AudioSource>();
			_sfxSource.playOnAwake = false;
			_sfxSource.spatialBlend = 0f;
		}

		private void PlayOneShot(AudioClip clip)
		{
			if (_sfxSource != null && clip != null)
				_sfxSource.PlayOneShot(clip);
		}

		public override void End()
		{
			if (_sfxSource != null)
				Object.Destroy(_sfxSource.gameObject);

			base.End();
		}
	}
}