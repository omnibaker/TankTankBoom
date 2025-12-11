using UnityEngine;
using System.Collections;

namespace Sumfulla.TankTankBoom
{
	public class CameraShaker : MonoBehaviour
	{
		[SerializeField] private Transform _camTransform;
        [SerializeField] private float _shakeDuration = 10f;

        // Amplitude of the shake. A larger value shakes the camera harder.
        [SerializeField] private float _shakeAmount = 0.7f;
        [SerializeField] private float _decreaseFactor = 1.0f;

        private Coroutine _shaking;
        private Vector3 _originalPos;

		private void Awake()
		{
			if (_camTransform == null)
			{
				Camera cam = FindFirstObjectByType<Camera>();
				if (cam != null)
				{
					_camTransform = cam.transform;
				}
			}
		}

        private void OnEnable()
		{
			_originalPos = _camTransform.localPosition;
		}

		/// <summary>
		/// Stops any coroutine currently running, and triggers new shake action
		/// </summary>
        public void StartShake()
		{
			if(_shaking != null)
			{
				StopCoroutine(_shaking);
			}

			_shaking = StartCoroutine(ShakeItUp());
        }

        /// <summary>
        /// Shakes camera by repositioning it rapidly at random points within a sphere
        /// </summary>
        public IEnumerator ShakeItUp()
        {
            _originalPos = _camTransform.localPosition;

			float t = 0;

			while (t < 1f)
			{
                _camTransform.localPosition = _originalPos + Random.insideUnitSphere * _shakeAmount;

				t += Time.deltaTime / _shakeDuration * _decreaseFactor;

				yield return null;
            }

            _camTransform.localPosition = _originalPos;
        }
	}
}