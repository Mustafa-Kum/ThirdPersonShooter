using UnityEngine;
using Lean.Pool;
using System.Collections.Generic;
using Manager;

namespace EnemySystems
{
    public class EnemyDropController : MonoBehaviour
    {
        [Header("Drop Settings")]
        [SerializeField] private List<GameObject> _dropItems = new List<GameObject>();
        [SerializeField, Range(0f, 1f)] private float _dropChance = 0.3f;
        [SerializeField, Range(0f, 5f)] private float _dropUpForce = 2f;
        [SerializeField, Range(0f, 5f)] private float _dropOutForce = 1f;

        [Header("Spawn Effects")]
        [SerializeField] private GameObject _spawnParticle;
        [SerializeField] private bool _useSpawnParticle = true;

        private void OnEnable()
        {
            EventManager.EnemySpawnEvents.EnemyDied += HandleEnemyDied;
        }

        private void OnDisable()
        {
            EventManager.EnemySpawnEvents.EnemyDied -= HandleEnemyDied;
        }

        private void HandleEnemyDied(GameObject enemy)
        {
            if (!IsCurrentEnemy(enemy)) return;

            if (!ShouldDropItem()) return;

            GameObject selectedItem = SelectRandomItem();
            if (selectedItem == null) return;

            GameObject droppedItem = SpawnDroppedItem(selectedItem);
            ApplyDropForce(droppedItem);

            TrySpawnParticleEffect();
        }

        private bool IsCurrentEnemy(GameObject enemy)
        {
            return enemy == gameObject;
        }

        private bool ShouldDropItem()
        {
            return Random.value <= _dropChance && _dropItems.Count > 0;
        }

        private GameObject SelectRandomItem()
        {
            int itemCount = _dropItems.Count;
            int randomIndex = Random.Range(0, itemCount);
            return _dropItems[randomIndex];
        }

        private GameObject SpawnDroppedItem(GameObject item)
        {
            Vector3 spawnPosition = transform.position + Vector3.up * 0.5f;
            return LeanPool.Spawn(item, spawnPosition, Quaternion.identity);
        }

        private void ApplyDropForce(GameObject item)
        {
            Rigidbody rb = item.GetComponent<Rigidbody>();
            if (rb == null) return;

            Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
            Vector3 force = Vector3.up * _dropUpForce + randomDirection * _dropOutForce;
            rb.AddForce(force, ForceMode.Impulse);
        }

        private void TrySpawnParticleEffect()
        {
            if (!_useSpawnParticle || _spawnParticle == null) return;

            Vector3 spawnPosition = transform.position + Vector3.up * 0.5f;
            GameObject particleInstance = LeanPool.Spawn(_spawnParticle, spawnPosition, Quaternion.identity);
            LeanPool.Despawn(particleInstance, 2f);
        }
    }
}