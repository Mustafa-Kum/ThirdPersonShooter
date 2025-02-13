using Interactable;
using Lean.Pool;
using Manager;
using ScriptableObjects;
using UnityEngine;

namespace Logic
{
    public class Item_Weapon : InteractableObject
    {
        [SerializeField] private PlayerWeaponSettingsSO _playerWeaponSettingsSO;

        internal override void Interaction()
        {
            base.Interaction();
            
            InvokeWeaponPickUpEvent();
            DeactivateObject();
        }

        /// <summary>
        /// Silah alma eventini çağırır.
        /// </summary>
        private void InvokeWeaponPickUpEvent()
        {
            EventManager.PlayerEvents.PlayerWeaponPickUp?.Invoke(_playerWeaponSettingsSO);
        }

        /// <summary>
        /// Nesneyi devre dışı bırakır.
        /// </summary>
        private void DeactivateObject()
        {
            gameObject.SetActive(false);
        }
    }
}