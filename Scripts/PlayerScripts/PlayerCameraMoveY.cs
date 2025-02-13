using System;
using System.Collections;
using Cinemachine;
using Manager;
using UnityEngine;

namespace PlayerScripts
{
    public class PlayerCameraMoveY : MonoBehaviour
    {
        private void OnEnable()
        {
            EventManager.PlayerEvents.PlayerCameraMoveY += CameraMoveY;
        }

        private void OnDisable()
        {
            EventManager.PlayerEvents.PlayerCameraMoveY -= CameraMoveY; }

        private void CameraMoveY(float targetY, float duration, CinemachineVirtualCamera _virtualCamera)
        {
            StartCoroutine(SmoothMoveCameraHeight(targetY, duration, _virtualCamera));
        }

        private IEnumerator SmoothMoveCameraHeight(float targetY, float duration, CinemachineVirtualCamera _virtualCamera)
        {
            Vector3 startPos = _virtualCamera.transform.position;
            Vector3 endPos = startPos;
            endPos.y = targetY; // Hedef y değeri

            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                // Lerp ile y koordinatını pürüzsüz bir şekilde değiştiriyoruz.
                _virtualCamera.transform.position = Vector3.Lerp(startPos, endPos, elapsedTime / duration);
                yield return null;
            }

            // İşlem sonunda pozisyonu tam olarak hedefe ayarla.
            _virtualCamera.transform.position = endPos;
        }
    }
}