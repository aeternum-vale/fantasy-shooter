using NaughtyAttributes;
using UnityEngine;
using Lean.Pool;

namespace FantasyShooter
{
    public class Game : MonoBehaviour
    {
        [SerializeField] Camera _mainCamera;
        [SerializeField] Enemy[] _enemyPrefabs;
        [SerializeField] Transform _enemiesParent;
        [SerializeField] Player _player;

        private void Awake()
        {
            Application.targetFrameRate = Constants.TargetFrameRate;
        }

        [Button]
        private void SpawnEnemy()
        {
            Enemy prefab = _enemyPrefabs[Random.Range(0, _enemyPrefabs.Length)];
            Vector3 position = _player.transform.position + (new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f))).normalized * _mainCamera.orthographicSize * 3;
            Enemy enemyInstance = LeanPool.Spawn(prefab, position, Quaternion.identity, _enemiesParent);
            enemyInstance.PlayerTransform = _player.transform;
        }
    }

}