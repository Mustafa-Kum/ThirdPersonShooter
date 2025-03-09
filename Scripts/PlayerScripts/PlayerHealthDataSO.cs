using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "PlayerHealthDataSO", menuName = "PlayerHealthDataSO", order = 0)]
    public class PlayerHealthDataSO : ScriptableObject
    {
        public int _currentHealthAmount;
    }
}