using MonsterLove.StateMachine;
using System;
using UnityEngine;
using Random = UnityEngine.Random;
using static FantasyShooter.Constants;


namespace FantasyShooter
{
    [RequireComponent(typeof(SpeedReckoner))]
    public class Enemy : MonoBehaviour
    {
        private enum EState
        {
            Move,
            Attack,
            Death
        }

        public event Action DamagePlayer;

        [SerializeField] private Animator _animator;
        [SerializeField] private Rigidbody _rigidBody;
        [SerializeField] private Collider _collider;
        [Space]
        [Range(0, 0.99f)]
        [SerializeField] private float _rotateSmoothness = 0.5f;
        [SerializeField] private float _speed;
        [SerializeField] private float _attackDistance;
        [SerializeField] private float _attackTimeForDamage;

        private StateMachine<EState> _fsm;

        private Transform _playerTransform;
        private SpeedReckoner _speedReckoner;

        private bool _isDead = false;
        private Vector3 _targetPoint;

        private int _deathTriggerID;
        private int _deathVariantID;
        private int _attackFlagID;
        private int _speedID;

        private float _attackingTime;

        private const string MoveState = "Moving";
        private const int DeathVariantsCount = 4;

        public Transform PlayerTransform { get => _playerTransform; set => _playerTransform = value; }

        private void Awake()
        {
            _speedReckoner = GetComponent<SpeedReckoner>();

            _fsm = new StateMachine<EState>(this);
            _fsm.ChangeState(EState.Move);

            AssignAnimationIDs();

            _animator.SetInteger(_deathVariantID, Random.Range(1, DeathVariantsCount + 1));
        }

        private void Start()
        {
            _animator.Play(MoveState, -1, Random.Range(0f, 1f));
        }

        private void AssignAnimationIDs()
        {
            _deathTriggerID = Animator.StringToHash("DeathTrigger");
            _attackFlagID = Animator.StringToHash("AttackFlag");
            _deathVariantID = Animator.StringToHash("DeathVariant");
            _speedID = Animator.StringToHash("Speed");
        }

        private void Update()
        {
            _fsm.Driver.Update.Invoke();
        }

        public void Kill()
        {
            _fsm.ChangeState(EState.Death);
        }

        private void RotateTowardTarget()
        {
            _targetPoint = Vector3.Slerp(_targetPoint, _playerTransform.position, (1f - _rotateSmoothness) * DeltaTimeCorrection);
            transform.LookAt(_targetPoint);
        }

        private void UpdateAttackingTime()
        {
            _attackingTime += Time.deltaTime;
            if (_attackingTime >= _attackTimeForDamage)
            {
                _attackingTime = 0f;
                DamagePlayer?.Invoke();
            }
        }

        #region FSM

        private void Move_Enter()
        {
            _animator.SetBool(_attackFlagID, false);
        }

        private void Move_Update()
        {
            RotateTowardTarget();

            Vector3 newPosition = transform.position + _speed * Time.deltaTime * transform.forward;
            _rigidBody.MovePosition(newPosition);

            if (Vector3.Distance(transform.position, _playerTransform.position) <= _attackDistance)
                _fsm.ChangeState(EState.Attack);
            else
                SetAnimatorSpeedValue();
        }

        private void SetAnimatorSpeedValue()
        {
            _animator.SetFloat(_speedID, Mathf.Clamp(_speedReckoner.ReckonedSpeed / (_speed * 0.65f), 0f, 1f));
        }

        private void Death_Enter()
        {
            _isDead = true;
            _collider.enabled = false;
            _rigidBody.isKinematic = true;
            _animator.SetTrigger(_deathTriggerID);
        }

        private void Attack_Enter()
        {
            _attackingTime = 0;
            _animator.SetBool(_attackFlagID, true);
        }

        private void Attack_Update()
        {
            UpdateAttackingTime();

            RotateTowardTarget();

            if (Vector3.Distance(transform.position, _playerTransform.position) > _attackDistance)
                _fsm.ChangeState(EState.Move);
        }

        #endregion

    }
}