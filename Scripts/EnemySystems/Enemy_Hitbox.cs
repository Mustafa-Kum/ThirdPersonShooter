using System.Collections;
using EnemyLogic;
using Lean.Pool;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace HitboxLogic
{
    public class Enemy_Hitbox : Hitbox
    {
        [SerializeField] private MultiAimConstraint _multiAimConstraint;
        [SerializeField] private AudioSource _bodyfallSound;
        [SerializeField] private GameObject[] _blood;
        [SerializeField] private GameObject _bodyAttachedBlood;

        private Enemy _enemy;
        private bool _hasPlayedSound = false;
        private bool _hasSpawnedAttachedBlood = false;

        protected override void Awake()
        {
            base.Awake();
            _enemy = GetComponentInParent<Enemy>();
        }

        public override void TakeDamage(int damage)
        {
            int adjustedDamage = CalculateDamage(damage);
            _enemy.GetHit(adjustedDamage);

            if (_multiAimConstraint != null)
            {
                StartCoroutine(SmoothWeightTransition());
            }
        }

        private int CalculateDamage(int damage)
        {
            return Mathf.RoundToInt(damage * _damageMultiplier);
        }

        private IEnumerator SmoothWeightTransition()
        {
            float duration = 0.1f;
            yield return TransitionWeight(0f, 0.3f, duration);
            yield return new WaitForSeconds(0.1f);
            yield return TransitionWeight(0.3f, 0f, duration);
        }

        private IEnumerator TransitionWeight(float from, float to, float duration)
        {
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                _multiAimConstraint.weight = Mathf.Lerp(from, to, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            _multiAimConstraint.weight = to;
        }

        private void OnCollisionEnter(Collision collision)
        {
            HandleGroundCollision(collision);
            HandleBulletCollision(collision);
        }

        private void HandleGroundCollision(Collision collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Ground") && !_hasPlayedSound && _bodyfallSound != null)
            {
                _bodyfallSound.Play();
                _hasPlayedSound = true;
            }
        }

        private void HandleBulletCollision(Collision collision)
        {
            if (_blood == null || _blood.Length == 0 || collision.contacts.Length == 0 || collision.gameObject.layer != LayerMask.NameToLayer("Bullet"))
                return;

            SpawnBloodDecal(collision);
            SpawnAttachedBloodOnce();
        }

        private void SpawnBloodDecal(Collision collision)
        {
            GameObject selectedBlood = _blood[Random.Range(0, _blood.Length)];
            Vector3 collisionPoint = collision.contacts[0].point;
            Vector3 collisionNormal = collision.contacts[0].normal;
            Quaternion rotation = Quaternion.LookRotation(-collisionNormal);

            var blood = LeanPool.Spawn(selectedBlood, collisionPoint, rotation);
            blood.transform.parent = null;
            LeanPool.Despawn(blood, 5f);
        }

        private void SpawnAttachedBloodOnce()
        {
            if (_hasSpawnedAttachedBlood || _bodyAttachedBlood == null)
                return;

            var attachedBlood = LeanPool.Spawn(_bodyAttachedBlood, transform);
            LeanPool.Despawn(attachedBlood, 10f);
            _hasSpawnedAttachedBlood = true;
        }

        private void OnCollisionExit(Collision collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                _hasPlayedSound = false;
            }
        }
    }
}
