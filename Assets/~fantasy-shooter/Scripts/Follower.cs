using UnityEngine;
using static FantasyShooter.Constants;

namespace FantasyShooter
{
    public class Follower : MonoBehaviour
    {
        [SerializeField] private Transform _target;

        [Range(0, 0.99f)]
        [SerializeField] private float _smoothness;

        private Vector3 _offset;
        private Vector3 _targetPosition;



        private void Awake()
        {
            _offset = transform.position - _target.position;
        }

        private void Update()
        {
            _targetPosition = _target.position + _offset;
            transform.position = Vector3.Slerp(transform.position, _targetPosition, (1f - _smoothness) * DeltaTimeCorrection);
        }
    }

}
