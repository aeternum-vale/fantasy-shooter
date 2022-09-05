using Lean.Pool;
using NaughtyAttributes;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FantasyShooter
{
    public class Game : MonoBehaviour
    {
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private UIController _uiController;
        [SerializeField] private Player _player;
        [SerializeField] private Enemy[] _enemyPrefabs;
        [SerializeField] private Transform _enemiesParent;
        [SerializeField] private float _enemySpawnInterval;
        [SerializeField] private float _enemySpawnIntervalDecreaseSpeed = 0.01f;
        [SerializeField] private float _minEnemySpawnInterval = 0.1f;
        [SerializeField] private int _maxEnemiesCount;
        [ReadOnly]
        [SerializeField] private int _enemiesCount;
        [SerializeField] private bool _isSpawningOn = true;

        private void Awake()
        {
            Application.targetFrameRate = Constants.TargetFrameRate;
            _player.Died += OnPlayerDied;
        }

        private void Start()
        {
            StartCoroutine(StartSpawningCycle());
        }

        private void OnDestroy()
        {
            _player.Died -= OnPlayerDied;
            StopAllCoroutines();
            LeanPool.DespawnAll();
        }

        private void OnPlayerDied()
        {
            _uiController.ShowGameOverMessage();
            _uiController.
                SetHealthImmediately(_player.NormalizedHealth);
            Time.timeScale = 0;
            StartCoroutine(WaitBeforeGameRestart());
        }

        private IEnumerator WaitBeforeGameRestart()
        {
            yield return new WaitForSecondsRealtime(5f);
            Time.timeScale = 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private IEnumerator StartSpawningCycle()
        {
            while (enabled)
            {
                yield return new WaitForSeconds(_enemySpawnInterval);
                SpawnEnemy();
            }
        }

        [Button]
        private void SpawnEnemy()
        {
            if (!_isSpawningOn) return;
            if (_enemiesCount >= _maxEnemiesCount) return;

            Enemy prefab = _enemyPrefabs[Random.Range(0, _enemyPrefabs.Length)];
            Vector3 position = _player.transform.position + _mainCamera.orthographicSize * 3 * new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
            Enemy enemyInstance = LeanPool.Spawn(prefab, position, Quaternion.identity, _enemiesParent);
            enemyInstance.PlayerTransform = _player.transform;
            AddListenersOnEnemy(enemyInstance);
            _enemiesCount++;
        }

        private void DespawnEnemy(Enemy enemy)
        {
            RemoveListenersFromEnemy(enemy);
            LeanPool.Despawn(enemy);
            _enemiesCount--;
        }

        private void AddListenersOnEnemy(Enemy enemy)
        {
            enemy.Decommissioned += OnDecommissioned;
            enemy.DamagePlayer += OnEnemyDamagePlayer;
        }

        private void RemoveListenersFromEnemy(Enemy enemy)
        {
            enemy.DamagePlayer -= OnEnemyDamagePlayer;
            enemy.Decommissioned -= OnDecommissioned;
        }

        private void OnEnemyDamagePlayer(Enemy enemy)
        {
            _player.Damage(enemy.DamageAmount);

            _uiController.
                SetHealthWithAnimation(_player.NormalizedHealth);
        }

        private void OnDecommissioned(Enemy enemy)
        {
            DespawnEnemy(enemy);
        }

        private void Update()
        {
            if (_enemySpawnInterval > _minEnemySpawnInterval)
                _enemySpawnInterval -= Time.deltaTime * _enemySpawnIntervalDecreaseSpeed;
            if (_enemySpawnInterval <= _minEnemySpawnInterval)
                _enemySpawnInterval = _minEnemySpawnInterval;
        }
    }
}