using System.Collections;
using TheGuilty.Core.GameEvents;
using TheGuilty.Game;
using UnityEngine;
using UnityEngine.AI;

namespace TheGuilty.Core.Directors
{
	public class MannequinPhoneCallNarrative : Narrative
	{
		private enum NarrativeMoveMode
		{
			None,
			Idle,
			QuietWalk,
			Run
		}

		private readonly Mannequin _mannequin;
		private readonly Vector3 _phonePosition;
		private readonly AudioClip _phoneRingClip;
		private readonly AudioClip _voicelineClip;
		private readonly float _phoneRingVolume;
		private readonly Camera _playerCamera;
		private readonly EventMannequinTransformHolder _transformHolder;
		private readonly GameEventBus _eventBus;
		private readonly Transform _playerTransform;

		private AudioSource _loopingPhoneAudio;
		private AudioSource _voicelineAudio;
		private AudioSource _narrativeSfxAudio;

		private AudioClip _zinClip;
		private AudioClip _stringClip;

		private bool _phonePickedUp;
		private bool _phonePutDown;
		private bool _voicelineFinished;

		private bool _playerSawMannequin;
		private bool _teleportSucceeded;
		private bool _ambushAvailable;

		private bool _playerSawOriginalSpotAfterTeleport;
		private bool _zinPlayed;

		private bool _shouldQuietWalkAfterPickup;
		private bool _postCallPauseStarted;
		private bool _postCallRunStarted;
		private bool _runSequenceFinished;

		private bool _afkChaseStarted;
		private bool _afkPunishmentStarted;
		private bool _isDisappearing;

		private float _elapsedTime;
		private float _ignorePhoneTimer;
		private float _postCallPauseTimer;
		private float _postCallRunTimer;
		private float _afkChaseTimer;

		private Vector3 _initialMannequinPosition;
		private NarrativeMoveMode _currentMode = NarrativeMoveMode.None;

		private const float InitialTeleportDelay = 10f;
		private const float IgnorePhoneThreshold = 60f;
		private const float PostCallPauseDuration = 1f;
		private const float PostCallRunDuration = 10f;
		private const float AfkChaseDuration = 20f;
		private const float AfkDisappearDuration = 10f;
		private const float ForcedTeleportDistance = 1.5f;
		private const float LookAtOriginalSpotDistance = 5f;
		private const float LookAtOriginalSpotAngle = 30f;
		private const float MannequinSeenAngle = 35f;

		private const string PhoneCallEventName = "PhoneCallEvent";
		private const string FarAwayEventName = "FarAway";

		public MannequinPhoneCallNarrative(
			Mannequin mannequin,
			Vector3 phonePosition,
			AudioClip phoneRingClip,
			AudioClip voicelineClip,
			Camera playerCamera,
			EventMannequinTransformHolder transformHolder,
			GameEventBus eventBus,
			MonoBehaviour coroutineHost,
			float phoneRingVolume = 0.8f)
			: base(coroutineHost)
		{
			_mannequin = mannequin;
			_phonePosition = phonePosition;
			_phoneRingClip = phoneRingClip;
			_voicelineClip = voicelineClip;
			_phoneRingVolume = phoneRingVolume;
			_playerCamera = playerCamera;
			_transformHolder = transformHolder;
			_eventBus = eventBus;
			_playerTransform = playerCamera != null ? playerCamera.transform.root : null;
		}

		public override void Start()
		{
			base.Start();

			_phonePickedUp = false;
			_phonePutDown = false;
			_voicelineFinished = false;

			_playerSawMannequin = false;
			_teleportSucceeded = false;
			_ambushAvailable = false;

			_playerSawOriginalSpotAfterTeleport = false;
			_zinPlayed = false;

			_shouldQuietWalkAfterPickup = false;
			_postCallPauseStarted = false;
			_postCallRunStarted = false;
			_runSequenceFinished = false;

			_afkChaseStarted = false;
			_afkPunishmentStarted = false;
			_isDisappearing = false;

			_elapsedTime = 0f;
			_ignorePhoneTimer = 0f;
			_postCallPauseTimer = 0f;
			_postCallRunTimer = 0f;
			_afkChaseTimer = 0f;

			_currentMode = NarrativeMoveMode.None;
			_initialMannequinPosition = _mannequin != null ? _mannequin.transform.position : Vector3.zero;

			if (_mannequin == null || _phoneRingClip == null || _playerCamera == null || _transformHolder == null || _eventBus == null)
			{
				Debug.LogError("[MannequinPhoneCallNarrative] Missing required references.");
				_isComplete = true;
				return;
			}

			_zinClip = Resources.Load<AudioClip>("Audio/Sfx/zin");
			_stringClip = Resources.Load<AudioClip>("Audio/Sfx/string");

			CreateNarrativeAudioSource();

			SetMode(NarrativeMoveMode.Idle);
			DisableHitbox();
			PlayLoopingPhoneRing();

			_eventBus.Subscribe<PhoneCallStartedEvent>(OnPhoneCallStarted);
			_eventBus.Subscribe<PhoneCallEndedEvent>(OnPhoneCallEnded);
		}

		public override void Update()
		{
			if (!_isRunning || _isComplete || _isDisappearing)
				return;

			_elapsedTime += Time.deltaTime;
			UpdateVoicelineFinishedState();

			bool mannequinSeen = IsMannequinSeenByPlayer();
			if (mannequinSeen)
				_playerSawMannequin = true;

			// SIMPLE WANTED BEHAVIOR:
			// After first 10 sec, if phone not picked up, keep checking every frame.
			// First frame mannequin is not seen -> teleport.
			if (!_phonePickedUp && !_teleportSucceeded && _elapsedTime >= InitialTeleportDelay)
			{
				if (!mannequinSeen)
				{
					_teleportSucceeded = TeleportToPhoneCallEvent();

					if (_teleportSucceeded)
					{
						_ambushAvailable = true;
						SetMode(NarrativeMoveMode.Idle);
						EnableHitbox();
					}
				}
			}

			// If player kept mannequin in view past 10 sec and then picked up the phone,
			// only then mannequin should quiet walk.
			if (_phonePickedUp && !_teleportSucceeded && _elapsedTime >= InitialTeleportDelay)
			{
				_shouldQuietWalkAfterPickup = true;
			}

			if (_teleportSucceeded && !_zinPlayed && !_playerSawOriginalSpotAfterTeleport)
			{
				if (IsPlayerLookingAtOriginalSpot())
				{
					_playerSawOriginalSpotAfterTeleport = true;
					_zinPlayed = true;
					PlayNarrativeSfx(_zinClip);
				}
			}

			if (!_phonePickedUp)
			{
				_ignorePhoneTimer += Time.deltaTime;

				if (_ignorePhoneTimer >= IgnorePhoneThreshold && !_afkChaseStarted && !_afkPunishmentStarted)
				{
					_afkChaseStarted = true;
					_afkChaseTimer = 0f;
					SetMode(NarrativeMoveMode.Run);
					EnableHitbox();
				}

				if (_afkChaseStarted)
				{
					_afkChaseTimer += Time.deltaTime;

					if (_afkChaseTimer >= AfkChaseDuration && !_afkPunishmentStarted)
					{
						_afkPunishmentStarted = true;
						_afkChaseStarted = false;
						StartRoutine(AfkPunishmentRoutine());
					}
				}

				return;
			}

			if (_ambushAvailable)
			{
				SetMode(NarrativeMoveMode.Idle);
				EnableHitbox();

				if (_voicelineFinished)
				{
					DisappearFarAway();
					_isComplete = true;
				}

				return;
			}

			if (_shouldQuietWalkAfterPickup && !_phonePutDown)
			{
				DisableHitbox();
				SetMode(NarrativeMoveMode.QuietWalk);
				return;
			}

			if (_shouldQuietWalkAfterPickup && _phonePutDown)
			{
				if (!_postCallPauseStarted)
				{
					_postCallPauseStarted = true;
					_postCallPauseTimer = 0f;
					SetMode(NarrativeMoveMode.Idle);
					DisableHitbox();
					PlayNarrativeSfx(_stringClip);
					return;
				}

				if (_postCallPauseTimer < PostCallPauseDuration)
				{
					_postCallPauseTimer += Time.deltaTime;
					return;
				}

				if (!_postCallRunStarted)
				{
					_postCallRunStarted = true;
					_postCallRunTimer = 0f;
					SetMode(NarrativeMoveMode.Run);
					EnableHitbox();
				}

				_postCallRunTimer += Time.deltaTime;
				if (_postCallRunTimer >= PostCallRunDuration && !_runSequenceFinished)
				{
					DisappearFarAway();
					_runSequenceFinished = true;
				}
			}

			if (_shouldQuietWalkAfterPickup)
			{
				if (_runSequenceFinished && _voicelineFinished)
					_isComplete = true;
			}
			else
			{
				if (_voicelineFinished)
					_isComplete = true;
			}
		}

		private void OnPhoneCallStarted(PhoneCallStartedEvent evt)
		{
			if (_phonePickedUp)
				return;

			_phonePickedUp = true;
			StopLoopingPhoneRing();
			PlayVoiceline();
		}

		private void OnPhoneCallEnded(PhoneCallEndedEvent evt)
		{
			if (_phonePickedUp)
				_phonePutDown = true;
		}

		private void UpdateVoicelineFinishedState()
		{
			if (_voicelineFinished || _voicelineAudio == null)
				return;

			if (!_voicelineAudio.isPlaying)
				_voicelineFinished = true;
		}

		private void SetMode(NarrativeMoveMode mode)
		{
			if (_mannequin == null || _currentMode == mode)
				return;

			_currentMode = mode;

			switch (mode)
			{
				case NarrativeMoveMode.Idle:
					_mannequin.SetStrategy(new IdleStrategy());
					_mannequin.ChangeState(MannequinState.Idle);
					_mannequin.SafeResetPath();
					break;

				case NarrativeMoveMode.QuietWalk:
					_mannequin.SafeResumeAgent();
					_mannequin.SetStrategy(new QuiteWalkStrategy());
					_mannequin.ChangeState(MannequinState.Following);
					break;

				case NarrativeMoveMode.Run:
					_mannequin.SafeResumeAgent();
					_mannequin.SetStrategy(new RunningStrategy());
					_mannequin.ChangeState(MannequinState.Following);
					break;
			}
		}

		private bool IsMannequinSeenByPlayer()
		{
			if (_mannequin == null || _playerCamera == null)
				return false;

			Vector3 targetPos = _mannequin.Hitbox != null
				? _mannequin.Hitbox.bounds.center
				: _mannequin.transform.position + Vector3.up * 1.2f;

			Vector3 viewport = _playerCamera.WorldToViewportPoint(targetPos);
			if (viewport.z <= 0f)
				return false;

			if (viewport.x < 0f || viewport.x > 1f || viewport.y < 0f || viewport.y > 1f)
				return false;

			Vector3 dir = (targetPos - _playerCamera.transform.position).normalized;
			float angle = Vector3.Angle(_playerCamera.transform.forward, dir);
			return angle <= MannequinSeenAngle;
		}

		private bool IsPlayerLookingAtOriginalSpot()
		{
			if (_playerCamera == null)
				return false;

			Vector3 toSpot = _initialMannequinPosition - _playerCamera.transform.position;
			float distance = toSpot.magnitude;
			if (distance > LookAtOriginalSpotDistance)
				return false;

			float angle = Vector3.Angle(_playerCamera.transform.forward, toSpot.normalized);
			return angle <= LookAtOriginalSpotAngle;
		}

		private void CreateNarrativeAudioSource()
		{
			GameObject go = new GameObject("PhoneCallNarrativeSFX");
			_narrativeSfxAudio = go.AddComponent<AudioSource>();
			_narrativeSfxAudio.playOnAwake = false;
			_narrativeSfxAudio.loop = false;
			_narrativeSfxAudio.spatialBlend = 0f;
		}

		private void PlayNarrativeSfx(AudioClip clip)
		{
			if (_narrativeSfxAudio != null && clip != null)
				_narrativeSfxAudio.PlayOneShot(clip);
		}

		private void PlayLoopingPhoneRing()
		{
			GameObject audioObject = new GameObject("PhoneRingSFX_Loop");
			audioObject.transform.position = _phonePosition;

			_loopingPhoneAudio = audioObject.AddComponent<AudioSource>();
			_loopingPhoneAudio.clip = _phoneRingClip;
			_loopingPhoneAudio.volume = _phoneRingVolume;
			_loopingPhoneAudio.spatialBlend = 1f;
			_loopingPhoneAudio.maxDistance = 50f;
			_loopingPhoneAudio.loop = true;
			_loopingPhoneAudio.Play();
		}

		private void StopLoopingPhoneRing()
		{
			if (_loopingPhoneAudio != null)
			{
				Object.Destroy(_loopingPhoneAudio.gameObject);
				_loopingPhoneAudio = null;
			}
		}

		private void PlayVoiceline()
		{
			if (_voicelineClip == null)
				return;

			GameObject audioObject = new GameObject("PhoneVoicelineSFX");
			audioObject.transform.position = _phonePosition;

			_voicelineAudio = audioObject.AddComponent<AudioSource>();
			_voicelineAudio.clip = _voicelineClip;
			_voicelineAudio.volume = 1f;
			_voicelineAudio.spatialBlend = 1f;
			_voicelineAudio.maxDistance = 50f;
			_voicelineAudio.loop = false;
			_voicelineAudio.Play();
		}

		private IEnumerator AfkPunishmentRoutine()
		{
			_isDisappearing = true;
			DisappearFarAway();

			yield return new WaitForSeconds(AfkDisappearDuration);

			if (_mannequin == null || _playerTransform == null)
			{
				_isComplete = true;
				yield break;
			}

			Vector3 spawnPos = _playerTransform.position + _playerTransform.forward * ForcedTeleportDistance;

			if (NavMesh.SamplePosition(spawnPos, out NavMeshHit navHit, 2f, NavMesh.AllAreas))
			{
				spawnPos = navHit.position;
			}

			Vector3 lookDir = _playerTransform.position - spawnPos;
			lookDir.y = 0f;
			Quaternion rot = lookDir.sqrMagnitude > 0.001f
				? Quaternion.LookRotation(lookDir.normalized)
				: Quaternion.identity;

			_mannequin.TeleportToPosition(spawnPos, rot);
			SetMode(NarrativeMoveMode.Idle);
			EnableHitbox();

			_isComplete = true;
		}

		private bool TeleportToPhoneCallEvent()
		{
			Transform teleportPos = _transformHolder.GetRandomPositionForEvent(PhoneCallEventName);
			if (teleportPos == null)
			{
				Debug.LogWarning($"[MannequinPhoneCallNarrative] Event position '{PhoneCallEventName}' not found.");
				return false;
			}

			Vector3 before = _mannequin.transform.position;
			_mannequin.TeleportTo(teleportPos);

			float distanceToTarget = Vector3.Distance(_mannequin.transform.position, teleportPos.position);
			float movedDistance = Vector3.Distance(before, _mannequin.transform.position);

			return distanceToTarget <= 2.5f || movedDistance > 0.5f;
		}

		private void DisappearFarAway()
		{
			Transform farAway = _transformHolder.GetRandomPositionForEvent(FarAwayEventName);
			if (farAway == null)
			{
				Debug.LogWarning($"[MannequinPhoneCallNarrative] Event position '{FarAwayEventName}' not found.");
				SetMode(NarrativeMoveMode.Idle);
				DisableHitbox();
				return;
			}

			_mannequin.TeleportTo(farAway);
			SetMode(NarrativeMoveMode.Idle);
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

		public override void End()
		{
			_eventBus?.Unsubscribe<PhoneCallStartedEvent>(OnPhoneCallStarted);
			_eventBus?.Unsubscribe<PhoneCallEndedEvent>(OnPhoneCallEnded);

			if (_loopingPhoneAudio != null)
				Object.Destroy(_loopingPhoneAudio.gameObject);

			if (_voicelineAudio != null)
				Object.Destroy(_voicelineAudio.gameObject);

			if (_narrativeSfxAudio != null)
				Object.Destroy(_narrativeSfxAudio.gameObject);

			base.End();
		}
	}
}