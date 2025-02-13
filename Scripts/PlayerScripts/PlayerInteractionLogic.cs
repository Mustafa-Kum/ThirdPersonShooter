using System;
using System.Collections.Generic;
using Data;
using Interactable;
using Manager;
using UnityEngine;

namespace Logic
{
    public class PlayerInteractionLogic : MonoBehaviour
    {
        [SerializeField] private PlayerInteractableData _playerInteractableData;
        
        public List<InteractableObject> InteractableObjects
        {
            get => _playerInteractableData.InteractableObjects;
            set => _playerInteractableData.InteractableObjects = value;
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        /// <summary>
        /// Events'e abonelik işlemleri.
        /// </summary>
        private void SubscribeToEvents()
        {
            EventManager.PlayerEvents.PlayerInteractable += InteractWithClosest;
        }

        /// <summary>
        /// Events'ten abonelikten çıkma işlemleri.
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            EventManager.PlayerEvents.PlayerInteractable -= InteractWithClosest;
        }

        internal void UpdateClosestInteractableObject()
        {
            UnhighlightCurrentClosest();
            SelectClosestInteractableObject();
            HighlightClosestInteractable();
        }

        /// <summary>
        /// Mevcut en yakın nesneyi unhighlight eder ve null'a çeker.
        /// </summary>
        private void UnhighlightCurrentClosest()
        {
            _playerInteractableData.ClosestInteractableObject?.HighlightObject(false);
            _playerInteractableData.ClosestInteractableObject = null;
        }

        /// <summary>
        /// En yakın etkileşim nesnesini bulup kaydeder.
        /// </summary>
        private void SelectClosestInteractableObject()
        {
            _playerInteractableData.ClosestInteractableObject = GetClosestInteractableObject();
        }

        /// <summary>
        /// Bulunan en yakın etkileşim nesnesini highlight eder.
        /// </summary>
        private void HighlightClosestInteractable()
        {
            _playerInteractableData.ClosestInteractableObject?.HighlightObject(true);
        }

        /// <summary>
        /// InteractableObjects listesinde en yakın nesneyi hesaplar.
        /// </summary>
        private InteractableObject GetClosestInteractableObject()
        {
            float closestDistance = float.MaxValue;
            InteractableObject closestObject = null;

            foreach (var interactableObject in InteractableObjects)
            {
                float distance = Vector3.Distance(transform.position, interactableObject.transform.position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestObject = interactableObject;
                }
            }

            return closestObject;
        }

        /// <summary>
        /// En yakın etkileşim nesnesi ile etkileşime girer.
        /// </summary>
        private void InteractWithClosest()
        {
            PerformInteraction(_playerInteractableData.ClosestInteractableObject);
        }

        /// <summary>
        /// Verilen etkileşim nesnesi ile etkileşimi başlatır.
        /// </summary>
        private void PerformInteraction(InteractableObject interactable)
        {
            interactable?.Interaction();
        }
    }
}
