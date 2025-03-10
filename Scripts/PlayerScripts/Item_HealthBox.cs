using Interactable;
using Manager;
using UnityEngine;

namespace Item
{
    public class Item_HealthBox : InteractableObject
    {
        internal override void Interaction()
        {
            base.Interaction();
            
            // Interaksiyon sürecini parçalıyoruz.
            InvokeHealthPickedEvent();
            DeactivateObject();
        }

        /// <summary>
        /// Ammo alındı event'ini çağırır.
        /// </summary>
        private void InvokeHealthPickedEvent()
        {
            EventManager.PlayerEvents.PlayerHealthPickedUp?.Invoke();
        }

        /// <summary>
        /// Nesnenin sahneden kaybolması veya devre dışı kalmasını yönetir.
        /// </summary>
        private void DeactivateObject()
        {
            gameObject.SetActive(false);
        }
    }
}