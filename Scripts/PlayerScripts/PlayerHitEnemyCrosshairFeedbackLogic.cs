using UnityEngine;
using System.Collections;
using Manager;
using UnityEngine.UI;

namespace Logic
{
    public enum HitArea
    {
        Head,
        Body,
        Death
    }

    public class PlayerHitEnemyCrosshairFeedbackLogic : MonoBehaviour
    {
        [SerializeField] private Vector2 _crosshairStarterPosition; 
        [SerializeField] private Vector2 _crosshairEnderPosition;
        [SerializeField] private HitArea _targetHitArea; // Hangi vuruş alanını dinleyeceğini belirleyen değişken

        private RectTransform _crosshairRectTransform;
        private Image _crosshairImage; 
        private Coroutine _animationCoroutine;

        private void OnEnable()
        {
            EventManager.PlayerEvents.PlayerHitEnemyCrosshairFeedBack += OnPlayerHitEnemyCrosshairFeedBack;
        }

        private void OnDisable()
        {
            EventManager.PlayerEvents.PlayerHitEnemyCrosshairFeedBack -= OnPlayerHitEnemyCrosshairFeedBack;
        }

        private void Start()
        {
            _crosshairRectTransform = transform.GetComponent<RectTransform>();
            _crosshairImage = transform.GetComponent<Image>();
            
            // Başlangıçta alpha = 0 ve GameObject ACTIVE olarak kalıyor
            Color initialColor = _crosshairImage.color;
            initialColor.a = 0f;
            _crosshairImage.color = initialColor;
        }

        private void OnPlayerHitEnemyCrosshairFeedBack(bool isHit, HitArea hitArea)
        {
            // Sadece belirtilen hitArea'ya sahipse tepki ver
            if (isHit && hitArea == _targetHitArea)
            {
                if (_animationCoroutine != null)
                {
                    StopCoroutine(_animationCoroutine);
                }
                _animationCoroutine = StartCoroutine(AnimateCrosshair());
            }
            else if (!isHit)
            {
                if (_animationCoroutine != null)
                {
                    StopCoroutine(_animationCoroutine);
                    _animationCoroutine = null;
                }
                // Alpha'yı direkt 0 yap ve pozisyonu resetle
                SetCrosshairAlpha(0f);
                _crosshairRectTransform.anchoredPosition = _crosshairStarterPosition;
            }
        }

        private IEnumerator AnimateCrosshair()
        {
            // Alpha = 1 yap (görünür)
            SetCrosshairAlpha(1f);
            
            Vector2 startPos = _crosshairStarterPosition;
            Vector2 endPos = _crosshairEnderPosition;
            float duration = 0.2f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                _crosshairRectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            _crosshairRectTransform.anchoredPosition = endPos;
            yield return new WaitForSeconds(0.1f); 
            
            // Alpha'yı 0 yap (görünmez)
            SetCrosshairAlpha(0f);
            _crosshairRectTransform.anchoredPosition = startPos;
            _animationCoroutine = null;
        }

        // Alpha değişimini merkezileştiren yardımcı fonksiyon
        private void SetCrosshairAlpha(float alpha)
        {
            Color color = _crosshairImage.color;
            color.a = alpha;
            _crosshairImage.color = color;
        }
    }
}