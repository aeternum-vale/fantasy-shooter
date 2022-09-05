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
        [SerializeField] private Transform _aimPlane;
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
        private Vector3 _aimPoint;
        private Vector2 _animationMoveCoords;
        private Vector2 _targetAnimationMoveCoords;
        private float _targetRotation = 0f;
        private float _rotationVelocity;
        private float _shootingTime = 0f;

        private int _animIDMoveX;
        private int _animIDMoveY;

        private const float RotationThresholdForAimCorrection = 0.3f;

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
            if (!_input.Shoot)
            {
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

            if (Mathf.Abs(_targetRotation - transform.rotation.y) <= RotationThresholdForAimCorrection)
                bullet.transform.forward = (_aimPoint - _gunTip.position).normalized;

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
            CalculateAimPoint();

            Vector3 lookDir = (_aimPoint - transform.position).normalized;
            _targetRotation = Mathf.Atan2(lookDir.x, lookDir.z) * Mathf.Rad2Deg + _additionalAngle;

            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                _rotationSmoothness * DeltaTimeCorrection);

            transform.rotation = Quaternion.Euler(0f, rotation, 0f);
        }

        private void CalculateAimPoint()
        {
            Vector3 screenPoint = _mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector3 directionVector = _mainCamera.transform.forward;
            float aimPlaneY = _aimPlane.transform.position.y;

            float t = (aimPlaneY - screenPoint.y) / directionVector.y;
            float aimPointX = directionVector.x * t + screenPoint.x;
            float aimPointZ = directionVector.z * t + screenPoint.z;

            _aimPoint = new Vector3(aimPointX, aimPlaneY, aimPointZ);
        }

        public void Damage(float damageAmount)
        {
            _health -= damageAmount;
            if (_health <= 0)
            {
                _health = 0;
                Died?.Invoke();
            }
        }
    }
}