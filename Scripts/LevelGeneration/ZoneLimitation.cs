using System;
using System.Collections;
using UnityEngine;

namespace LevelGeneration
{
    public class ZoneLimitation : MonoBehaviour
    {
        private BoxCollider _zoneCollider;

        private void Awake()
        {
            GetComponent<MeshRenderer>().enabled = false;
            _zoneCollider = GetComponent<BoxCollider>();
            
            ActivateWall(false);
        }

        private void ActivateWall(bool activate)
        {
            _zoneCollider.isTrigger = !activate;
        }

        private IEnumerator WallActivationCoroutine()
        {
            ActivateWall(true);

            yield return new WaitForSeconds(1);
            
            ActivateWall(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            StartCoroutine(WallActivationCoroutine());
        }
    }
}