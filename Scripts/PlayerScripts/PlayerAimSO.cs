using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "PlayerAimValueSO", menuName = "PlayerAimValueSO", order = 0)]
    public class PlayerAimSO : ScriptableObject
    {
        private Vector2 _aimInput;
        private Vector3 _aimDirection;
        private RaycastHit _lastKnownMouseHitInfo;
        
        public Vector2 AimInput
        {
            get { return _aimInput; }
            set { _aimInput = value; }
        }

    }
}