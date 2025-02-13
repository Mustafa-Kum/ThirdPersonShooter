using System;
using MissionLogic;
using UnityEngine;

namespace Manager
{
    public class MissionManager : MonoBehaviour
    {
        public static MissionManager instance;
        
        public Mission _currentMission;
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
        }

        private void OnEnable()
        {
            EventManager.GameEvents.MissionStart += StartMission;
        }
        
        private void OnDisable()
        {
            EventManager.GameEvents.MissionStart -= StartMission;
        }

        private void Update()
        {
            _currentMission.UpdateMission();
        }
        
        public void SetCurrentMission(Mission newMission)
        {
            _currentMission = newMission;
        }

        public void StartMission()
        {
            _currentMission.StartMission();
        }
        
        public bool IsMissionComplete()
        {
            return _currentMission.IsMissionComplete();
        }
    }
}

