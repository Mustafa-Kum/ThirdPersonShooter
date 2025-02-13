using UnityEngine;

namespace PlayerScripts
{
    public class PlayerMovementCamera : MonoBehaviour
    {
        [SerializeField] private Transform targetTransform; // Kameranın takip edeceği transform
        [SerializeField] private float lerpSpeed = 5f;       // Lerp hızını kontrol eder

        private void Update()
        {
            if (IsTargetAvailable())
            {
                UpdateCameraPosition();
            }
        }

        /// <summary>
        /// Hedefin geçerli olup olmadığını kontrol eder.
        /// </summary>
        private bool IsTargetAvailable()
        {
            return targetTransform != null;
        }

        /// <summary>
        /// Kameranın pozisyonunu hedefin pozisyonuna doğru yumuşak bir şekilde hareket ettirir.
        /// </summary>
        private void UpdateCameraPosition()
        {
            transform.position = Vector3.Lerp(transform.position, targetTransform.position, Time.deltaTime * lerpSpeed);
        }
    }
}