using System.Collections.Generic;
using EnemyLogic;
using LevelGeneration;
using UILogic;
using Unity.VisualScripting;
using UnityEngine;

namespace MissionLogic
{
    [CreateAssetMenu(fileName = "MissionEnemyHunt", menuName = "Mission/MissionEnemyHunt")]
    public class MissionEnemyHunt : Mission
    {
        public EnemyType _enemyTypeToHunt;
        public int _amountOfEnemiesToKill = 12;
        
        private int _targetsToKillLeft;
        
        public override void StartMission()
        {
            _targetsToKillLeft = _amountOfEnemiesToKill;
            
            UpdateMissionUI();
            
            MissionObject_HuntTarget.OnTargetKilled += ReduceKillTargetAmount;
            
            List<Enemy> validEnemies = new List<Enemy>();
            
            if (_enemyTypeToHunt == EnemyType.Random)
                validEnemies = LevelGenerator.instance.GetEnemiesList();
            else
            {
                foreach (Enemy enemy in LevelGenerator.instance.GetEnemiesList())
                {
                    if (enemy._enemyType == _enemyTypeToHunt)
                    {
                        validEnemies.Add(enemy);
                    }
                }
            }
            
            for (int i = 0; i < _amountOfEnemiesToKill; i++)
            {
                if (validEnemies.Count <= 0)
                    return;
                
                int randomIndex = Random.Range(0, validEnemies.Count);
                validEnemies[randomIndex].AddComponent<MissionObject_HuntTarget>();
                validEnemies.RemoveAt(randomIndex);
                
            }
        }

        public override bool IsMissionComplete()
        {
            return _targetsToKillLeft <= 0;
        }
        
        public void ReduceKillTargetAmount()
        {
            _targetsToKillLeft--;
            UpdateMissionUI();
            
            if (_targetsToKillLeft <= 0)
            {
                UI.instance._inGameUI.UpdateMissionInfo("Mission Complete", "All targets eliminated");
                MissionObject_HuntTarget.OnTargetKilled -= ReduceKillTargetAmount;
            }
        }

        private void UpdateMissionUI()
        {
            string missionName = "Hunt " + _amountOfEnemiesToKill + " " + _enemyTypeToHunt.ToString() + " enemies";
            string missionDetails = "Targets left: " + _targetsToKillLeft;
            
            UI.instance._inGameUI.UpdateMissionInfo(missionName, missionDetails);
        }
    }
}