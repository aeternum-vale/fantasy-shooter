using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FantasyShooter
{
    public class UIController : MonoBehaviour
    {
        [SerializeField] private Image _healthBar;
        [SerializeField] private TMP_Text _gameOverLabel;

        private float _healthBarInitScaleX;
        private Tween _healthBarTween;

        private void Awake()
        {
            _healthBarInitScaleX = _healthBar.rectTransform.localScale.x;
        }

        private void OnDestroy()
        {
            _healthBarTween?.Kill();
        }

        public void ShowGameOverMessage()
        {
            _gameOverLabel.gameObject.SetActive(true);
        }

        public void SetHealthWithAnimation(float normalizedHealth)
        {
            _healthBarTween?.Kill();
            _healthBarTween = _healthBar.rectTransform.DOScaleX(_healthBarInitScaleX * normalizedHealth, 0.2f)
                .SetEase(Ease.InQuad);
        }

        public void SetHealthImmediately(float normalizedHealth)
        {
            _healthBarTween?.Kill();
            _healthBar.rectTransform.localScale = _healthBar.rectTransform.localScale.WithX(_healthBarInitScaleX * normalizedHealth);
        }
    }
}