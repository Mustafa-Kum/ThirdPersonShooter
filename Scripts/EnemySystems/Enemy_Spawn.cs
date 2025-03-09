using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Manager;
using ScriptableObjects;
using Lean.Pool;
using TMPro;

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
            public GameObject weaponSpawn;
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
        [SerializeField] private GameObject _spawnParticle;
        [SerializeField] private TextMeshProUGUI _waveText;
        #endregion

        #region Runtime State
        private Coroutine activeWaveCoroutine;
        private Coroutine waveTextAnimationCoroutine;
        private int currentWaveIndex = 0;
        private int activeEnemyCount = 0;
        private bool waveSpawningCompleted = false;
        private bool isWaveActive = false;
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
            
            // Wave text'i sıfırla ve gizle
            if (_waveText != null)
            {
                _waveText.alpha = 0f;
                _waveText.transform.localScale = Vector3.zero;
            }
        }

        public virtual void SkipToWave(int waveIndex)
        {
            if (!IsValidWaveIndex(waveIndex)) return;

            currentWaveIndex = waveIndex;
            StopAllCoroutines();
            
            // Wave text animasyonu sıfırla
            if (_waveText != null)
            {
                _waveText.alpha = 0f;
                _waveText.transform.localScale = Vector3.zero;
            }
            
            StartWave();
        }
        #endregion

        #region Wave Management
        protected virtual void StartWave()
        {
            if (!CanStartNewWave()) return;

            // Wave text animasyonu başlat
            if (_waveText != null)
            {
                // Önceki animasyon varsa durdur
                if (waveTextAnimationCoroutine != null)
                {
                    StopCoroutine(waveTextAnimationCoroutine);
                }
                
                waveTextAnimationCoroutine = StartCoroutine(AnimateWaveText($"Wave {currentWaveIndex + 1}"));
            }

            // Eğer 5. dal başlıyorsa (0 tabanlı dizide index 4)
            if (currentWaveIndex == 1)
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
            
            // Opsiyonel: specialSpawn objesini spawnla (null kontrolü ile)
            if (currentWave.weaponSpawn != null)
            {
                SpawnWeaponObject(currentWave.weaponSpawn);
            }

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

        protected virtual void SpawnWeaponObject(GameObject weaponPrefab)
        {
            if (TryCalculateSafeSpawnPosition(out Vector3 spawnPosition))
            {
                // Özel obje spawn ediliyor.
                GameObject weaponObj = LeanPool.Spawn(weaponPrefab, spawnPosition, Quaternion.identity);

                // İsteğe bağlı: Spawn efektleri veya ek ayarlamalar eklenebilir.
                GameObject particleInstance = LeanPool.Spawn(_spawnParticle, weaponObj.transform.position, Quaternion.identity);
                ParticleSystem ps = particleInstance.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    float delay = ps.main.duration;
                    LeanPool.Despawn(particleInstance, delay);
                }
                else
                {
                    LeanPool.Despawn(particleInstance, 2f);
                }
            }
            else
            {
                Debug.LogWarning("Özel obje için uygun spawn pozisyonu bulunamadı!");
            }
        }


        protected virtual IEnumerator DelayedNextWave()
        {
            yield return new WaitForSeconds(waveCooldown);

            currentWaveIndex++;
            isWaveActive = false;

            if (HasMoreWaves())
            {
                // Wave text animasyonu için önceki animasyonu durdurma işlemi
                if (waveTextAnimationCoroutine != null)
                {
                    StopCoroutine(waveTextAnimationCoroutine);
                }
                
                // Eğer daha sonraki wave'e geçiliyorsa, yeni animasyonu başlat
                if (_waveText != null)
                {
                    waveTextAnimationCoroutine = StartCoroutine(AnimateWaveText($"Wave {currentWaveIndex + 1}"));
                }
                
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
                // Enemy spawn ediliyor.
                GameObject enemy = LeanPool.Spawn(enemyPrefab, spawnPosition, Quaternion.identity);
                
                // Spawn particle efektini enemy'nin pozisyonunda oluşturuyoruz.
                GameObject particleInstance = LeanPool.Spawn(_spawnParticle, enemy.transform.position, Quaternion.identity);
                
                // Particle sistemini alıp, efekt süresi kadar bekleyip despawn ediyoruz.
                ParticleSystem ps = particleInstance.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    // Particle sisteminin süresi kadar bekleyip, efekt tamamlandığında despawn ediliyor.
                    float delay = ps.main.duration;
                    // Eğer particle sisteminin startLifetime'ı da eklenmek isteniyorsa:
                    // delay += ps.main.startLifetime.constant;
                    LeanPool.Despawn(particleInstance, delay);
                }
                else
                {
                    // Eğer ParticleSystem bileşeni bulunamazsa, sabit bir süre sonrası despawn ediliyor.
                    LeanPool.Despawn(particleInstance, 2f);
                }
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

                    // Artık enemy spawn işlemi coroutine olarak çağrılıyor
                    yield return SpawnSingleEnemyCoroutine(enemyPrefab);
                    activeEnemyCount++;
                    spawnedCount++;
                    yield return new WaitForSeconds(wave.spawnInterval);
                }
            }
        } 

        protected virtual IEnumerator SpawnSingleEnemyCoroutine(GameObject enemyPrefab)
        {
            Vector3 spawnPosition;
            // Uygun pozisyon bulunana kadar bekle
            while (!TryCalculateSafeSpawnPosition(out spawnPosition))
            {
                // Her denemeden sonra bir sonraki frame'e geçiyoruz.
                yield return null;
            }

            // Uygun pozisyon bulunduğunda enemy spawn ediliyor.
            GameObject enemy = LeanPool.Spawn(enemyPrefab, spawnPosition, Quaternion.identity);
            
            // Spawn efektleri
            GameObject particleInstance = LeanPool.Spawn(_spawnParticle, enemy.transform.position, Quaternion.identity);
            ParticleSystem ps = particleInstance.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                float delay = ps.main.duration;
                LeanPool.Despawn(particleInstance, delay);
            }
            else
            {
                LeanPool.Despawn(particleInstance, 2f);
            }
        } 
        #endregion

        #region UI Animation
        protected virtual IEnumerator AnimateWaveText(string waveText)
        {
            // Text içeriğini ayarla
            _waveText.text = waveText;
            
            // Başlangıçta tamamen saydam ve küçük
            _waveText.alpha = 0f;
            _waveText.transform.localScale = Vector3.zero;
            
            // Animasyon süreleri
            float fadeInDuration = 0.5f;
            float displayDuration = 3f;
            float fadeOutDuration = 0.5f;
            
            // Fade in ve scale up
            float elapsedTime = 0f;
            while (elapsedTime < fadeInDuration)
            {
                float normalizedTime = elapsedTime / fadeInDuration;
                _waveText.alpha = Mathf.Lerp(0f, 1f, normalizedTime);
                _waveText.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, Mathf.SmoothStep(0f, 1f, normalizedTime));
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            _waveText.alpha = 1f;
            _waveText.transform.localScale = Vector3.one;
            
            // Text'i belirli süre göster
            yield return new WaitForSeconds(displayDuration);
            
            // Fade out ve scale down
            elapsedTime = 0f;
            while (elapsedTime < fadeOutDuration)
            {
                float normalizedTime = elapsedTime / fadeOutDuration;
                _waveText.alpha = Mathf.Lerp(1f, 0f, normalizedTime);
                _waveText.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, Mathf.SmoothStep(0f, 1f, normalizedTime));
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            _waveText.alpha = 0f;
            _waveText.transform.localScale = Vector3.zero;
        }
        #endregion
    }
}
