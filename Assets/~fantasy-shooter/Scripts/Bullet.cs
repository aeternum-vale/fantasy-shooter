using System;
using UnityEngine;
using static FantasyShooter.Constants;

namespace FantasyShooter
{
    public class Bullet : MonoBehaviour
    {
        public event Action<Bullet, Enemy> EnemyHit;
        public event Action<Bullet> OutOfView;

        [SerializeField] private Renderer _renderer;
        [SerializeField] private TrailRenderer _trailRenderer;

        private float _speed;

        public float Speed { get => _speed; set => _speed = value; }

        private void Update()
        {
            transform.position += _speed * DeltaTimeCorrection * transform.forward;
        }

        private void OnBecameInvisible()
        {
            OutOfView?.Invoke(this);
        }

        private void OnDisable()
        {
            _trailRenderer.Clear();
        }

        private void OnTriggerEnter(Collider other)
        {
            EnemyHit?.Invoke(this, other.GetComponentInParent<Enemy>());
        }
    }
}