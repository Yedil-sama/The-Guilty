using System.Collections;
using UnityEngine;

namespace TheGuilty.Core.VisualEffects
{
	public sealed class CameraSwayEffect : MonoBehaviour
	{
		[SerializeField] private float _swayDuration = 3f;
		[SerializeField] private float _swayAmplitude = 2.5f;
		[SerializeField] private float _frequencyX = 0.8f;
		[SerializeField] private float _frequencyY = 0.6f;
		[SerializeField] private float _phaseOffsetY = 1f;

		private Camera _targetCamera;
		private Coroutine _activeSway;

		public void Initialize(Camera targetCamera)
		{
			_targetCamera = targetCamera;
		}

		public void Play()
		{
			if (_activeSway != null)
				return;

			_activeSway = StartCoroutine(SwayRoutine());
		}

		public void Stop()
		{
			if (_activeSway != null)
			{
				StopCoroutine(_activeSway);
				_activeSway = null;
			}
		}

		private IEnumerator SwayRoutine()
		{
			float elapsed = 0f;
			Quaternion baseRotation = _targetCamera.transform.localRotation;
			float startTime = Time.time;

			while (elapsed < _swayDuration)
			{
				float t = elapsed / _swayDuration;
				float intensity = Mathf.Lerp(1f, 0f, t);

				float swayX = Mathf.Sin((Time.time - startTime) * _frequencyX * Mathf.PI * 2f) * _swayAmplitude * intensity;
				float swayY = Mathf.Sin((Time.time - startTime) * _frequencyY * Mathf.PI * 2f + _phaseOffsetY) * _swayAmplitude * intensity;

				_targetCamera.transform.localRotation = baseRotation * Quaternion.Euler(swayX, swayY, 0);

				elapsed += Time.deltaTime;
				yield return null;
			}

			_targetCamera.transform.localRotation = baseRotation;
			_activeSway = null;
		}
	}
}
