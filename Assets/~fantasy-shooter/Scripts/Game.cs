using DG.Tweening;
using Lean.Pool;
using NaughtyAttributes;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FantasyShooter
{
    public class Game : MonoBehaviour
    {

        [SerializeField] private Camera _mainCamera;
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

        [Header("UI")]
        [SerializeField] private Image _healthBar;
        [SerializeField] private TMP_Text _gameOverLabel;

        private float _healthBarInitScaleX;
        private Tween _healthBarTween;


        private void Awake()
        {
            Application.targetFrameRate = Constants.TargetFrameRate;

            _player.Died += OnPlayerDied;

            _healthBarInitScaleX = _healthBar.rectTransform.localScale.x;
        }

        private void OnDestroy()
        {
            _healthBarTween?.Kill();
            LeanPool.DespawnAll();
        }

        private void OnPlayerDied()
        {
            _gameOverLabel.gameObject.SetActive(true);
            UpdateHealthBarImmediately();
            Time.timeScale = 0;
            StartCoroutine(WaitBeforeGameRestart());
        }

        private IEnumerator WaitBeforeGameRestart()
        {
            yield return new WaitForSecondsRealtime(5f);
            Time.timeScale = 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void Start()
        {
            StartCoroutine(StartSpawningCycle());
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

        private void OnEnemyDamagePlayer()
        {
            _player.Damage();

            UpdateHealthBarWithAnimation();
        }

        private void UpdateHealthBarWithAnimation()
        {
            _healthBarTween?.Kill();
            _healthBarTween = _healthBar.rectTransform.DOScaleX(_healthBarInitScaleX * _player.NormalizedHealth, 0.2f)
                .SetEase(Ease.InQuad);
        }

        private void UpdateHealthBarImmediately()
        {
            _healthBarTween?.Kill();
            _healthBar.rectTransform.localScale = _healthBar.rectTransform.localScale.WithX(_healthBarInitScaleX * _player.NormalizedHealth);
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