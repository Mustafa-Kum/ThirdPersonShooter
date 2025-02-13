using Interactable;
using Manager;
using UnityEngine;

namespace Item
{
    public class Item_AmmoBox : InteractableObject
    {
        internal override void Interaction()
        {
            base.Interaction();
            
            // Interaksiyon sürecini parçalıyoruz.
            InvokeAmmoPickedEvent();
            DeactivateObject();
            LogAmmoAdded();
        }

        /// <summary>
        /// Ammo alındı event'ini çağırır.
        /// </summary>
        private void InvokeAmmoPickedEvent()
        {
            EventManager.PlayerEvents.PlayerAmmoPickedUp?.Invoke();
        }

        /// <summary>
        /// Nesnenin sahneden kaybolması veya devre dışı kalmasını yönetir.
        /// </summary>
        private void DeactivateObject()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Ammo eklendiğini loglar.
        /// </summary>
        private void LogAmmoAdded()
        {
            Debug.Log("Added ammo to player inventory.");
        }
    }
}