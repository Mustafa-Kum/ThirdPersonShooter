using System;
using CarLogic;
using UnityEngine;

namespace MissionLogic
{
    public class MissionObject_CarDeliveryZone : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            CarController car = other.GetComponent<CarController>();
            
            if (car == null)
                return;
            
            car.GetComponent<MissionObject_CarToDelivery>().InvokeOnCarDelivery();
            
            Debug.Log("Car delivered");
        }
    }
}