using System;
using System.Collections.Generic;
using EnemyLogic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LevelGeneration
{
    public class LevelPart : MonoBehaviour
    {
        [Header("Intersection Check")] 
        [SerializeField] private LayerMask _intersectionLayer;
        [SerializeField] private Transform _intersectionCheckParent;
        [SerializeField] private Collider[] _intersectionCheckColliders;

        private void Awake()
        {
            if (_intersectionCheckColliders.Length <= 0)
            {
                _intersectionCheckColliders = _intersectionCheckParent.GetComponentsInChildren<Collider>();
            }
        }

        [ContextMenu("Set static to environment Layer")]
        private void AdjustLayerForStaticObjects()
        {
            foreach (Transform childTransform in transform.GetComponentsInChildren<Transform>(true))
            {
                if (childTransform.gameObject.isStatic)
                {
                    childTransform.gameObject.layer = LayerMask.NameToLayer("Environment");
                }
            }
        }

        public bool IntersectionDetected()
        {
            Physics.SyncTransforms();

            foreach (var colliders in _intersectionCheckColliders)
            {
                Collider[] hitColliders = Physics.OverlapBox(colliders.bounds.center, colliders.bounds.extents,
                    Quaternion.identity, _intersectionLayer);

                foreach (var hit in hitColliders)
                {
                    IntersectionCheck intersectionCheck = hit.GetComponentInParent<IntersectionCheck>();

                    if (intersectionCheck != null && _intersectionCheckParent != intersectionCheck.transform)
                        return true;
                }
            }

            return false;
        }
        
        public Enemy[] MyEnemies()
        {
            return GetComponentsInChildren<Enemy>(true);
        }
        
        public void SnapAndAlignPartTo(SnapPoint targetSnapPoint)
        {
            SnapPoint entrancePoint = GetEntrancePoint();
            
            AlignTo(entrancePoint, targetSnapPoint);
            SnapTo(entrancePoint, targetSnapPoint);
        }

        private void AlignTo(SnapPoint ownSnapPoint, SnapPoint targetSnapPoint)
        {
            var rotationOffSet = ownSnapPoint.transform.rotation.eulerAngles.y - transform.rotation.eulerAngles.y;

            transform.rotation = targetSnapPoint.transform.rotation;
            transform.Rotate(0, 180, 0);
            transform.Rotate(0, -rotationOffSet, 0);
        }
        
        private void SnapTo(SnapPoint ownSnapPoint, SnapPoint targetSnapPoint)
        {
            var offSet = transform.position - ownSnapPoint.transform.position;

            var newPosition = targetSnapPoint.transform.position + offSet;

            transform.position = newPosition;
        }
        
        public SnapPoint GetEntrancePoint() => GetSnapPointOfType(SnapPointType.Enter);

        public SnapPoint GetExitPoint() => GetSnapPointOfType(SnapPointType.Exit);
        
        private SnapPoint GetSnapPointOfType(SnapPointType pointType)
        {
            SnapPoint[] snapPoints = GetComponentsInChildren<SnapPoint>();

            List<SnapPoint> filteredSnapPoints = new List<SnapPoint>();

            foreach (SnapPoint snapPoint in snapPoints)
            {
                if (snapPoint._snapPointType == pointType)
                {
                    filteredSnapPoints.Add(snapPoint);
                }
            }

            if (filteredSnapPoints.Count > 0)
            {
                int randomIndex = Random.Range(0, filteredSnapPoints.Count);

                return filteredSnapPoints[randomIndex];
            }

            return null;
        }
    }
}