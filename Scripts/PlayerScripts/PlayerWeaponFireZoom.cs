using System.Collections;
using Cinemachine;
using Manager;
using UnityEngine;

public class PlayerWeaponFireZoom : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private AnimationCurve lerpCurve;

    private void OnEnable()
    {
        EventManager.PlayerEvents.PlayerWeaponFireZoomRoutine += HandleCameraZoom;
    }

    private void OnDisable()
    {
        EventManager.PlayerEvents.PlayerWeaponFireZoomRoutine -= HandleCameraZoom;
    }

    private void HandleCameraZoom(float newMultiplier, float duration, float incomingFOV)
    {
        // Zoom işlemini başlat
        StartCoroutine(PerformCameraZoom(newMultiplier ,duration, incomingFOV));
    }

    private IEnumerator PerformCameraZoom(float newMultiplier, float duration, float incomingFOV)
    {
        float targetFOV = incomingFOV * newMultiplier; // Hedef FOV
        float initialFOV = incomingFOV; // Başlangıç FOV
        float elapsed = 0f;
        float halfDuration = duration * 0.5f; // Git ve gel süreleri için toplam sürenin yarısı

        // Git: FOV'u başlangıçtan hedef FOV'a doğru zoom yap
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            float curveValue = lerpCurve.Evaluate(t);
            virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(initialFOV, targetFOV, curveValue);
            yield return null;
        }

        // Gel: FOV'u hedef FOV'dan başlangıç FOV'una geri döndür
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            float curveValue = lerpCurve.Evaluate(t);
            virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(targetFOV, initialFOV, curveValue);
            yield return null;
        }
    }

}
