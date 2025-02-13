using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "PlayerWeaponIndexSO", menuName = "PlayerWeaponIndexSO", order = 0)]
    public class PlayerWeaponIndexSO : ScriptableObject
    {
        [SerializeField] private int _weaponIndex;
        
        public int WeaponIndex
        {
            get { return _weaponIndex; }
            set { _weaponIndex = value; }
        }
    }
}