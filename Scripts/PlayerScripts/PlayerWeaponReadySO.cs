using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "PlayerWeaponReadySO", menuName = "PlayerWeaponReadySO", order = 0)]
    public class PlayerWeaponReadySO : ScriptableObject
    {
        [SerializeField] private bool _isWeaponReady;
        
        public bool IsWeaponReady
        {
            get { return _isWeaponReady; }
            set { _isWeaponReady = value; }
        }
    }
}