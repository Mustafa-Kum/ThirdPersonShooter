using System.Collections;
using System.Collections.Generic;
using Manager;
using ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UILogic
{
    public class UIInGame : MonoBehaviour
    {
        [Header("Health Bar")]
        [SerializeField] private Image _healthBar;
        [SerializeField] private float _healthBarChangeRate = 5f;
        
        [Header("Weapon Slot")]
        [SerializeField] private UIWeaponSlot _weaponSlot_UI;
        
        [Header("Missions")]
        [SerializeField] private TextMeshProUGUI _missionText;
        [SerializeField] private TextMeshProUGUI _missionDetailsText;
        [SerializeField] private GameObject _missionToolTipParent;
        
        [Header("Car Info")]
        [SerializeField] private Image _carHealthBar;
        [SerializeField] private TextMeshProUGUI _carSpeed;
        
        [Header("Character And Car UI")]
        [SerializeField] private GameObject _characterUI;
        [SerializeField] private GameObject _carUI;
        
        private bool _tooltipActive = true;

        private void Awake()
        {
            if (_weaponSlot_UI == null)
                _weaponSlot_UI = GetComponentInChildren<UIWeaponSlot>();
        }

        private void OnEnable()
        {
            EventManager.UIEvents.UIWeaponUpdate += UpdateWeaponUI;
            EventManager.UIEvents.UIWeaponAlpha += UpdateWeaponAlphaUI;
            EventManager.UIEvents.UIMissionToolTipSwitch += SwitchMissionToolTip;
        }
        
        private void OnDisable()
        {
            EventManager.UIEvents.UIWeaponUpdate -= UpdateWeaponUI;
            EventManager.UIEvents.UIWeaponAlpha -= UpdateWeaponAlphaUI;
            EventManager.UIEvents.UIMissionToolTipSwitch -= SwitchMissionToolTip;
        }
        
        public void SwitchToCharacterUI()
        {
            _characterUI.SetActive(true);
            _carUI.SetActive(false);
        }
        
        public void SwitchToCarUI()
        {
            _characterUI.SetActive(false);
            _carUI.SetActive(true);
        }
        
        public void UpdateMissionInfo(string missionName, string missionDetails = "")
        {
            _missionText.text = missionName;
            _missionDetailsText.text = missionDetails;
        }

        public void UpdateWeaponUI(List<PlayerWeaponSettingsSO> weaponSlots, PlayerWeaponSettingsSO currentWeapon)
        {
            // Tek bir slot olduğundan, yalnızca geçerli silahı güncelliyoruz.
            _weaponSlot_UI.UpdateWeaponSlot(currentWeapon);
        }
        
        public void UpdateWeaponAlphaUI(PlayerWeaponIndexSO slotIndex)
        {
            // Tek slot olduğundan, yalnızca slotIndex 0 ise alfa değeri aktif olacak şekilde güncelle.
            _weaponSlot_UI.UpdateWeaponAlpha(slotIndex.WeaponIndex == 0);
        }

        public void SwitchMissionToolTip()
        {
            _tooltipActive = !_tooltipActive;
            _missionToolTipParent.SetActive(_tooltipActive);
        }

        public void UpdateHealthUI(float currentHealth, float maxHealth)
        {
            float targetFillAmount = currentHealth / maxHealth;
            StartCoroutine(SmoothUpdateHealthBar(targetFillAmount));
        }
        
        public void UpdateCarHealthUI(float currentCarHealth, float maxCarHealth)
        {
            _carHealthBar.fillAmount = currentCarHealth / maxCarHealth;
        }
        
        public void UpdateCarSpeedTextUI(string currentSpeedText)
        {
            _carSpeed.text = currentSpeedText;
        }

        private IEnumerator SmoothUpdateHealthBar(float targetFillAmount)
        {
            while (Mathf.Abs(_healthBar.fillAmount - targetFillAmount) > 0.01f)
            {
                _healthBar.fillAmount = Mathf.Lerp(_healthBar.fillAmount, targetFillAmount, _healthBarChangeRate * Time.deltaTime);
                yield return null;
            }
            _healthBar.fillAmount = targetFillAmount;
        }
    }
}
