using UnityEngine;

public class Item_Animasyon : MonoBehaviour
{
    [SerializeField] private float amplitude = 0.5f;      // Yukarı aşağı hareket mesafesi
    [SerializeField] private float frequency = 1f;        // Hareket hızı (frekans)
    [SerializeField] private float rotationSpeed = 90f;   // Y ekseni etrafında rotasyon hızı (derece/saniye)

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        HandleVerticalOscillation();
        HandleYAxisRotation();
    }

    // Sadece yukarı aşağı hareketten sorumlu metod.
    private void HandleVerticalOscillation()
    {
        float newY = startPosition.y + Mathf.Sin(Time.time * frequency) * amplitude;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }

    // Sadece Y ekseni etrafında rotasyondan sorumlu metod.
    private void HandleYAxisRotation()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }
}
