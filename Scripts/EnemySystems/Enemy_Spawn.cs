using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Manager;
using ScriptableObjects;

namespace EnemyLogic
{
    public class Enemy_Spawn : MonoBehaviour
    {
        #region Nested Classes
        [System.Serializable]
        public class Wave
        {
            public string waveName = "Wave";
            public List<GameObject> enemies;
            [Tooltip("Saniye cinsinden spawn aralığı")] 
            public float spawnInterval = 2f;
            [Header("Spawn Limitleri")]
            public int maxEnemies = 10;
            [Header("Spawn Mesafesi")]
            [Tooltip("Oyuncudan en yakın mesafe")]
            public float minDistanceFromPlayer = 10f;
            [Tooltip("Oyuncudan maksimum uzaklık")]
            public float maxDistanceFromPlayer = 30f;
            public bool infiniteEnemies;

            public bool ShouldContinueSpawning(int spawnedCount) => infiniteEnemies || spawnedCount < maxEnemies;
        }
        #endregion

        #region Dependencies
        [SerializeField] private List<Wave> waves;
        [SerializeField] private float waveCooldown = 5f;
        [SerializeField] private BoxCollider spawnArea;
        [SerializeField] private PlayerTransformValueSO playerTransform;
        [SerializeField] private LayerMask environmentLayerMask;
        #endregion

        #region Runtime State
        private int currentWaveIndex = 0;
        private Coroutine activeWaveCoroutine;
        private bool isWaveActive = false;
        private int activeEnemyCount = 0;
        private bool waveSpawningCompleted = false;
        #endregion

        #region Unity Events
        private void OnEnable() 
        {
            EventManager.EnemySpawnEvents.EnemySpawn += StartWave;
            EventManager.EnemySpawnEvents.EnemyDied += OnEnemyDied;
        }

        private void OnDisable()
        {
            EventManager.EnemySpawnEvents.EnemySpawn -= StartWave;
            EventManager.EnemySpawnEvents.EnemyDied -= OnEnemyDied;
            StopAllCoroutines();
            ResetState();
        }
        #endregion

        #region Public API
        public virtual void ResetWaves()
        {
            currentWaveIndex = 0;
            ResetState();
            StopAllCoroutines();
        }

        public virtual void SkipToWave(int waveIndex)
        {
            if (!IsValidWaveIndex(waveIndex)) return;

            currentWaveIndex = waveIndex;
            StopAllCoroutines();
            StartWave();
        }
        #endregion

        #region Wave Management
        protected virtual void StartWave()
        {
            if (!CanStartNewWave()) return;

            // Eğer 5. dal başlıyorsa (0 tabanlı dizide index 4)
            if (currentWaveIndex == 4)
            {
                EventManager.MapEvents.ColumnSpawn?.Invoke();
            }

            // Yeni dal başlamadan önce sayaçları sıfırlıyoruz.
            activeEnemyCount = 0;
            waveSpawningCompleted = false;

            activeWaveCoroutine = StartCoroutine(SpawnWaveRoutine());
            isWaveActive = true;
        }


        protected virtual IEnumerator SpawnWaveRoutine()
        {
            var currentWave = waves[currentWaveIndex];
            LogWaveStart(currentWave);

            // Dal kapsamında düşman spawn işlemi başlatılıyor.
            yield return SpawnEnemies(currentWave);

            // Tüm spawn işlemi tamamlandı.
            waveSpawningCompleted = true;

            // Eğer o ana kadar oluşturulan tüm düşmanlar öldüyse, dalı bitir.
            if (activeEnemyCount <= 0)
            {
                yield return DelayedNextWave();
            }
            // Aksi durumda, OnEnemyDied metodu kalan düşmanları takip edecektir.
        }

        protected virtual IEnumerator DelayedNextWave()
        {
            yield return new WaitForSeconds(waveCooldown);

            currentWaveIndex++;
            isWaveActive = false;

            if (HasMoreWaves())
            {
                StartNextWave();
            }
            else
            {
                Debug.Log("Tüm wave'lar tamamlandı!");
            }
        }
        #endregion

        #region Spawn Logic
        protected virtual void SpawnSingleEnemy(GameObject enemyPrefab)
        {
            if (TryCalculateSafeSpawnPosition(out Vector3 spawnPosition))
            {
                Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            }
        }

        protected virtual bool TryCalculateSafeSpawnPosition(out Vector3 position)
        {
            const int maxAttempts = 100;
            int attempts = 0;

            do
            {
                position = GetRandomPositionInBounds();
                attempts++;
            }
            while ((IsInvalidDistanceFromPlayer(position) || IsPositionBlocked(position)) && attempts <= maxAttempts);

            if (attempts > maxAttempts)
            {
                Debug.LogWarning("Uygun spawn pozisyonu bulunamadı!");
                return false;
            }
            return true;
        }

        protected virtual bool IsInvalidDistanceFromPlayer(Vector3 position)
        {
            float distance = Vector3.Distance(position, playerTransform.PlayerTransform);
            Wave currentWave = waves[currentWaveIndex];
            return distance < currentWave.minDistanceFromPlayer || distance > currentWave.maxDistanceFromPlayer;
        }

        protected virtual bool IsPositionBlocked(Vector3 position)
        {
            const float checkRadius = 0.5f;
            return Physics.CheckSphere(position, checkRadius, environmentLayerMask);
        }

        protected virtual Vector3 GetRandomPositionInBounds()
        {
            Bounds bounds = spawnArea.bounds;
            return new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                bounds.min.y,
                Random.Range(bounds.min.z, bounds.max.z)
            );
        }
        #endregion

        #region Validation & Checks
        protected virtual bool CanStartNewWave() => !isWaveActive && currentWaveIndex < waves.Count;
        protected virtual bool IsValidWaveIndex(int index) => index >= 0 && index < waves.Count;
        protected virtual bool ShouldSpawnMore(Wave wave, int spawnedCount) => wave.infiniteEnemies || spawnedCount < wave.maxEnemies;
        protected virtual bool HasMoreWaves() => currentWaveIndex < waves.Count;
        protected virtual bool IsTooCloseToPlayer(Vector3 position) =>
            Vector3.Distance(position, playerTransform.PlayerTransform) < waves[currentWaveIndex].minDistanceFromPlayer;
        #endregion

        #region Utility Methods
        protected virtual void ResetState() => isWaveActive = false;

        protected virtual void StartNextWave()
        {
            Debug.Log($"Sonraki wave'e geçiliyor: {waves[currentWaveIndex].waveName}");
            StartWave();
        }

        protected virtual void LogWaveStart(Wave wave) => Debug.Log($"Wave {wave.waveName} başladı!");
        #endregion

        #region Enemy Death Handling
        protected virtual void OnEnemyDied(GameObject enemy)
        {
            activeEnemyCount--;
            // Eğer spawn işlemi tamamlandıysa ve tüm düşmanlar öldüyse, dalı bitir.
            if (waveSpawningCompleted && activeEnemyCount <= 0)
            {
                StartCoroutine(DelayedNextWave());
            }
        }
        #endregion

        #region Spawn Coroutine
        protected virtual IEnumerator SpawnEnemies(Wave wave)
        {
            int spawnedCount = 0;

            while (wave.ShouldContinueSpawning(spawnedCount))
            {
                foreach (var enemyPrefab in wave.enemies)
                {
                    if (!ShouldSpawnMore(wave, spawnedCount))
                        yield break;

                    SpawnSingleEnemy(enemyPrefab);
                    activeEnemyCount++;
                    spawnedCount++;
                    yield return new WaitForSeconds(wave.spawnInterval);
                }
            }
        }
        #endregion
    }
}
