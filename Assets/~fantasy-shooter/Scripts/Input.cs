using UnityEngine;
using UnityEngine.InputSystem;


namespace FantasyShooter
{
    public class Input : MonoBehaviour
    {
        public Vector2 Move { get; set; }
        public bool Sprint { get; set; }
        public bool Shoot { get; set; }


        private void OnMove(InputValue value)
        {
            Move = value.Get<Vector2>();
        }

        private void OnSprint(InputValue value)
        {
            Sprint = value.isPressed;
        }

        private void OnShoot(InputValue value)
        {
            Shoot = value.isPressed;
        }


    }

}