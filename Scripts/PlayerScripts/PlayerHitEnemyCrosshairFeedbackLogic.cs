using UnityEngine;
using System.Collections;
using System.Collections.Generic;
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
        
        // Tüm crosshair instance'larını takip etmek için statik liste
        private static List<PlayerHitEnemyCrosshairFeedbackLogic> _activeCrosshairs = new List<PlayerHitEnemyCrosshairFeedbackLogic>();
        // Death animasyonu oynatıldığında diğerlerini durdurmak için statik flag
        private static bool _isDeathPlaying = false;

        private void OnEnable()
        {
            EventManager.PlayerEvents.PlayerHitEnemyCrosshairFeedBack += OnPlayerHitEnemyCrosshairFeedBack;
            _activeCrosshairs.Add(this);
        }

        private void OnDisable()
        {
            EventManager.PlayerEvents.PlayerHitEnemyCrosshairFeedBack -= OnPlayerHitEnemyCrosshairFeedBack;
            _activeCrosshairs.Remove(this);
            
            // Eğer bu Death crosshair'i ise ve devreden çıkıyorsa, flag'i resetle
            if (_targetHitArea == HitArea.Death)
            {
                _isDeathPlaying = false;
            }
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
            // Eğer Death oynatılıyorsa ve bu Death değilse hiçbir şey yapma
            if (_isDeathPlaying && _targetHitArea != HitArea.Death)
                return;
                
            // Death tetiklendiğinde diğer tüm crosshair'leri durdur
            if (isHit && hitArea == HitArea.Death && _targetHitArea != HitArea.Death)
                return;
                
            // Death crosshair'inin tetiklenmesi durumu
            if (isHit && hitArea == HitArea.Death && _targetHitArea == HitArea.Death)
            {
                // Diğer tüm crosshair'leri durdur
                StopAllOtherCrosshairs();
                _isDeathPlaying = true;
            }
                
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
                
                // Eğer bu Death crosshair'i ise ve duruyorsa, flag'i resetle
                if (_targetHitArea == HitArea.Death)
                {
                    _isDeathPlaying = false;
                }
            }
        }

        // Diğer tüm aktif crosshair'lerin animasyonlarını durdurur
        private void StopAllOtherCrosshairs()
        {
            foreach (var crosshair in _activeCrosshairs)
            {
                if (crosshair != this && crosshair._targetHitArea != HitArea.Death)
                {
                    // Diğer crosshair'in animasyonunu durdur
                    if (crosshair._animationCoroutine != null)
                    {
                        crosshair.StopCoroutine(crosshair._animationCoroutine);
                        crosshair._animationCoroutine = null;
                    }
                    
                    // Alpha'yı 0 yap ve pozisyonu resetle
                    crosshair.SetCrosshairAlpha(0f);
                    crosshair._crosshairRectTransform.anchoredPosition = crosshair._crosshairStarterPosition;
                }
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
            
            // Eğer bu Death crosshair'i ise ve animasyon bittiyse, flag'i resetle
            if (_targetHitArea == HitArea.Death)
            {
                _isDeathPlaying = false;
            }
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