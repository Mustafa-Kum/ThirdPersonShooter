using System;
using ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UILogic
{
    public class UIWeaponSlot : MonoBehaviour
    {
        public Image _weaponIcon;
        public TextMeshProUGUI _ammoText;

        private void Awake()
        {
            _weaponIcon = GetComponentInChildren<Image>();
            _ammoText = GetComponentInChildren<TextMeshProUGUI>();
        }
        
        public void UpdateWeaponSlot(PlayerWeaponSettingsSO myWeapon)
        {
            if (myWeapon == null)
            {
                _weaponIcon.color = Color.clear;
                _ammoText.text = "";
                return;
            }
            
            _weaponIcon.sprite = myWeapon.WeaponIcon;
            _ammoText.text = myWeapon.WeaponAmmo + " / " + myWeapon.TotalReserveAmmo;
            _ammoText.color = Color.white;
        }

        public void UpdateWeaponAlpha(bool activeWeapon)
        {
            Color newColor = activeWeapon ? Color.white : new Color(1,1,1,0.5f);
            _weaponIcon.color = newColor;
        }
    }
}