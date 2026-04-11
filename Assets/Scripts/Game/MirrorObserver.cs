using UnityEngine;

namespace TheGuilty.Game
{
	public class MirrorObserver : MonoBehaviour
	{
		[SerializeField] private Camera _playerCamera;
		[SerializeField] private Transform _mirrorLookTarget;
		[SerializeField] private float _viewAngle = 30f;
		[SerializeField] private string _playerTag = "Player";

		private bool _playerInsideZone;
		private Transform _playerTransform;

		public bool IsPlayerInsideZone => _playerInsideZone;
		public Transform PlayerTransform => _playerTransform;

		private void Reset()
		{
			if (_playerCamera == null)
				_playerCamera = Camera.main;

			if (_mirrorLookTarget == null)
				_mirrorLookTarget = transform;
		}

		public bool IsPlayerNearAndLookingAtMirror()
		{
			if (!_playerInsideZone || _playerCamera == null || _mirrorLookTarget == null)
				return false;

			Vector3 dirToMirror = (_mirrorLookTarget.position - _playerCamera.transform.position).normalized;
			float angle = Vector3.Angle(_playerCamera.transform.forward, dirToMirror);

			return angle <= _viewAngle;
		}

		private void OnTriggerEnter(Collider other)
		{
			if (!other.CompareTag(_playerTag))
				return;

			_playerInsideZone = true;
			_playerTransform = other.transform;
		}

		private void OnTriggerStay(Collider other)
		{
			if (!other.CompareTag(_playerTag))
				return;

			_playerInsideZone = true;
			_playerTransform = other.transform;
		}

		private void OnTriggerExit(Collider other)
		{
			if (!other.CompareTag(_playerTag))
				return;

			_playerInsideZone = false;

			if (_playerTransform == other.transform)
				_playerTransform = null;
		}
	}
}