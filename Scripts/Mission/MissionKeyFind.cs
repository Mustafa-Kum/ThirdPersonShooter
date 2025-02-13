using EnemyLogic;
using EnemySystems;
using LevelGeneration;
using UILogic;
using UnityEngine;

namespace MissionLogic
{
    [CreateAssetMenu(fileName = "MissionKeyFind", menuName = "Mission/MissionKeyFind", order = 0)]
    public class MissionKeyFind : Mission
    {
        [SerializeField] private GameObject _key;
        
        private bool _keyFound;
        
        public override void StartMission()
        {
            MissionObject_Key.OnKeyPickedUp += PickUpKey;
            
            UI.instance._inGameUI.UpdateMissionInfo("Find the key holder", "Find the key to unlock the exit");
            
            Enemy enemy = LevelGenerator.instance.GetRandomEnemy();
            enemy.GetComponent<EnemyDropController>()?.GiveKey(_key);
            enemy.ChosenEnemyForKey();
        }

        public override bool IsMissionComplete()
        {
            return _keyFound;
        }
        
        private void PickUpKey()
        {
            _keyFound = true;
            
            MissionObject_Key.OnKeyPickedUp -= PickUpKey;
            
            UI.instance._inGameUI.UpdateMissionInfo("Mission Complete", "Key found, exit unlocked");
        }
    }
}