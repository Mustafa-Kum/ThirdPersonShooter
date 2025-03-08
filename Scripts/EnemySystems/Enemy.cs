using System.Collections;
using RagDollLogic;
using EnemyStateMachineLogic;
using EnemySystems;
using HpController;
using Interface;
using Lean.Pool;
using Manager;
using MissionLogic;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace EnemyLogic
{
    public enum EnemyType
    {
        Melee,
        Ranged,
        Boss,
        Random
    }
    
    [RequireComponent(typeof(NavMeshAgent))]
    public class Enemy : MonoBehaviour
    {
        #region Public Fields & Inspector Variables

        public EnemyType _enemyType;
        public LayerMask whoIsPlayer;
        
        [Header("Idle Data")]
        public float _idleTime;

        [Header("Attack Data")] 
        public float _agroRange;
        
        [Header("Movement Data")]
        public float _walkSpeed = 1.5f;
        public float _runSpeed = 3f;
        public float _rotationSpeed;
        public Transform[] _patrolPoints;
        
        [Header("Player Data")]
        public PlayerTransformValueSO _playerTransformValueSO;
        
        [Header("Sound Settings")]
        public AudioSource _swooshSound;
        public AudioSource _bulletImpactSound;
        public AudioSource _footStepSound;
        public AudioSource _runStepSound;
        public AudioSource _dodgeSound;
        public AudioSource _shieldHitSound;
        public AudioSource[] _robotVoiceList;

        #endregion

        #region Private/Protected Fields

        private int _currentPatrolIndex;
        private bool _moveManually;
        private bool _rotationManually;
        private Vector3[] _patrolPointPositions;
        protected bool _isMeleeAttackReady;
        protected bool _isRobotVoiceCoroutineRunning = false;
        protected internal bool _inBattleMode { get; private set; }

        #endregion

        #region Components & Properties
        
        public NavMeshAgent _navMeshAgent { get; private set; }
        public EnemyStateMachine _stateMachine { get; private set; }
        public Enemy_Visuals _enemyVisuals { get; private set; }
        public Animator _animator { get; private set; }
        public RagDoll RagDoll { get; private set; }
        public Enemy_Health _enemyHealth { get; private set; }
        public EnemyDropController _enemyDropController { get; private set; }

        #endregion
        
        #region Unity Built-in Methods
        
        /// <summary>
        /// Başlangıçta gerekli component referanslarını alır.
        /// </summary>
        protected virtual void Awake()
        {
            InitializeComponents();
        }
        
        /// <summary>
        /// Oyuncunun patrol noktalarını hazırlar.
        /// </summary>
        protected virtual void Start()
        {
            InitializePatrolPoints();
        }
        
        /// <summary>
        /// Her frame tetiklenir, düşmanın battle moduna girip girmeyeceğini kontrol eder.
        /// </summary>
        protected virtual void Update()
        {
            if (ShouldEnterBattleMode())
                EnterBattleMode();
        }

        /// <summary>
        /// Özel düşman özelliklerini (örneğin Ranged/Unstoppable) başlatmak için override edilebilir.
        /// </summary>
        protected virtual void InitiliazeSpeciality()
        {
            
        }

        /// <summary>
        /// Düşman "gizli" çizimleri, örneğin agro mesafesi çemberini gösterir.
        /// </summary>
        protected virtual void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _agroRange);
        }

        #endregion

        #region Initialization & Setup
        
        /// <summary>
        /// Gerekli component’lerin referanslarını alır.
        /// </summary>
        private void InitializeComponents()
        {
            _stateMachine = new EnemyStateMachine();
            _enemyVisuals = GetComponent<Enemy_Visuals>();
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _animator = GetComponentInChildren<Animator>();
            RagDoll = GetComponent<RagDoll>();
            _enemyHealth = GetComponent<Enemy_Health>();
            _enemyDropController = GetComponent<EnemyDropController>();
        }

        /// <summary>
        /// Patrol noktalarını cache’ler ve gizler.
        /// </summary>
        private void InitializePatrolPoints()
        {
            _patrolPointPositions = new Vector3[_patrolPoints.Length];
            
            for (int i = 0; i < _patrolPoints.Length; i++)
            {
                _patrolPointPositions[i] = _patrolPoints[i].position;
                _patrolPoints[i].gameObject.SetActive(false);
            }
        }

        #endregion

        #region Health & Damage
        
        /// <summary>
        /// Düşman bir hasar aldığında çalışır. Hasar alırken Battle mode’a geçer ve düşmanın ölüp ölmeyeceğini kontrol eder.
        /// </summary>
        /// <param name="damage"></param>
        public virtual void GetHit(int damage)
        {
            EnterBattleMode();
            _enemyHealth.ReduceHealth(damage);

            if (_enemyHealth.ShouldDie())
                Die();
        }

        /// <summary>
        /// Görev (Mission) objesi olarak seçildiğinde ek sağlık ve boyut artışı uygular.
        /// </summary>
        public virtual void ChosenEnemyForKey()
        {
            int additonalHealth = Mathf.RoundToInt(_enemyHealth._currentHealth * 1.5f);
            _enemyHealth._currentHealth += additonalHealth;
            transform.localScale = transform.localScale * 1.15f;
        }

        /// <summary>
        /// Düşmanın ölümü ile ilgili işlemler.
        /// </summary>
        public virtual void Die()
        {
            _enemyDropController.DropItem();
            DeactivateAnimatorAndNavAgent();
            RagDoll.ActivateRagDollRigidBody(true);
            //HandleMissionHuntTarget();
        }

        /// <summary>
        /// Ölürken animatörü, NavMeshAgent’i kapatma işlemi.
        /// </summary>
        private void DeactivateAnimatorAndNavAgent()
        {
            _animator.enabled = false;
            _navMeshAgent.isStopped = false;
            _navMeshAgent.enabled = false;
        }

        /// <summary>
        /// Eğer bir HuntTarget var ise, öldüğünü bildirir.
        /// </summary>
        private void HandleMissionHuntTarget()
        {
            MissionObject_HuntTarget missionObjectHuntTarget = GetComponent<MissionObject_HuntTarget>();
            if (missionObjectHuntTarget != null)
                missionObjectHuntTarget.TargetKilled();
        }
        
        #endregion

        #region Battle & Combat

        /// <summary>
        /// Düşmanı Battle Mode’a sokar.
        /// </summary>
        public virtual void EnterBattleMode()
        {
            _inBattleMode = true;
        }

        /// <summary>
        /// Oyuncu agro menziline girdiğinde Battle Mode’a geçmemiz gerekip gerekmediğini döndürür.
        /// </summary>
        /// <returns></returns>
        protected bool ShouldEnterBattleMode()
        {
            if (IsPlayerInAgroRange() && !_inBattleMode)
            {
                EnterBattleMode(); 
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Yakın dövüş saldırısını kontrol eder. Hasar verebilecek hedefleri algılar.
        /// </summary>
        /// <param name="damagePoints"></param>
        /// <param name="attackCheckRadius"></param>
        /// <param name="meleeAttackImpactFx"></param>
        /// <param name="damage"></param>
        /// <param name="impactSound"></param>
        public virtual void MeleeAttackCheck(
            Transform[] damagePoints, 
            float attackCheckRadius, 
            GameObject meleeAttackImpactFx, 
            int damage, 
            AudioSource impactSound)
        {
            if (!_isMeleeAttackReady)
                return;

            foreach (Transform attackPoint in damagePoints)
            {
                Collider[] detectedHits = Physics.OverlapSphere(attackPoint.position, attackCheckRadius, whoIsPlayer);

                for (int i = 0; i < detectedHits.Length; i++)
                {
                    IDamagable damagable = detectedHits[i].GetComponent<IDamagable>();
                    if (damagable != null)
                    {
                        damagable.TakeDamage(damage);
                        _isMeleeAttackReady = false;
                        
                        // Vuruş efekti
                        GameObject newAttackImpactFx = LeanPool.Spawn(meleeAttackImpactFx, detectedHits[0].transform);
                        EventManager.AudioEvents.AudioEnemyMeleeImpactSound?.Invoke(impactSound, true, 0.9f, 1.1f);
                        LeanPool.Despawn(newAttackImpactFx, 1f);
                        return;
                    }
                }
            }
        }
        
        /// <summary>
        /// Yakın dövüş saldırısı yapıp yapmayacağımızı belirler.
        /// </summary>
        /// <param name="enable"></param>
        public void EnableMeleeAttackCheck(bool enable)
        { 
            _isMeleeAttackReady = enable;
        }

        /// <summary>
        /// Özel yetenek tetiklendiğinde state üzerinden çağrılır.
        /// </summary>
        public virtual void SpecialAbilityTrigger()
        {
            _stateMachine._currentState.SpecialAbilityTrigger();
        }

        #endregion

        #region Audio & Robot Voice

        /// <summary>
        /// Tüm robot seslerini durdurur.
        /// </summary>
        protected void StopAllRobotVoices()
        {
            if (_robotVoiceList != null && _robotVoiceList.Length > 0)
            {
                foreach (var audioSource in _robotVoiceList)
                {
                    if (audioSource.isPlaying)
                    {
                        audioSource.Stop(); 
                    }
                }
            }

            _isRobotVoiceCoroutineRunning = false;
        }

        /// <summary>
        /// Robot sesi için bir coroutine, belirli aralıklarla rastgele robot sesi çalar.
        /// </summary>
        /// <returns></returns>
        protected IEnumerator RobotVoiceEnumerator()
        {
            _isRobotVoiceCoroutineRunning = true;
            while (true)
            {
                if (_robotVoiceList != null && _robotVoiceList.Length > 0)
                {
                    int randomIndex = Random.Range(0, _robotVoiceList.Length);
                    AudioSource selectedAudio = _robotVoiceList[randomIndex];

                    if (!selectedAudio.isPlaying)
                    {
                        selectedAudio.Play();
                    }
                }

                float randomDelay = Random.Range(3f, 10f);
                yield return new WaitForSeconds(randomDelay);
            }
        }

        /// <summary>
        /// Karakterin yürürken çıkardığı ayak sesini çalar.
        /// </summary>
        public void FootStepSound()
        {
            float pitch = Random.Range(0.7f, 1.3f);
            _footStepSound.pitch = pitch;
            
            if (!_footStepSound.isPlaying)
                _footStepSound.Play();
        }

        /// <summary>
        /// Karakterin koşarken çıkardığı ayak sesini çalar.
        /// </summary>
        public void RunStepSound()
        {
            float pitch = Random.Range(0.7f, 1.3f);
            _runStepSound.pitch = pitch;
            
            if (!_runStepSound.isPlaying)
                _runStepSound.Play();
        }

        /// <summary>
        /// Karakterin dodge animasyonu sırasında çıkardığı sesi çalar.
        /// </summary>
        public void DodgeSound()
        {
            float pitch = Random.Range(0.7f, 1.3f);
            _dodgeSound.pitch = pitch;
            
            if (!_dodgeSound.isPlaying)
                _dodgeSound.Play();
        }

        /// <summary>
        /// Karakterin kalkanıyla engelleme yaptığında çıkardığı sesi çalar.
        /// </summary>
        public void ShieldHitSound()
        {
            float pitch = Random.Range(0.7f, 1.3f);
            _shieldHitSound.pitch = pitch;
            
            if (!_shieldHitSound.isPlaying)
                _shieldHitSound.Play();
        }

        #endregion

        #region Movement & Rotation

        /// <summary>
        /// Basit bir patrol mekaniği için hedef konumu döndürür.
        /// </summary>
        /// <returns></returns>
        public Vector3 GetPatrolDestination()
        {
            if (_patrolPoints.Length == 0)
                return transform.position;
            
            Vector3 destination = _patrolPointPositions[_currentPatrolIndex];
            _currentPatrolIndex++;
            
            if (_currentPatrolIndex >= _patrolPoints.Length)
                _currentPatrolIndex = 0;
            
            return destination;
        }

        /// <summary>
        /// Hedefe doğru yüzünü döndürür.
        /// </summary>
        /// <param name="target">Dönülecek nokta</param>
        /// <param name="rotationSpeed">İsteğe bağlı, 0 girilirse _rotationSpeed kullanılır.</param>
        public void FaceToTarget(Vector3 target, float rotationSpeed = 0)
        {
            Quaternion targetRotation = Quaternion.LookRotation(target - transform.position);
            Vector3 currentEulerAngle = transform.rotation.eulerAngles;

            if (rotationSpeed == 0)
                rotationSpeed = _rotationSpeed;
            
            float yRotation = Mathf.LerpAngle(currentEulerAngle.y, targetRotation.eulerAngles.y, rotationSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(currentEulerAngle.x, yRotation, currentEulerAngle.z);
        }
        
        /// <summary>
        /// Animasyon içerisindeki transition event’lerinde çağrılır; state makinesine tetik gönderir.
        /// </summary>
        public void AnimationTrigger()
        {
            _stateMachine._currentState.AnimationTrigger();
        }

        /// <summary>
        /// Manuel olarak hareket verisini aktifleştirir.
        /// </summary>
        /// <param name="moveManual"></param>
        public void ActivateManualMovement(bool moveManual)
        {
            _moveManually = moveManual;
        }

        /// <summary>
        /// Manuel olarak dönme verisini aktifleştirir.
        /// </summary>
        /// <param name="rotateManual"></param>
        public void ActivateManualRotation(bool rotateManual)
        {
            _rotationManually = rotateManual;
        }
        
        /// <summary>
        /// Manuel hareket aktif mi?
        /// </summary>
        /// <returns></returns>
        public bool ManualMovementActive()
        {
            return _moveManually;
        }
        
        /// <summary>
        /// Manuel dönüş aktif mi?
        /// </summary>
        /// <returns></returns>
        public bool ManualRotationActive()
        {
            return _rotationManually;
        }

        #endregion

        #region Checks & Conditions
        
        /// <summary>
        /// Oyuncu agro (düşman görme) menzilinde mi?
        /// </summary>
        /// <returns></returns>
        public bool IsPlayerInAgroRange()
        {
            return Vector3.Distance(transform.position, _playerTransformValueSO.PlayerTransform) < _agroRange;
        }

        #endregion
    }
}
