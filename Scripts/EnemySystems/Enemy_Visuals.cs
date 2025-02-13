using System;
using System.Collections.Generic;
using EnemyRangeLogic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace EnemyLogic
{
    public enum EnemyMelee_WeaponModelType
    {
        OneHand,
        Throw,
        Unarmed
    }
    
    public enum EnemyRange_WeaponModelType
    {
        Pistol,
        Revolver,
        Shotgun,
        Rifle,
        Sniper,
        Random
    }
    
    public class Enemy_Visuals : MonoBehaviour
    {
        [Header("Color")] 
        [SerializeField] private Texture[] _colorTextures;
        [SerializeField] private SkinnedMeshRenderer _skinnedMeshRenderer;
        public GameObject _currentWeaponModel { get; private set; }
        public GameObject _granedeModel;

        [Header("Corruption")] 
        [SerializeField] private GameObject[] _corruptionCrystals;
        [SerializeField] private int _corruptionAmount;
        
        [Header("Rig")]
        [SerializeField] private Transform _leftHandIK;
        [SerializeField] private Transform _leftElbowIK;
        [SerializeField] private TwoBoneIKConstraint _leftHandIKConstraint;
        [SerializeField] private MultiAimConstraint _weaponAimConstraint;
        
        private float _leftHandIKWeight;
        private float _weaponAimWeight;
        private float _rigChangeRate;

        private void Update()
        {
            if (_leftHandIKConstraint == null || _weaponAimConstraint == null)
                return;
            
            _leftHandIKConstraint.weight = AdjustIKWeight(_leftHandIKConstraint.weight, _leftHandIKWeight);
            _weaponAimConstraint.weight = AdjustIKWeight(_weaponAimConstraint.weight, _weaponAimWeight);
        }

        public void SetupLook()
        {
            SetupRandomColor();
            SetupRandomWeapon();
            SetupRandomCorruption();
        }

        public void EnableWeaponTrail(bool enable)
        {
            Enemy_WeaponModel currenWeaponScript = _currentWeaponModel.GetComponent<Enemy_WeaponModel>();
            
            currenWeaponScript.EnableTrailEffect(enable);
        }
        
        public void EnableIK(bool enableLeftHand, bool enableAim, float changeRate = 10)
        {
            _rigChangeRate = changeRate;
            
            _leftHandIKWeight = enableLeftHand ? 1 : 0;
            _weaponAimWeight = enableAim ? 1 : 0;
        }
        
        public void EnableWeaponModel(bool active)
        {
            _currentWeaponModel?.gameObject.SetActive(active);
        }
        
        public void EnableSecondaryWeaponModel(bool active)
        {
            FindSecondaryWeaponModels()?.SetActive(active);
        }

        private void SetupRandomWeapon()
        {
            bool thisEnemyIsMelee = GetComponent<EnemyMelee>() != null;
            bool thisEnemyIsRanged = GetComponent<EnemyRange>() != null;
            
            if (thisEnemyIsRanged)
                _currentWeaponModel = FindRangeWeaponModel();
            
            if (thisEnemyIsMelee)
                _currentWeaponModel = FindMeleeWeaponModel();
            
            _currentWeaponModel.SetActive(true);

            OverrideAnimatorController();
        }
        
        public void EnableGranedeModel(bool active)
        {
            _granedeModel?.SetActive(active);
        }

        private GameObject FindRangeWeaponModel()
        {
            EnemyRange_WeaponModel[] _enemyRangeWeaponModels = GetComponentsInChildren<EnemyRange_WeaponModel>(true);
            EnemyRange_WeaponModelType _enemyRangeWeaponModelType = GetComponent<EnemyRange>()._enemyRangeWeaponModelType;
            
            Debug.Log("Weapon Type: " + _enemyRangeWeaponModelType);

            foreach (var weaponModel in _enemyRangeWeaponModels)
            {
                if (weaponModel._enemyRangeWeaponModelType == _enemyRangeWeaponModelType)
                {
                    SwitchAnimationLayer((int)weaponModel._enemyRangeHoldType);
                    SetupLeftHandIK(weaponModel._leftHandTarget, weaponModel._leftElbowTarget);
                    
                    return weaponModel.gameObject;
                }
            }
            
            Debug.LogWarning("No weapon model found for " + _enemyRangeWeaponModelType);
            return null;
        }

        private GameObject FindMeleeWeaponModel()
        {
            Enemy_WeaponModel[] _enemyWeaponModels = GetComponentsInChildren<Enemy_WeaponModel>(true);
            EnemyMelee_WeaponModelType _enemyMeleeWeaponModelType = GetComponent<EnemyMelee>()._enemyMeleeWeaponModelType;
            
            List<Enemy_WeaponModel> filteredWeaponModels = new List<Enemy_WeaponModel>();

            foreach (var enemyWeaponModel in _enemyWeaponModels)
            {
                if (enemyWeaponModel._enemyMeleeWeaponModelType == _enemyMeleeWeaponModelType)
                    filteredWeaponModels.Add(enemyWeaponModel);
            }

            int randomIndex = Random.Range(0, filteredWeaponModels.Count);
            
            return filteredWeaponModels[randomIndex].gameObject;
        }
        
        private GameObject FindSecondaryWeaponModels()
        {
            EnemyRange_SecondaryRangeWeaponModel[] _enemySecondaryWeaponModels = 
                GetComponentsInChildren<EnemyRange_SecondaryRangeWeaponModel>(true);
            
            EnemyRange_WeaponModelType _enemyRangeWeaponModelType = GetComponentInParent<EnemyRange>()._enemyRangeWeaponModelType;

            foreach (var weaponModel in _enemySecondaryWeaponModels)
            {
                if (weaponModel._enemyRangeWeaponModelType == _enemyRangeWeaponModelType)
                {
                    return weaponModel.gameObject;
                }
            }
            
            return null;
        }

        private void OverrideAnimatorController()
        {
            AnimatorOverrideController overrideController =
                _currentWeaponModel.GetComponent<Enemy_WeaponModel>()?._animatorOverrideController;

            if (overrideController != null)
            {
                GetComponentInChildren<Animator>().runtimeAnimatorController = overrideController;
            }
        }

        private void SetupRandomColor()
        {
            int randomIndex = Random.Range(0, _colorTextures.Length);

            Material newMaterial = new Material(_skinnedMeshRenderer.material);

            newMaterial.mainTexture = _colorTextures[randomIndex];

            _skinnedMeshRenderer.material = newMaterial;
        }

        private void SetupRandomCorruption()
        {
            List<int> avaliableIndex = new List<int>();
            
            _corruptionCrystals = CollectCorruptionCrystals();

            for (int i = 0; i < _corruptionCrystals.Length; i++)
            {
                avaliableIndex.Add(i);
                _corruptionCrystals[i].SetActive(false);
            }

            for (int i = 0; i < _corruptionAmount; i++)
            {
                if (avaliableIndex.Count == 0)
                    break;
                
                int randomIndex = Random.Range(0, avaliableIndex.Count);
                int objectIndex = avaliableIndex[randomIndex];
                
                _corruptionCrystals[objectIndex].SetActive(true);
                avaliableIndex.RemoveAt(randomIndex);
            }
        }
        
        private void SwitchAnimationLayer(int layerIndex)
        {
            Animator anim = GetComponentInChildren<Animator>();
            
            for (int i = 0; i < anim.layerCount; i++)
            {
                anim.SetLayerWeight(i, 0);
            }
            
            anim.SetLayerWeight(layerIndex, 1);
        }

        private void SetupLeftHandIK(Transform leftHandTarget, Transform leftElbowTarget)
        {
            _leftHandIK.localPosition = leftHandTarget.localPosition;
            _leftHandIK.localRotation = leftHandTarget.localRotation;
            
            _leftElbowIK.localPosition = leftElbowTarget.localPosition;
            _leftElbowIK.localRotation = leftElbowTarget.localRotation;
        }
        
        private GameObject[] CollectCorruptionCrystals()
        {
            Enemy_CorruptionCrystal[] crystalComponents = GetComponentsInChildren<Enemy_CorruptionCrystal>(true);
            GameObject[] _corruptionCrystals = new GameObject[crystalComponents.Length];

            for (int i = 0; i < crystalComponents.Length; i++)
            {
                _corruptionCrystals[i] = crystalComponents[i].gameObject;
            }
            
            return _corruptionCrystals;
        }
        
        private float AdjustIKWeight(float currentWeight, float targetWeight)
        {
            if (Mathf.Abs(currentWeight - targetWeight) > 0.05f)
                return Mathf.Lerp(currentWeight, targetWeight, _rigChangeRate * Time.deltaTime);
            else
                return targetWeight;
        }
    }
}