using System.Collections;
using UnityEngine;
using static FantasyShooter.Constants;

namespace FantasyShooter
{
    public class SpeedReckoner : MonoBehaviour
    {
        [SerializeField] private float _updateDelay;
        [Range(0, 0.99f)]
        [SerializeField] private float _speedChangeSmoothness;

        private float _rockonedSpeed;
        private float _targetReckonedSpeed;

        public float ReckonedSpeed => _rockonedSpeed;

        private void OnEnable()
        {
            StartCoroutine(StartSpeedReckoningCycle());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }

        private IEnumerator StartSpeedReckoningCycle()
        {
            YieldInstruction delay = new WaitForSeconds(_updateDelay);
            Vector3 lastPosition = transform.position;
            float lastTimestamp = Time.time;

            while (enabled)
            {
                yield return delay;

                var deltaPosition = (transform.position - lastPosition).magnitude;
                var deltaTime = Time.time - lastTimestamp;

                if (Mathf.Approximately(deltaPosition, 0f))
                    deltaPosition = 0f;

                _targetReckonedSpeed = deltaPosition / deltaTime;

                lastPosition = transform.position;
                lastTimestamp = Time.time;
            }
        }

        private void Update()
        {
            _rockonedSpeed = 
                Mathf.Lerp(
                    _rockonedSpeed, 
                    _targetReckonedSpeed, 
                    (1f - _speedChangeSmoothness) * DeltaTimeCorrection);
        }
    }
}