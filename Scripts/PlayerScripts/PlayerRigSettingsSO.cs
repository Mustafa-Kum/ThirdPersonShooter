using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "PlayerRigSettingsSO", menuName = "PlayerRigSettingsSO", order = 0)]
    public class PlayerRigSettingsSO : ScriptableObject
    {
        [SerializeField] private bool _rigWeightIncrease;
        [SerializeField] private bool _isGrabbingWeapon;
        
        public bool RigWeightIncrease
        {
            get { return _rigWeightIncrease; }
            set { _rigWeightIncrease = value; }
        }
        
        public bool IsGrabbingWeapon
        {
            get { return _isGrabbingWeapon; }
            set { _isGrabbingWeapon = value; }
        }
    }
}