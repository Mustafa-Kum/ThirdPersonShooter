using System;
using System.Collections.Generic;
using EnemyLogic;
using Lean.Pool;
using UILogic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MissionLogic
{
    [CreateAssetMenu(fileName = "MissionLastDefence", menuName = "Mission/MissionLastDefence")]
    public class MissionLastDefence : Mission
    {
        public bool _defenceBegun = false;
        
        [Header("Cooldown and Duration")]
        public float _defenceDuration = 120;
        public int _waveCooldown = 15;
        
        [Header("Respawn Details")] 
        public int _amountOfRespawnPoints = 2;
        public int _enemiesPerWave;
        public GameObject[] _enemyPrefabs;
        public List<Transform> _respawnPoints;
        
        private Vector3 _defencePoint;
        private float _defenceTimer;
        private float _waveTimer;
        private string _defenceTimerText;

        private void OnEnable()
        {
            _defenceBegun = false;
        }

        public override void StartMission()
        {
            _defencePoint = FindObjectOfType<MissionEndTrigger>().transform.position;
            _respawnPoints = new List<Transform>(ClosestRespawnPoints(_amountOfRespawnPoints));
            
            UI.instance._inGameUI.UpdateMissionInfo("Last Defence", "Defend the point for 2 minutes");
        }
        
        public override void UpdateMission()
        {
            if (_defenceBegun == false)
                return;
            
            _waveTimer -= Time.deltaTime;
            
            if (_defenceTimer > 0)
                _defenceTimer -= Time.deltaTime;
            
            if (_waveTimer <= 0)
            {
                CreateNewEnemies(_enemiesPerWave);
                _waveTimer = _waveCooldown;
            }
            
            _defenceTimerText = System.TimeSpan.FromSeconds(_defenceTimer).ToString("mm':'ss");
            
            UI.instance._inGameUI.UpdateMissionInfo("Last Defence", $"Defend the point for {_defenceTimerText}");
        }

        public override bool IsMissionComplete()
        {
            if (_defenceBegun == false)
            {
                StartDefenceEvent();
                return false;
            }
            
            return _defenceTimer <= 0;
        }
        
        private void CreateNewEnemies(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                int randomEnemyIndex = Random.Range(0, _enemyPrefabs.Length);
                int randomRespawnPointIndex = Random.Range(0, _respawnPoints.Count);
                
                GameObject randomEnemy = _enemyPrefabs[randomEnemyIndex];
                Transform randomRespawnPoint = _respawnPoints[randomRespawnPointIndex];
                
                randomEnemy.GetComponent<Enemy>()._agroRange = 100;

                LeanPool.Spawn(randomEnemy, randomRespawnPoint);
            }
        }
        
        private void StartDefenceEvent()
        {
            _waveTimer = 0.5f;
            _defenceTimer = _defenceDuration;
            _defenceBegun = true;
        }
        
        private List<Transform> ClosestRespawnPoints(int amount)
        {
            List<Transform> closestRespawnPoints = new List<Transform>();
            List<MissionObject_EnemyRespawnPoint> allRespawnPoints = 
                new List<MissionObject_EnemyRespawnPoint>(FindObjectsOfType<MissionObject_EnemyRespawnPoint>());

            while (closestRespawnPoints.Count < amount && allRespawnPoints.Count > 0)
            {
                float shortestDistance = float.MaxValue;
                MissionObject_EnemyRespawnPoint closestRespawnPoint = null;
                
                foreach (var point in allRespawnPoints)
                {
                    float distance = Vector3.Distance(point.transform.position, _defencePoint);
                    if (distance < shortestDistance)
                    {
                        shortestDistance = distance;
                        closestRespawnPoint = point;
                    }
                }
                
                if (closestRespawnPoint != null)
                {
                    closestRespawnPoints.Add(closestRespawnPoint.transform);
                    allRespawnPoints.Remove(closestRespawnPoint);
                }
            }
            
            return closestRespawnPoints;
        }
    }
}