using Lean.Pool;
using NaughtyAttributes;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static FantasyShooter.Constants;
using Random = UnityEngine.Random;

namespace FantasyShooter
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInput))]
    public class Player : MonoBehaviour
    {
        public event Action Died;

        [SerializeField] private Camera _mainCamera;

        [Header("Player")]
        [ReadOnly]
        [SerializeField] private float _health;
        [SerializeField] private float _healthTotal;
        [SerializeField] private float _decreasedHealthOnDamage;
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
        [SerializeField] private ParticleSystem _gunFlash;
        [SerializeField] private float _bulletSpeed = 1f;
        [SerializeField] private float _bulletSpreadAngle = 1f;
        [SerializeField] private float _shootingInterval = 0.01f;

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
        private float _shootingTime = 0f;

        private int _animIDMoveX;
        private int _animIDMoveY;

        public float NormalizedHealth => _health / _healthTotal;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<Input>();
            AssignAnimationIDs();
            _health = _healthTotal;
        }

        private void AssignAnimationIDs()
        {
            _animIDMoveX = Animator.StringToHash("MoveX");
            _animIDMoveY = Animator.StringToHash("MoveY");
        }

        private void Update()
        {
            if (Time.timeScale == 0) return;

            UpdateRotation();
            UpdatePosition();
            UpdateAnimation();
            UpdateShooting();
        }

        private void UpdateShooting()
        {
            if (!_input.Shoot) {
                _shootingTime = _shootingInterval;
                return;
            };

            _shootingTime += Time.deltaTime;

            if (_shootingTime < _shootingInterval) return;

            _shootingTime = 0f;

            float GetSpreadEulerAngle() => Random.Range(-_bulletSpreadAngle, _bulletSpreadAngle) * _speed;

            var spreadRotation =
                Quaternion.Euler(
                    GetSpreadEulerAngle(),
                    GetSpreadEulerAngle(),
                    GetSpreadEulerAngle());

            var bullet =
                LeanPool.Spawn(
                    _bulletPrefab,
                    _gunTip.position,
                    _gunTip.rotation * spreadRotation,
                    _bulletParent);

            if (Mathf.Approximately(_targetRotation, transform.rotation.y))
                bullet.transform.forward = (_aimGroundPosition - _gunTip.position).normalized;

            bullet.Speed = _bulletSpeed;
            AddListenersOnBullet(bullet);

            _gunFlash.Play();
        }

        private void AddListenersOnBullet(Bullet bullet)
        {
            bullet.OutOfView += OnBulletOutOfTheScreen;
            bullet.EnemyHit += OnEnemyHit;
        }

        private void RemoveListenersFromBullet(Bullet bullet)
        {
            bullet.OutOfView -= OnBulletOutOfTheScreen;
            bullet.EnemyHit -= OnEnemyHit;
        }

        private void OnEnemyHit(Bullet bullet, Enemy enemy)
        {
            DespawnBullet(bullet);
            enemy.Kill();
        }

        private void OnBulletOutOfTheScreen(Bullet bullet)
        {
            DespawnBullet(bullet);
        }

        private void DespawnBullet(Bullet bullet)
        {
            RemoveListenersFromBullet(bullet);
            LeanPool.Despawn(bullet);
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

            int layerMask = 1 << AimPlaneLayer;
            if (Physics.Raycast(start, end - start, out RaycastHit hit, _mainCamera.farClipPlane, layerMask))
                _aimGroundPosition = hit.point;

            Vector3 lookDir = (_aimGroundPosition - transform.position).normalized;
            _targetRotation = Mathf.Atan2(lookDir.x, lookDir.z) * Mathf.Rad2Deg + _additionalAngle;

            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                _rotationSmoothness * DeltaTimeCorrection);

            transform.rotation = Quaternion.Euler(0f, rotation, 0f);
        }

        public void Damage()
        {
            _health -= _decreasedHealthOnDamage;
            if (_health <= 0)
            {
                _health = 0;
                Died?.Invoke();
            }
        }
    }
}