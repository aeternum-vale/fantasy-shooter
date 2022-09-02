using UnityEngine;

namespace FantasyShooter
{
    public class Game : MonoBehaviour
    {
        private void Awake()
        {
            Application.targetFrameRate = Constants.TargetFrameRate;
        }
    }

}