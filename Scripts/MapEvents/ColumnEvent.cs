using System.Collections;
using UnityEngine;
using Manager;

public class ColumnEvent : MonoBehaviour
{
    [SerializeField] private float targetYPosition = -3.5f;
    [SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private ParticleSystem _particleSystem;

    private Coroutine moveCoroutine;

    private void OnEnable()
    {
        EventManager.MapEvents.ColumnSpawn += OnColumnSpawn;
    }

    private void OnDisable()
    {
        EventManager.MapEvents.ColumnSpawn -= OnColumnSpawn;
    }

    /// <summary>
    /// Event tetiklendiğinde kolonu hareket ettirmek için çağrılır.
    /// </summary>
    protected virtual void OnColumnSpawn()
    {
        // Önceden başlamış bir hareket varsa, iptal edelim.
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }
        moveCoroutine = StartCoroutine(MoveToTarget());
    }

    /// <summary>
    /// Kolonun hedef Y konumuna doğru hareket etmesini sağlayan coroutine.
    /// </summary>
    protected virtual IEnumerator MoveToTarget()
    {
        _particleSystem.Play();
        // 2 saniye bekle
        yield return new WaitForSeconds(1.5f);
        
        Vector3 targetPosition = new Vector3(transform.position.x, targetYPosition, transform.position.z);
        
        while (!Mathf.Approximately(transform.position.y, targetYPosition))
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        
            yield return null;
        }
        
        moveCoroutine = null;
    }
}
