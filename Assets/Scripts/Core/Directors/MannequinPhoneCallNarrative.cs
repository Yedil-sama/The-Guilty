using TheGuilty.Core.GameEvents;
using TheGuilty.Game;
using UnityEngine;

namespace TheGuilty.Core.Directors
{
	public class MannequinPhoneCallNarrative : Narrative
	{
		private Mannequin _mannequin;
		private Vector3 _phonePosition;
		private AudioClip _phoneRingClip;
		private AudioClip _voicelineClip;
		private float _phoneRingVolume;
		private Camera _playerCamera;
		private EventMannequinTransformHolder _transformHolder;
		private GameEventBus _eventBus;

		private AudioSource _loopingPhoneAudio;
		private AudioSource _voicelineAudio;
		private bool _phonePickedUp = false;
		private bool _mannequinTeleported = false;
		private bool _mannequinWasVisible = true;
		private float _elapsedTime = 0f;
		private float _teleportTimeout = 10f;

		public MannequinPhoneCallNarrative(
			Mannequin mannequin,
			Vector3 phonePosition,
			AudioClip phoneRingClip,
			AudioClip voicelineClip,
			Camera playerCamera,
			EventMannequinTransformHolder transformHolder,
			GameEventBus eventBus,
			float phoneRingVolume = 0.8f)
		{
			_mannequin = mannequin;
			_phonePosition = phonePosition;
			_phoneRingClip = phoneRingClip;
			_voicelineClip = voicelineClip;
			_phoneRingVolume = phoneRingVolume;
			_playerCamera = playerCamera;
			_transformHolder = transformHolder;
			_eventBus = eventBus;
		}

		public override void Start()
		{
			Debug.Log($"[MannequinPhoneCallNarrative] Start called");
			base.Start();
			_elapsedTime = 0f;

			if (_mannequin == null || _phoneRingClip == null || _playerCamera == null || _transformHolder == null)
			{
				Debug.Log($"[MannequinPhoneCallNarrative] Missing: Mannequin={_mannequin}, Clip={_phoneRingClip}, Camera={_playerCamera}, Holder={_transformHolder}");
				_isComplete = true;
				return;
			}

			// Set mannequin to idle
			_mannequin.SetStrategy(new IdleStrategy());
			_mannequin.ChangeState(MannequinState.Idle);

			// Start looping phone ring
			PlayLoopingPhoneRing();

			// Subscribe to phone call events
			_eventBus.Subscribe<PhoneCallStartedEvent>(OnPhoneCallStarted);
			_eventBus.Subscribe<PhoneCallEndedEvent>(OnPhoneCallEnded);
			Debug.Log("[MannequinPhoneCallNarrative] Subscribed to phone call events");
		}

		public override void Update()
		{
			if (!_isRunning) return;

			_elapsedTime += Time.deltaTime;

			// Check if mannequin is visible in camera frustum
			bool mannequinVisible = IsMannequinInCameraFrustum();

			//Debug.Log($"[MannequinPhoneCallNarrative] Update: time={_elapsedTime:F1}, visible={mannequinVisible}, picked={_phonePickedUp}, teleported={_mannequinTeleported}");

			// After 10 seconds
			if (_elapsedTime >= _teleportTimeout)
			{
				if (_phonePickedUp)
				{
					// If phone picked up and mannequin visible, start quiet walk
					if (mannequinVisible && !_mannequinTeleported)
					{
						Debug.Log("[MannequinPhoneCallNarrative] Phone picked up after 10s, mannequin visible, starting quiet walk towards player");
						_mannequin.SetStrategy(new QuiteWalkStrategy());
						_mannequin.ChangeState(MannequinState.Following);

						// Enable hitbox for attack
						BoxCollider hitbox = _mannequin.Hitbox.GetComponent<BoxCollider>();
						if (hitbox != null)
						{
							hitbox.enabled = true;
							Debug.Log("[MannequinPhoneCallNarrative] Hitbox enabled");
						}
					}
				}
				else
				{
					// If not picked up and mannequin out of frustum, teleport
					if (!mannequinVisible && !_mannequinTeleported)
					{
						Debug.Log($"[MannequinPhoneCallNarrative] Mannequin out of frustum after {_teleportTimeout}s, teleporting");
						TeleportMannequinToPhoneCallPosition();
						_mannequinTeleported = true;
					}
				}
			}

			_mannequinWasVisible = mannequinVisible;
		}

		private bool IsMannequinInCameraFrustum()
		{
			Renderer renderer = _mannequin.GetComponentInChildren<Renderer>();
			if (renderer == null)
			{
				Debug.LogWarning("[MannequinPhoneCallNarrative] No renderer found on mannequin!");
				return false;
			}

			Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(_playerCamera);
			bool visible = GeometryUtility.TestPlanesAABB(frustumPlanes, renderer.bounds);
			//Debug.Log($"[MannequinPhoneCallNarrative] Mannequin visible: {visible}, bounds: {renderer.bounds}");
			return visible;
		}

		private void PlayLoopingPhoneRing()
		{
			Debug.Log("[MannequinPhoneCallNarrative] PlayLoopingPhoneRing called");
			GameObject audioObject = new GameObject("PhoneRingSFX_Loop");
			audioObject.transform.position = _phonePosition;
			_loopingPhoneAudio = audioObject.AddComponent<AudioSource>();
			_loopingPhoneAudio.clip = _phoneRingClip;
			_loopingPhoneAudio.volume = _phoneRingVolume;
			_loopingPhoneAudio.spatialBlend = 1f;
			_loopingPhoneAudio.maxDistance = 50f;
			_loopingPhoneAudio.loop = true;
			_loopingPhoneAudio.Play();

			Debug.Log($"[MannequinPhoneCallNarrative] Phone ring started at {_phonePosition}, clip length: {_phoneRingClip.length}, volume: {_phoneRingVolume}");
		}

		private void PlayVoiceline()
		{
			Debug.Log("[MannequinPhoneCallNarrative] Playing voiceline");
			GameObject audioObject = new GameObject("VoicelineSFX");
			audioObject.transform.position = _phonePosition;
			_voicelineAudio = audioObject.AddComponent<AudioSource>();
			_voicelineAudio.clip = _voicelineClip;
			_voicelineAudio.volume = 1f;
			_voicelineAudio.spatialBlend = 1f;
			_voicelineAudio.maxDistance = 50f;
			_voicelineAudio.Play();

			// Schedule narrative end after voiceline
			_mannequin.StartCoroutine(EndNarrativeAfterVoiceline());
		}

		private System.Collections.IEnumerator EndNarrativeAfterVoiceline()
		{
			yield return new WaitForSeconds(_voicelineClip.length);
			Debug.Log("[MannequinPhoneCallNarrative] Voiceline finished, ending narrative");
			_isComplete = true;
		}

		private void TeleportMannequinToPhoneCallPosition()
		{
			Transform teleportPos = _transformHolder.GetRandomPositionForEvent("PhoneCallEvent");
			if (teleportPos == null)
			{
				Debug.Log("[MannequinPhoneCallNarrative] PhoneCallEvent position not found!");
				return;
			}

			_mannequin.TeleportTo(teleportPos);
		}

		private void OnPhoneCallStarted(PhoneCallStartedEvent evt)
		{
			_phonePickedUp = true;
			Debug.Log("[MannequinPhoneCallNarrative] Phone picked up!");

			// Stop looping phone ring
			if (_loopingPhoneAudio != null)
			{
				Object.Destroy(_loopingPhoneAudio.gameObject);
				_loopingPhoneAudio = null;
			}

			// Play voiceline
			PlayVoiceline();
		}

		private void OnPhoneCallEnded(PhoneCallEndedEvent evt)
		{
			Debug.Log("[MannequinPhoneCallNarrative] Phone call ended, switching to running chase mode");

			// Switch to running strategy for chase
			_mannequin.SetStrategy(new RunningStrategy());
			_mannequin.ChangeState(MannequinState.Following);

			// Enable hitbox for attack
			BoxCollider hitbox = _mannequin.Hitbox.GetComponent<BoxCollider>();
			if (hitbox != null)
			{
				hitbox.enabled = true;
				Debug.Log("[MannequinPhoneCallNarrative] Hitbox enabled for running chase");
			}
		}

		public override void End()
		{
			Debug.Log("[MannequinPhoneCallNarrative] Ending narrative");

			// Unsubscribe from events
			_eventBus.Unsubscribe<PhoneCallStartedEvent>(OnPhoneCallStarted);
			_eventBus.Unsubscribe<PhoneCallEndedEvent>(OnPhoneCallEnded);

			// Clean up looping audio
			if (_loopingPhoneAudio != null)
			{
				Object.Destroy(_loopingPhoneAudio.gameObject);
				_loopingPhoneAudio = null;
			}

			// Clean up voiceline audio
			if (_voicelineAudio != null)
			{
				Object.Destroy(_voicelineAudio.gameObject);
				_voicelineAudio = null;
			}

			base.End();
		}
	}
}