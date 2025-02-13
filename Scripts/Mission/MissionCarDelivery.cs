using CarLogic;
using UILogic;
using Unity.VisualScripting;
using UnityEngine;

namespace MissionLogic
{
    [CreateAssetMenu(fileName = "MissionCarDelivery", menuName = "Mission/MissionCarDelivery")]
    public class MissionCarDelivery : Mission
    {
        private bool _carWasDelivered;
        
        public override void StartMission()
        {
            FindObjectOfType<MissionObject_CarDeliveryZone>(true).gameObject.SetActive(true);
            
            string missionName = "Find a functional car";
            string missionDetails = "Deliver car to the delivery zone";
            
            UI.instance._inGameUI.UpdateMissionInfo(missionName, missionDetails);
            
            _carWasDelivered = false;
            
            MissionObject_CarToDelivery.OnCarDelivery += CarDeliveryCompleted;
            
            CarController[] cars = FindObjectsOfType<CarController>();
            
            foreach (CarController car in cars)
            {
                car.AddComponent<MissionObject_CarToDelivery>();
            }
        }

        public override bool IsMissionComplete()
        {
            return _carWasDelivered;
        }
        
        private void CarDeliveryCompleted()
        {
            _carWasDelivered = true;
            
            MissionObject_CarToDelivery.OnCarDelivery -= CarDeliveryCompleted;
            
            UI.instance._inGameUI.UpdateMissionInfo("Mission Complete", "Car delivered to the delivery zone");
        }
    }
}