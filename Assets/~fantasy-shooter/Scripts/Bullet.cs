using UnityEngine;
using static FantasyShooter.Constants;


namespace FantasyShooter
{
    public class Bullet : MonoBehaviour
    {
        private float _speed;

        public float Speed { get => _speed; set => _speed = value; }

        private void Update()
        {
            transform.position += _speed * DeltaTimeCorrection * transform.forward;
        }
    }
}