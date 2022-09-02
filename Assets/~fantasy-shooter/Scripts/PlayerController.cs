using Gamelogic.Extensions;
using UnityEngine;
using UnityEngine.InputSystem;
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

        [SerializeField] private float _speedChangeRate = 10f;

        [Range(0, 0.99f)]
        [SerializeField] private float _speedSmoothness;

        [Header("Player Grounded")]
        [SerializeField] private bool _grounded = true;

        [SerializeField] private float _groundedOffset = -0.14f;
        [SerializeField] private float _groundedRadius = 0.28f;
        [SerializeField] private LayerMask _groundLayers;

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

        private float DeltaTimeCorrection => Time.deltaTime * TargetFrameRate;

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

            _animationMoveCoords = Vector2.Lerp(_animationMoveCoords, _targetAnimationMoveCoords, (1f - _speedSmoothness) * DeltaTimeCorrection);
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
            _targetRotation = Mathf.Atan2(lookDir.x, lookDir.z) * Mathf.Rad2Deg;

            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                _rotationSmoothness * DeltaTimeCorrection);

            transform.rotation = Quaternion.Euler(0f, rotation, 0f);
        }
    }
}