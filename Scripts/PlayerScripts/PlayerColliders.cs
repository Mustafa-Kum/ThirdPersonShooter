using System;
using Manager;
using RagDollLogic;
using UnityEngine;

namespace PlayerScripts
{
    public class PlayerColliders : MonoBehaviour
    {
        [SerializeField] private RagDoll _playerRagDoll;
        [SerializeField] private CharacterController _characterController;

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            EventManager.PlayerEvents.PlayerColliderFalse += OnCharacterColliderFalse;
            EventManager.PlayerEvents.PlayerColliderTrue += OnCharacterColliderTrue;
        }

        private void UnsubscribeFromEvents()
        {
            EventManager.PlayerEvents.PlayerColliderFalse -= OnCharacterColliderFalse;
            EventManager.PlayerEvents.PlayerColliderTrue -= OnCharacterColliderTrue;
        }

        private void OnCharacterColliderFalse()
        {
            SetRagdollColliderActive(false);
            SetCharacterControllerProperties(false, 0.001f);
        }

        private void OnCharacterColliderTrue()
        {
            SetRagdollColliderActive(true);
            SetCharacterControllerProperties(true, 0.3f);
        }

        /// <summary>
        /// Ragdoll collider'ını aktif/pasif eder.
        /// </summary>
        private void SetRagdollColliderActive(bool active)
        {
            _playerRagDoll.ActiveRagdollCollider(active);
        }

        /// <summary>
        /// CharacterController özelliklerini günceller.
        /// </summary>
        private void SetCharacterControllerProperties(bool enable, float stepOffset)
        {
            _characterController.enabled = enable;
            _characterController.stepOffset = stepOffset;
        }
    }
}