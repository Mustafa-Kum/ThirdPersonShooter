using Cinemachine;
using Manager;
using UnityEngine;

public class PlayerWeaponScreenShake : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private float recoverySpeed = 1f;
    [SerializeField] private AnimationCurve shakeIntensityCurve;

    private CinemachineBasicMultiChannelPerlin noise;
    private float currentShakeTime;
    private float currentIntensity;

    private void Awake()
    {
        InitializeCameraComponents();
    }

    private void InitializeCameraComponents()
    {
        noise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        if (noise != null)
        {
            noise.m_AmplitudeGain = 0f;
        }
    }

    private void OnEnable()
    {
        EventManager.PlayerEvents.PlayerWeaponCameraShake += HandleCameraShake;
    }

    private void OnDisable()
    {
        EventManager.PlayerEvents.PlayerWeaponCameraShake -= HandleCameraShake;
    }

    private void Update()
    {
        UpdateCameraShake();
    }

    private void HandleCameraShake(float intensity, float duration)
    {
        currentIntensity = intensity;
        currentShakeTime = duration;
        ApplyShake();
    }

    private void UpdateCameraShake()
    {
        if (currentShakeTime <= 0) return;

        currentShakeTime -= Time.deltaTime * recoverySpeed;

        if (noise != null)
        {
            noise.m_AmplitudeGain = currentIntensity * shakeIntensityCurve.Evaluate(currentShakeTime);
        }

        if (currentShakeTime <= 0 && noise != null)
        {
            noise.m_AmplitudeGain = 0f;
        }
    }

    private void ApplyShake()
    {
        if (noise != null)
        {
            noise.m_AmplitudeGain = currentIntensity;
            noise.m_FrequencyGain = Random.Range(1.5f, 2.5f);
        }
    }
}