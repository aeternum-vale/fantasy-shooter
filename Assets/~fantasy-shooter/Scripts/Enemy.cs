using UnityEngine;

using static FantasyShooter.Constants;
public class Enemy : MonoBehaviour
{

    [SerializeField] private Animator _animator;
    [SerializeField] Transform _target;
    [Range(0, 0.99f)]
    [SerializeField] private float _rotateSmoothness = 0.5f;
    [SerializeField] private float _speed;

    private bool _isKilled = false;
    private Vector3 _targetPoint;

    private int _killTriggerID;
    private int _deathVariantID;

    private const int DeathVariantsCount = 4;

    private void Awake()
    {
        AssignAnimationIDs();
        _animator.SetInteger(_deathVariantID, Random.Range(1, DeathVariantsCount + 1));
    }

    private void AssignAnimationIDs()
    {
        _killTriggerID = Animator.StringToHash("Kill");
        _deathVariantID = Animator.StringToHash("DeathVariant");
    }

    private void Update()
    {
        if (_isKilled) return;

        _targetPoint = Vector3.Slerp(_targetPoint, _target.position, (1f - _rotateSmoothness) * DeltaTimeCorrection);
        transform.LookAt(_targetPoint);

        transform.position += _speed * Time.deltaTime * transform.forward;
    }

    public void Kill()
    {
        _isKilled = true;
        _animator.SetTrigger(_killTriggerID);
    }


}
