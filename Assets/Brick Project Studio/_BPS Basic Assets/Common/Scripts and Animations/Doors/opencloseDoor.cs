using System.Collections;
using UnityEngine;
using UHFPS.Input;
using UHFPS.Runtime;

namespace SojaExiles

{
	public class OpenCloseDoor : MonoBehaviour
	{
		[SerializeField] private Animator _animator;
		[SerializeField] private bool _isOpen;
		[SerializeField] private Transform _player;
		[SerializeField] private float _interactionDistance = 15f;
		[SerializeField] private string _openAnimation = "Opening";
		[SerializeField] private string _closeAnimation = "Closing";
		[SerializeField] private float _animationDelay = 0.5f;

		public Animator Animator => _animator;
		public bool IsOpen
		{
			get => _isOpen;
			private set => _isOpen = value;
		}
		public Transform Player => _player;
		public float InteractionDistance
		{
			get => _interactionDistance;
			set => _interactionDistance = value;
		}
		public string OpenAnimation => _openAnimation;
		public string CloseAnimation => _closeAnimation;
		public float AnimationDelay => _animationDelay;

		private void Start()
		{
			IsOpen = false;
			TryAssignPlayer();
		}

		private void TryAssignPlayer()
		{
			if (_player != null)
				return;

			try
			{
				_player = PlayerManager.Instance.transform;
			}
			catch
			{
				// PlayerManager may not be initialized yet.
			}
		}

		private void OnMouseOver()
		{
			if (_player == null)
				TryAssignPlayer();

			if (_player == null)
				return;

			if (Vector3.Distance(_player.position, transform.position) > _interactionDistance)
				return;

			if (!InputManager.ReadButtonOnce(this, Controls.USE))
				return;

			if (!IsOpen)
				StartCoroutine(OpenDoorCoroutine());
			else
				StartCoroutine(CloseDoorCoroutine());
		}

		private IEnumerator OpenDoorCoroutine()
		{
			if (_animator == null)
				yield break;

			Debug.Log("Opening door");
			_animator.Play(_openAnimation);
			IsOpen = true;
			yield return new WaitForSeconds(_animationDelay);
		}

		private IEnumerator CloseDoorCoroutine()
		{
			if (_animator == null)
				yield break;

			Debug.Log("Closing door");
			_animator.Play(_closeAnimation);
			IsOpen = false;
			yield return new WaitForSeconds(_animationDelay);
		}
	}
}