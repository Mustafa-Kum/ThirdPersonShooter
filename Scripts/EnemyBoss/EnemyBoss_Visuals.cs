using System;
using UnityEngine;

namespace EnemyBossLogic
{
    public class EnemyBoss_Visuals : MonoBehaviour
    {
        [Header("Batteries")]
        [SerializeField] private GameObject[] _batteries;
        [SerializeField] private float _initialBatteryScaleY = 0.2f;
        
        [Header("Visual System")]
        [SerializeField] private ParticleSystem _landingZoneParticles;
        [SerializeField] private GameObject[] _weaponTrails;
        
        private EnemyBoss _enemyBoss;
        private float _dischargeSpeed;
        private float _rechargeSpeed;
        private bool _isRecharging;

        private void Awake()
        {
            _enemyBoss = GetComponent<EnemyBoss>();
            
            _landingZoneParticles.transform.parent = null;
            _landingZoneParticles.Stop();
            
            ResetBatteries();  
        }

        private void Update()
        {
            UpdateBatteriesScale();
        }

        public void ResetBatteries()
        {
            _isRecharging = true;
            
            _rechargeSpeed = _initialBatteryScaleY / _enemyBoss._abilityCooldown;
            _dischargeSpeed = _initialBatteryScaleY / (_enemyBoss._flameThrowerDuration * 0.75f);
            
            foreach (var battery in _batteries)
            {
                battery.SetActive(true);
            }
        }
        
        public void DischargeBattery()
        {
            _isRecharging = false;
        }
        
        public void PlaceLandingZoneParticles(Vector3 target)
        {
            _landingZoneParticles.transform.position = target;
            _landingZoneParticles.Clear();
            
            var mainModule = _landingZoneParticles.main;
            mainModule.startLifetime = _enemyBoss._travelTimeToTarget / 2;
            
            _landingZoneParticles.Play();
        }

        public void EnableWeaponTrail(bool active)
        {
            if (_weaponTrails.Length <= 0)
                return;
            
            foreach (var weaponTrail in _weaponTrails)
            {
                weaponTrail.SetActive(active);
            }
        }

        private void UpdateBatteriesScale()
        {
            if (_batteries.Length <= 0)
                return;

            foreach (var battery in _batteries)
            {
                if (battery.activeSelf)
                {
                    float scaleChange = (_isRecharging ? _rechargeSpeed : -_dischargeSpeed) * Time.deltaTime;
                    float newScaleY = Mathf.Clamp(battery.transform.localScale.y + scaleChange, 0, _initialBatteryScaleY);
                    
                    battery.transform.localScale = new Vector3(0.15f, newScaleY, 0.15f);
                    
                    if (battery.transform.localScale.y <= 0)
                        battery.SetActive(false);
                }
            }
        }
    }
}