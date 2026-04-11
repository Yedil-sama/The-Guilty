using System.Collections;
using UHFPS.Runtime;
using UnityEngine;

namespace TheGuilty.Game
{
	[RequireComponent(typeof(Collider))]
	public class MannequinHitbox : MonoBehaviour
	{
		[Header("Setup")]
		[SerializeField] private Mannequin _mannequin;
		[SerializeField] private Transform _jumpscareLookTarget;

		[Header("UHFPS")]
		[SerializeField] private JumpscareTrigger _uhfpsJumpscareTrigger;

		[Header("Attack")]
		[SerializeField] private float _killDelay = 2f;
		[SerializeField] private float _unlockBeforeKillDelay = 0.05f;
		[SerializeField] private bool _freezePlayerIfNoUhfpsTrigger = true;

		[Header("Animation")]
		[SerializeField] private string _jumpscareAnimationState = "Jumpscare";
		[SerializeField] private float _animationCrossFadeTime = 0.05f;
		[SerializeField] private bool _facePlayerBeforeScare = true;

		[Header("Audio")]
		[SerializeField] private string _jumpscareResourcesPath = "Audio/Sfx/Jumpscares";
		[SerializeField, Range(0f, 1f)] private float _jumpscareVolume = 1f;
		[SerializeField] private float _spatialBlend = 0f;

		private bool _isProcessingAttack;
		private AudioClip[] _jumpscareClips;
		private Collider _hitboxCollider;

		private void Awake()
		{
			_hitboxCollider = GetComponent<Collider>();

			if (_mannequin == null)
			{
				_mannequin = GetComponentInParent<Mannequin>();
			}

			_jumpscareClips = Resources.LoadAll<AudioClip>(_jumpscareResourcesPath);

			if (_jumpscareClips == null || _jumpscareClips.Length == 0)
			{
				Debug.LogWarning($"[MannequinHitbox] No jumpscare clips found at Resources/{_jumpscareResourcesPath}");
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			if (_isProcessingAttack)
				return;

			if (!other.CompareTag("Player"))
				return;

			if (!other.TryGetComponent(out PlayerHealth playerHealth))
				return;

			StartCoroutine(JumpscareAndKillRoutine(playerHealth, other.transform));
		}

		private IEnumerator JumpscareAndKillRoutine(PlayerHealth playerHealth, Transform playerTransform)
		{
			_isProcessingAttack = true;

			if (_hitboxCollider != null)
				_hitboxCollider.enabled = false;

			StopMannequin();

			if (_facePlayerBeforeScare && _mannequin != null && playerTransform != null)
			{
				Vector3 lookDir = playerTransform.position - _mannequin.transform.position;
				lookDir.y = 0f;

				if (lookDir.sqrMagnitude > 0.001f)
				{
					_mannequin.transform.rotation = Quaternion.LookRotation(lookDir.normalized);
				}
			}

			PlayMannequinJumpscareAnimation();
			PlayRandomJumpscareSfx();

			bool usedUhfpsJumpscare = false;

			if (_uhfpsJumpscareTrigger != null && JumpscareManager.HasReference)
			{
				JumpscareManager.Instance.StartJumpscareEffect(_uhfpsJumpscareTrigger);
				usedUhfpsJumpscare = true;
			}
			else if (_freezePlayerIfNoUhfpsTrigger)
			{
				FreezePlayerFallback(true);
			}

			yield return new WaitForSeconds(_killDelay);

			// Release lock just before death so UHFPS doesn't remain stuck.
			if (usedUhfpsJumpscare && JumpscareManager.HasReference)
			{
				JumpscareManager.Instance.EndJumpscareEffect();
				yield return new WaitForSeconds(_unlockBeforeKillDelay);
			}
			else if (_freezePlayerIfNoUhfpsTrigger)
			{
				FreezePlayerFallback(false);
				yield return new WaitForSeconds(_unlockBeforeKillDelay);
			}

			playerHealth.ApplyDamageMax();

			Debug.LogWarning("[MannequinHitbox] Jumpscare finished, player killed.");
		}

		private void StopMannequin()
		{
			if (_mannequin == null)
				return;

			_mannequin.SetStrategy(new IdleStrategy());
			_mannequin.ChangeState(MannequinState.Idle);

			if (_mannequin.NavMeshAgent != null)
			{
				_mannequin.NavMeshAgent.ResetPath();
				_mannequin.NavMeshAgent.isStopped = true;
				_mannequin.NavMeshAgent.velocity = Vector3.zero;
			}
		}

		private void PlayMannequinJumpscareAnimation()
		{
			if (_mannequin == null || _mannequin.Animator == null)
				return;

			int stateHash = Animator.StringToHash(_jumpscareAnimationState);

			if (_mannequin.Animator.HasState(0, stateHash))
			{
				_mannequin.Animator.CrossFade(stateHash, _animationCrossFadeTime);
			}
			else
			{
				Debug.LogWarning($"[MannequinHitbox] Animator state '{_jumpscareAnimationState}' was not found.");
			}
		}

		private void PlayRandomJumpscareSfx()
		{
			if (_jumpscareClips == null || _jumpscareClips.Length == 0)
				return;

			AudioClip clip = _jumpscareClips[Random.Range(0, _jumpscareClips.Length)];
			if (clip == null)
				return;

			GameObject audioObject = new GameObject("MannequinJumpscareSFX");
			audioObject.transform.position = _jumpscareLookTarget != null
				? _jumpscareLookTarget.position
				: transform.position;

			AudioSource source = audioObject.AddComponent<AudioSource>();
			source.clip = clip;
			source.volume = _jumpscareVolume;
			source.spatialBlend = _spatialBlend;
			source.playOnAwake = false;
			source.loop = false;
			source.Play();

			Destroy(audioObject, clip.length + 0.2f);
		}

		private void FreezePlayerFallback(bool freeze)
		{
			if (!GameManager.HasReference)
				return;

			GameManager.Instance.FreezePlayer(freeze);
		}
	}
}