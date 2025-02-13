using UnityEngine;

namespace MissionLogic
{
    public abstract class Mission : ScriptableObject
    {
        public string _missionName;
        
        [TextArea]
        public string _missionDescription;
        
        public abstract void StartMission();
        public abstract bool IsMissionComplete();
        public virtual void UpdateMission()
        {
        }
    }
}

