using System.Collections;
using TheGuilty.Game;
using UnityEngine;
using UnityEngine.AI;

namespace TheGuilty.Core.Directors
{
	public class MirrorMannequinNarrative : Narrative
	{
		private readonly Mannequin _mannequin;
		private readonly MirrorObserver _mirror;
		private readonly Transform _player;
		private readonly EventMannequinTransformHolder _holder;

		private AudioClip _dunClip;
		private AudioSource _sfxSource;

		private int _stage;
		private bool _sequenceStarted;
		private bool _finalTriggered;
		private bool _finishedAllStages;
		private float _stageTimer;
		private float _exitTimer;
		private bool _exitCountdownStarted;

		private const float StageAdvanceDelay = 3f;
		private const float ExitPunishDelay = 10f;
		private const float FinalSpawnDistance = 1.5f;

		private const string FarAwayEventName = "FarAway";

		public MirrorMannequinNarrative(
			Mannequin mannequin,
			MirrorObserver mirror,
			Transform player,
			EventMannequinTransformHolder holder,
			MonoBehaviour coroutineHost)
			: base(coroutineHost)
		{
			_mannequin = mannequin;
			_mirror = mirror;
			_player = player;
			_holder = holder;
		}

		public override void Start()
		{
			base.Start();

			_stage = -1;
			_stageTimer = 0f;
			_exitTimer = 0f;
			_exitCountdownStarted = false;
			_sequenceStarted = false;
			_finalTriggered = false;
			_finishedAllStages = false;

			if (_mannequin == null || _mirror == null || _player == null || _holder == null)
			{
				Debug.LogError("[MirrorMannequinNarrative] Missing required references.");
				_isComplete = true;
				return;
			}

			_dunClip = Resources.Load<AudioClip>("Audio/Sfx/dun");
			CreateAudioSource();

			TeleportFarAwayAndHide();
		}

		public override void Update()
		{
			if (!_isRunning || _isComplete || _finalTriggered)
				return;

			bool insideZone = _mirror.IsPlayerInsideZone;
			bool lookingAtMirror = _mirror.IsPlayerNearAndLookingAtMirror();

			if (!_sequenceStarted)
			{
				if (insideZone && lookingAtMirror)
				{
					_sequenceStarted = true;
					_stageTimer = 0f;
					AdvanceToNextStage();
				}

				return;
			}

			if (!insideZone && !_finishedAllStages)
			{
				if (!_exitCountdownStarted)
				{
					_exitCountdownStarted = true;
					_exitTimer = 0f;
				}

				_exitTimer += Time.deltaTime;
				if (_exitTimer >= ExitPunishDelay)
				{
					_finalTriggered = true;
					StartRoutine(PunishAndChasePlayerRoutine());
				}

				return;
			}

			if (insideZone)
			{
				_exitCountdownStarted = false;
				_exitTimer = 0f;
			}

			if (_finishedAllStages)
				return;

			_stageTimer += Time.deltaTime;

			if (_stageTimer >= StageAdvanceDelay)
			{
				AdvanceToNextStage();
			}
		}

		private void AdvanceToNextStage()
		{
			_stageTimer = 0f;
			_stage++;

			if (_stage <= 2)
			{
				MoveToStagePosition(_stage, true);
				return;
			}

			TeleportFarAwayAndHide();
			_finishedAllStages = true;
			_isComplete = true;
		}

		private void MoveToStagePosition(int stage, bool playSound)
		{
			string eventName = "MirrorStage" + stage;
			Transform pos = _holder.GetRandomPositionForEvent(eventName);

			if (pos == null)
			{
				Debug.LogWarning($"[MirrorMannequinNarrative] Event position '{eventName}' not found.");
				return;
			}

			_mannequin.TeleportTo(pos);
			SetIdleFacingPlayer();
			EnableHitbox();

			if (playSound)
				PlayOneShot(_dunClip);
		}

		private IEnumerator PunishAndChasePlayerRoutine()
		{
			Vector3 spawnPos = _player.position - _player.forward * FinalSpawnDistance;

			if (NavMesh.SamplePosition(spawnPos, out NavMeshHit navHit, 2f, NavMesh.AllAreas))
			{
				spawnPos = navHit.position;
			}

			Vector3 lookDir = _player.position - spawnPos;
			lookDir.y = 0f;
			Quaternion rot = lookDir.sqrMagnitude > 0.001f
				? Quaternion.LookRotation(lookDir.normalized)
				: Quaternion.identity;

			_mannequin.TeleportToPosition(spawnPos, rot);
			PlayOneShot(_dunClip);

			if (_player != null)
			{
				Vector3 dir = _player.position - _mannequin.transform.position;
				dir.y = 0f;
				if (dir.sqrMagnitude > 0.001f)
				{
					_mannequin.transform.rotation = Quaternion.LookRotation(dir.normalized);
				}
			}

			if (_mannequin.NavMeshAgent != null)
			{
				_mannequin.SafeResumeAgent();
			}

			_mannequin.SetStrategy(new RunningStrategy());
			_mannequin.ChangeState(MannequinState.Following);
			EnableHitbox();

			yield return null;
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
				Debug.LogWarning($"[MirrorMannequinNarrative] Event position '{FarAwayEventName}' not found.");
			}

			_mannequin.SetStrategy(new IdleStrategy());
			_mannequin.ChangeState(MannequinState.Idle);
			_mannequin.SafeResetPath();

			if (_mannequin.Hitbox != null)
				_mannequin.Hitbox.enabled = false;
		}

		private void SetIdleFacingPlayer()
		{
			if (_mannequin == null)
				return;

			if (_player != null)
			{
				Vector3 lookDir = _player.position - _mannequin.transform.position;
				lookDir.y = 0f;

				if (lookDir.sqrMagnitude > 0.001f)
				{
					_mannequin.transform.rotation = Quaternion.LookRotation(lookDir.normalized);
				}
			}

			_mannequin.SetStrategy(new IdleStrategy());
			_mannequin.ChangeState(MannequinState.Idle);
			_mannequin.SafeResetPath();
		}

		private void EnableHitbox()
		{
			if (_mannequin?.Hitbox != null)
				_mannequin.Hitbox.enabled = true;
		}

		private void CreateAudioSource()
		{
			GameObject go = new GameObject("MirrorNarrativeSFX");
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