using System.Collections;
using Cinemachine;
using Manager;
using UnityEngine;

namespace PlayerScripts
{
    public class PlayerFOVZoom : MonoBehaviour
    {
        private Coroutine currentZoomCoroutine;

        private void OnEnable()
        {
            EventManager.PlayerEvents.PlayerFOVZoomRoutine += SmoothZoomTransitionEvent;
        }

        private void OnDisable()
        {
            EventManager.PlayerEvents.PlayerFOVZoomRoutine -= SmoothZoomTransitionEvent;
        }

        private void SmoothZoomTransitionEvent(float targetFOV, float duration, CinemachineVirtualCamera thirdPersonCamera)
        {
            // Eğer hâlihazırda çalışmakta olan bir coroutine varsa durduruyoruz
            if (currentZoomCoroutine != null)
            {
                StopCoroutine(currentZoomCoroutine);
            }

            // Yeni değerler ile yeni coroutine başlat
            currentZoomCoroutine = StartCoroutine(SmoothZoomTransition(targetFOV, duration, thirdPersonCamera));
        }

        private IEnumerator SmoothZoomTransition(float targetFOV, float duration, CinemachineVirtualCamera thirdPersonCamera)
        {
            float startFOV = thirdPersonCamera.m_Lens.FieldOfView;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                thirdPersonCamera.m_Lens.FieldOfView = Mathf.Lerp(startFOV, targetFOV, t);
                yield return null;
            }

            // Son olarak hedef FOV değerini tam olarak ayarla
            thirdPersonCamera.m_Lens.FieldOfView = targetFOV;

            // Bittiğinde referansı sıfırlayabilirsiniz
            currentZoomCoroutine = null;
        }
    }
}