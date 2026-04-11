using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace TheGuilty.Core.VisualEffects
{
	public sealed class WakeUpEffectController : MonoBehaviour, IPostProcessEffect
	{
		[Header("References")]
		[SerializeField] private Volume _postProcessVolume;
		[SerializeField] private Camera _targetCamera;

		[Header("Timing")]
		[SerializeField] private float _totalDuration = 5f;
		[SerializeField] private AnimationCurve _intensityCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

		[Header("Depth of Field")]
		[SerializeField] private float _startGaussianEnd = 5f;
		[SerializeField] private float _endGaussianEnd = 30f;
		[SerializeField] private float _startGaussianMaxRadius = 1.5f;
		[SerializeField] private float _endGaussianMaxRadius = 1f;

		[Header("Vignette")]
		[SerializeField] private float _startVignetteIntensity = 0.6f;
		[SerializeField] private float _endVignetteIntensity = 0f;

		[Header("Chromatic Aberration")]
		[SerializeField] private float _startChromaticIntensity = 0.3f;
		[SerializeField] private float _endChromaticIntensity = 0f;

		[Header("Camera Sway")]
		[SerializeField] private bool _enableCameraSway = true;
		[SerializeField] private float _swayDuration = 3f;
		[SerializeField] private float _swayAmplitude = 2.5f;

		private DepthOfField _depthOfField;
		private Vignette _vignette;
		private ChromaticAberration _chromaticAberration;
		private CameraSwayEffect _cameraSway;
		private Coroutine _activeEffect;

		public bool IsPlaying => _activeEffect != null;

		public void Initialize(Volume postProcessVolume, Camera targetCamera)
		{
			_postProcessVolume = postProcessVolume;
			_targetCamera = targetCamera;

			if (_enableCameraSway && _targetCamera != null)
			{
				_cameraSway = gameObject.AddComponent<CameraSwayEffect>();
				_cameraSway.Initialize(_targetCamera);
			}

			CacheVolumeComponents();
		}

		private void CacheVolumeComponents()
		{
			if (_postProcessVolume == null || _postProcessVolume.profile == null)
				return;

			if (!_postProcessVolume.profile.TryGet(out _depthOfField))
			{
				_depthOfField = _postProcessVolume.profile.Add<DepthOfField>();
			}

			if (!_postProcessVolume.profile.TryGet(out _vignette))
			{
				_vignette = _postProcessVolume.profile.Add<Vignette>();
			}

			if (!_postProcessVolume.profile.TryGet(out _chromaticAberration))
			{
				_chromaticAberration = _postProcessVolume.profile.Add<ChromaticAberration>();
			}
		}

		public void Play()
		{
			if (_activeEffect != null)
				return;

			CacheVolumeComponents();
			SetInitialValues();
			EnableEffects();

			if (_enableCameraSway && _cameraSway != null)
			{
				_cameraSway.Play();
			}

			_activeEffect = StartCoroutine(PlayEffectRoutine());
		}

		public void Stop()
		{
			if (_activeEffect != null)
			{
				StopCoroutine(_activeEffect);
				_activeEffect = null;
			}

			if (_cameraSway != null)
				_cameraSway.Stop();

			DisableEffects();
		}

		private void SetInitialValues()
		{
			if (_depthOfField != null)
			{
				_depthOfField.mode.value = DepthOfFieldMode.Gaussian;
				_depthOfField.gaussianStart.value = 0f;
				_depthOfField.gaussianEnd.value = _startGaussianEnd;
				_depthOfField.gaussianMaxRadius.value = _startGaussianMaxRadius;
			}

			if (_vignette != null)
			{
				_vignette.intensity.value = _startVignetteIntensity;
				_vignette.smoothness.value = 0.3f;
				_vignette.rounded.value = false;
			}

			if (_chromaticAberration != null)
			{
				_chromaticAberration.intensity.value = _startChromaticIntensity;
			}
		}

		private IEnumerator PlayEffectRoutine()
		{
			float elapsed = 0f;

			while (elapsed < _totalDuration)
			{
				float t = elapsed / _totalDuration;
				float curveValue = _intensityCurve.Evaluate(t);

				if (_depthOfField != null)
				{
					_depthOfField.gaussianEnd.value = Mathf.Lerp(_startGaussianEnd, _endGaussianEnd, curveValue);
					_depthOfField.gaussianMaxRadius.value = Mathf.Lerp(_startGaussianMaxRadius, _endGaussianMaxRadius, curveValue);
				}

				if (_vignette != null)
				{
					_vignette.intensity.value = Mathf.Lerp(_startVignetteIntensity, _endVignetteIntensity, curveValue);
				}

				if (_chromaticAberration != null)
				{
					_chromaticAberration.intensity.value = Mathf.Lerp(_startChromaticIntensity, _endChromaticIntensity, curveValue);
				}

				elapsed += Time.deltaTime;
				yield return null;
			}

			DisableEffects();
			_activeEffect = null;
		}

		private void EnableEffects()
		{
			if (_depthOfField != null) _depthOfField.active = true;
			if (_vignette != null) _vignette.active = true;
			if (_chromaticAberration != null) _chromaticAberration.active = true;
		}

		private void DisableEffects()
		{
			if (_depthOfField != null) _depthOfField.active = false;
			if (_vignette != null) _vignette.active = false;
			if (_chromaticAberration != null) _chromaticAberration.active = false;
		}
	}
}
