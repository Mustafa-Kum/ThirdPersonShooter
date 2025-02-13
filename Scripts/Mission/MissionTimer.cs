using Manager;
using UILogic;
using UnityEngine;

namespace MissionLogic
{
    [CreateAssetMenu(fileName = "MissionTimer", menuName = "Mission/MissionTimer")]
    public class MissionTimer : Mission
    {
        public float _timer;
        
        private float _currentTime;

        public override void StartMission()
        {
            _currentTime = _timer;
        }
        
        public override void UpdateMission()
        {
            _currentTime -= Time.deltaTime;

            if (_currentTime <= 0)
            {
                //EventManager.GameEvents.GameOver?.Invoke();
            }

            string timeText = System.TimeSpan.FromSeconds(_currentTime).ToString("mm':'ss");

            if (UI.instance == null)
            {
                Debug.LogError("UI instance is null. Make sure the UI script is in the scene and correctly initialized.");
                return;
            }

            if (UI.instance._inGameUI == null)
            {
                Debug.LogError("In-Game UI is not assigned in the UI script.");
                return;
            }

            UI.instance._inGameUI.UpdateMissionInfo("Time Remaining", timeText);
        }


        public override bool IsMissionComplete()
        {
            return _currentTime > 0;
        }
    }
}