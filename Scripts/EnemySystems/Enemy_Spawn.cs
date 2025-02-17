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
            [Tooltip("Sonsuz düşman için işaretle")] 
            public bool infiniteEnemies;

            public bool ShouldContinueSpawning(int spawnedCount) => infiniteEnemies || spawnedCount < maxEnemies;
        }
        #endregion

        #region Dependencies
        [SerializeField] private List<Wave> waves;
        [SerializeField] private float waveCooldown = 5f;
        [SerializeField] private BoxCollider spawnArea;
        [SerializeField] private PlayerTransformValueSO playerTransform;
        [SerializeField] private float minDistanceFromPlayer = 10f;
        #endregion

        #region Runtime State
        private int currentWaveIndex = 0;
        private Coroutine activeWaveCoroutine;
        private bool isWaveActive = false;

        // Aktif düşman sayısını ve spawn işleminin tamamlanıp tamamlanmadığını takip eden değişkenler:
        private int activeEnemyCount = 0;
        private bool waveSpawningCompleted = false;
        #endregion

        #region Unity Events

        private void OnEnable() 
        {
            EventManager.EnemySpawnEvents.EnemySpawn += StartWave;
            // Düşman öldüğünde tetiklenen event'e abone ol
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
        public void ResetWaves()
        {
            currentWaveIndex = 0;
            ResetState();
            StopAllCoroutines();
        }

        public void SkipToWave(int waveIndex)
        {
            if(!IsValidWaveIndex(waveIndex)) return;
            
            currentWaveIndex = waveIndex;
            StopAllCoroutines();
            StartWave();
        }
        #endregion

        #region Wave Management
        private void StartWave()
        {
            if(!CanStartNewWave()) return;
            
            // Yeni dal başlamadan önce sayaçları sıfırlıyoruz.
            activeEnemyCount = 0;
            waveSpawningCompleted = false;
            
            activeWaveCoroutine = StartCoroutine(SpawnWaveRoutine());
            isWaveActive = true;
        }

        private IEnumerator SpawnWaveRoutine()
        {
            var currentWave = waves[currentWaveIndex];
            LogWaveStart(currentWave);
            
            // Dal için düşmanları spawn etmeye başla.
            yield return SpawnEnemies(currentWave);
            
            // Tüm düşman spawn işlemi tamamlandı.
            waveSpawningCompleted = true;
            
            // Eğer o ana kadar spawn ettiğimiz tüm düşmanlar öldüyse, dalı tamamla.
            if(activeEnemyCount <= 0)
            {
                yield return DelayedNextWave();
            }
            // Eğer aktif düşman kalmışsa, OnEnemyDied event handler'ı kalan düşmanları takip edip, 
            // sayaç sıfırlandığında dal geçişini tetikleyecektir.
        }

        private IEnumerator DelayedNextWave()
        {
            yield return new WaitForSeconds(waveCooldown);
            
            currentWaveIndex++;
            isWaveActive = false;

            if(HasMoreWaves())
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
        private void SpawnSingleEnemy(GameObject enemyPrefab)
        {
            Vector3 spawnPosition = CalculateSafeSpawnPosition();
            Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        }

        private Vector3 CalculateSafeSpawnPosition()
        {
            const int maxAttempts = 100;
            int attempts = 0;
            Vector3 position;

            do
            {
                position = GetRandomPositionInBounds();
                attempts++;
            } 
            while(IsTooCloseToPlayer(position) && attempts <= maxAttempts);

            if(attempts > maxAttempts)
                Debug.LogWarning("Uygun spawn pozisyonu bulunamadı!");

            return position;
        }

        private Vector3 GetRandomPositionInBounds()
        {
            Bounds bounds = spawnArea.bounds;
            return new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y),
                Random.Range(bounds.min.z, bounds.max.z)
            );
        }
        #endregion

        #region Validation & Checks
        private bool CanStartNewWave() => !isWaveActive && currentWaveIndex < waves.Count;
        private bool IsValidWaveIndex(int index) => index >= 0 && index < waves.Count;
        private bool ShouldSpawnMore(Wave wave, int spawnedCount) => wave.infiniteEnemies || spawnedCount < wave.maxEnemies;
        private bool HasMoreWaves() => currentWaveIndex < waves.Count;
        private bool IsTooCloseToPlayer(Vector3 position) => Vector3.Distance(position, playerTransform.PlayerTransform) < minDistanceFromPlayer;
        #endregion

        #region Utility Methods
        private void ResetState() => isWaveActive = false;
        private void StartNextWave()
        {
            Debug.Log($"Sonraki wave'e geçiliyor: {waves[currentWaveIndex].waveName}");
            StartWave();
        }
        private void LogWaveStart(Wave wave) => Debug.Log($"Wave {wave.waveName} başladı!");
        #endregion

        #region Enemy Death Handling
        // Düşman öldüğünde tetiklenen event handler.
        private void OnEnemyDied(GameObject enemy)
        {
            activeEnemyCount--;
            // Eğer spawn işlemi tamamlandıysa ve tüm düşmanlar öldüyse, dalı bitir.
            if(waveSpawningCompleted && activeEnemyCount <= 0)
            {
                StartCoroutine(DelayedNextWave());
            }
        }
        #endregion

        #region Spawn Coroutine
        private IEnumerator SpawnEnemies(Wave wave)
        {
            int spawnedCount = 0;
            
            while(wave.ShouldContinueSpawning(spawnedCount))
            {
                foreach(var enemyPrefab in wave.enemies)
                {
                    if(!ShouldSpawnMore(wave, spawnedCount)) yield break;
                    
                    SpawnSingleEnemy(enemyPrefab);
                    activeEnemyCount++; // Her spawn edilen düşman için sayaç artırılıyor.
                    spawnedCount++;
                    yield return new WaitForSeconds(wave.spawnInterval);
                }
            }
        }
        #endregion
    }
}
