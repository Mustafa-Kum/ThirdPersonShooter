using System;
using UnityEngine;

namespace MissionLogic
{
    public class MissionObject_HuntTarget : MonoBehaviour
    {
        public static event Action OnTargetKilled;
        
        public void TargetKilled()
        {
            OnTargetKilled?.Invoke();
        }
    }
}