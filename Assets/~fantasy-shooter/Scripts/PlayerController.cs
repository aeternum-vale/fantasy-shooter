using Gamelogic.Extensions;
using UnityEngine;
using UnityEngine.InputSystem;
using Lean.Pool;

using static FantasyShooter.Constants;


namespace FantasyShooter
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private Camera _mainCamera;

        [Header("Player")]
        [SerializeField] private float _moveSpeed = 2f;

        [SerializeField] private float _sprintSpeed = 3f;

        [Range(0f, 0.3f)]
        [SerializeField] private float _rotationSmoothness = 0.12f;
        [SerializeField] private float _additionalAngle;

        [SerializeField] private float _speedChangeRate = 10f;

        [Range(0, 0.99f)]
        [SerializeField] private float _speedSmoothness;



        [Header("Shooting")]
        [SerializeField] private Transform _gunTip;
        [SerializeField] private Bullet _bulletPrefab;
        [SerializeField] private Transform _bulletParent;
        [SerializeField] float _bulletSpeed = 1f;
        [SerializeField] float _bulletSpreadAngle = 1f;
        [SerializeField] ParticleSystem _gunFlash;


        private CharacterController _controller;
        private Input _input;
        private Animator _animator;

        private float _speed;
        private float _targetSpeed;
        private Vector3 _aimGroundPosition;
        private Vector2 _animationMoveCoords;
        private Vector2 _targetAnimationMoveCoords;
        private float _targetRotation = 0f;
        private float _rotationVelocity;

        private int _animIDMoveX;
        private int _animIDMoveY;


        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<Input>();
            AssignAnimationIDs();
        }

        private void Update()
        {
            UpdateRotation();
            UpdatePosition();
            UpdateAnimation();
            UpdateShooting();
        }

        private void UpdateShooting()
        {
            if (!_input.Shoot) return;

            var spreadRotation =
                Quaternion.Euler(
                    Random.Range(-_bulletSpreadAngle, _bulletSpreadAngle),
                    Random.Range(-_bulletSpreadAngle, _bulletSpreadAngle),
                    Random.Range(-_bulletSpreadAngle, _bulletSpreadAngle));

            var bullet = LeanPool.Spawn(_bulletPrefab, _gunTip.position, _gunTip.rotation * spreadRotation, _bulletParent);
            bullet.Speed = _bulletSpeed;

            _gunFlash.Play();
        }

        private void AssignAnimationIDs()
        {
            _animIDMoveX = Animator.StringToHash("MoveX");
            _animIDMoveY = Animator.StringToHash("MoveY");
        }

        private void UpdatePosition()
        {
            _targetSpeed = _input.Sprint ? _sprintSpeed : _moveSpeed;
            if (_input.Move == Vector2.zero) _targetSpeed = 0f;

            _speed = Mathf.Lerp(_speed, _targetSpeed, _speedSmoothness * DeltaTimeCorrection);

            var moveDistance = _speed / TargetFrameRate;
            Vector3 forward = _mainCamera.transform.forward.WithY(0f).normalized * _input.Move.y;
            Vector3 right = _mainCamera.transform.right * _input.Move.x;
            Vector3 targetDirection = (forward + right).normalized;

            _controller.Move(targetDirection * moveDistance);
        }

        private void UpdateAnimation()
        {
            _targetAnimationMoveCoords =
                Quaternion.Euler(0f, 0f, transform.localEulerAngles.y - _mainCamera.transform.localEulerAngles.y)
                    * _input.Move * _speed;

            _animationMoveCoords =
                Vector2.Lerp(_animationMoveCoords, _targetAnimationMoveCoords,
                    (1f - _speedSmoothness) * DeltaTimeCorrection);
            _animator.SetFloat(_animIDMoveX, _animationMoveCoords.x);
            _animator.SetFloat(_animIDMoveY, _animationMoveCoords.y);
        }

        private void UpdateRotation()
        {
            Vector3 start = _mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector3 end = start + _mainCamera.transform.forward;

            int layerMask = 1 << GroundLayer;
            if (Physics.Raycast(start, end - start, out RaycastHit hit, _mainCamera.farClipPlane, layerMask))
                _aimGroundPosition = hit.point;

            var lookDir = (_aimGroundPosition - transform.position).normalized;
            _targetRotation = Mathf.Atan2(lookDir.x, lookDir.z) * Mathf.Rad2Deg + _additionalAngle;

            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                _rotationSmoothness * DeltaTimeCorrection);

            transform.rotation = Quaternion.Euler(0f, rotation, 0f);
        }
    }
}