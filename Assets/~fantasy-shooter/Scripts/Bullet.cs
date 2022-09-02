using System;
using UnityEngine;
using static FantasyShooter.Constants;


namespace FantasyShooter
{
    public class Bullet : MonoBehaviour
    {
        public event Action<Bullet> Hit;
        public event Action<Bullet> OutOfTheScreen;

        [SerializeField] private Renderer _renderer;
        [SerializeField] private TrailRenderer _trailRenderer;


        public float Speed { get => _speed; set => _speed = value; }

        //[SerializeField] private Camera _mainCamera;


        private float _speed;
        private bool _isOutOfTheScreen = false;

        private void Update()
        {
            transform.position += _speed * DeltaTimeCorrection * transform.forward;
        }

        private void OnBecameInvisible()
        {
            if (_isOutOfTheScreen) return;
            _isOutOfTheScreen = true;
            OutOfTheScreen?.Invoke(this);
        }

        private void OnEnable()
        {

        }

        private void OnDisable()
        {
            _isOutOfTheScreen = false;
            _trailRenderer.Clear();
        }
    }
}