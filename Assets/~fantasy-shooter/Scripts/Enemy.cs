using MonsterLove.StateMachine;
using UnityEngine;

using static FantasyShooter.Constants;

public class Enemy : MonoBehaviour
{
    private enum States
    {
        Move,
        Attack,
        Death
    }

    [SerializeField] private Animator _animator;
    [SerializeField] Transform _target;
    [SerializeField] Rigidbody _rigidBody;
    [SerializeField] Collider _collider;
    [Range(0, 0.99f)]
    [SerializeField] private float _rotateSmoothness = 0.5f;
    [SerializeField] private float _speed;
    [SerializeField] private float _attackDistance;

    private StateMachine<States> _fsm;

    private bool _isDead = false;
    private Vector3 _targetPoint;

    private int _deathTriggerID;
    private int _deathVariantID;
    private int _attackFlagID;

    private const string MoveState = "Moving";
    private const int DeathVariantsCount = 4;

    private void Awake()
    {
        _fsm = new StateMachine<States>(this);
        _fsm.ChangeState(States.Move);

        AssignAnimationIDs();

        _animator.SetInteger(_deathVariantID, Random.Range(1, DeathVariantsCount + 1));
    }

    private void AssignAnimationIDs()
    {
        _deathTriggerID = Animator.StringToHash("DeathTrigger");
        _attackFlagID = Animator.StringToHash("AttackFlag");
        _deathVariantID = Animator.StringToHash("DeathVariant");
    }

    private void Update()
    {
        _fsm.Driver.Update.Invoke();
    }

    public void Kill()
    {
        _fsm.ChangeState(States.Death);
    }

    private void RotateTowardTarget()
    {
        _targetPoint = Vector3.Slerp(_targetPoint, _target.position, (1f - _rotateSmoothness) * DeltaTimeCorrection);
        transform.LookAt(_targetPoint);
    }

    #region FSM

    private void Move_Enter()
    {
        _animator.SetBool(_attackFlagID, false);

        _animator.Play(MoveState, -1, Random.Range(0f, 1f));
    }

    private void Move_Update()
    {
        RotateTowardTarget();

        _rigidBody.MovePosition(transform.position + _speed * Time.deltaTime * transform.forward);
        if (Vector3.Distance(transform.position, _target.position) <= _attackDistance)
            _fsm.ChangeState(States.Attack);
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
        _animator.SetBool(_attackFlagID, true);
    }

    private void Attack_Update()
    {
        RotateTowardTarget();
        if (Vector3.Distance(transform.position, _target.position) > _attackDistance)
            _fsm.ChangeState(States.Move);
    }

    #endregion

}
