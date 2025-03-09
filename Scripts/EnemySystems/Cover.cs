using System;
using System.Collections.Generic;
using EnemyRangeLogic;
using ScriptableObjects;
using UnityEngine;

namespace CoverLogic
{
    public class Cover : MonoBehaviour
    {
        [Header("Cover Points")]
        [SerializeField] private GameObject _coverPointPrefab;
        [SerializeField] private List<CoverPoint> _coverPoints = new List<CoverPoint>();
        [SerializeField] private float _xOffset = 1.0f;
        [SerializeField] private float _yOffset = 0.2f;
        [SerializeField] private float _zOffset = 1.0f;
        
        [SerializeField] private PlayerTransformValueSO _playerTransform;

        private void Start()
        {
            //GenerateCoverPoints();
        }
        
        public List<CoverPoint> GetCoverPoints(Transform enemyTransform)
        {
            List<CoverPoint> validCoverPoints = new List<CoverPoint>();
            
            foreach (CoverPoint coverPoint in _coverPoints)
            {
                if (IsValidCoverPoint(coverPoint, enemyTransform))
                {
                    validCoverPoints.Add(coverPoint);
                }
            }
            
            return validCoverPoints;
        }

        private void GenerateCoverPoints()
        {
            Vector3[] localCoverPoints =
            {
                new Vector3(0, _yOffset, _zOffset),
                new Vector3(0, _yOffset, -_zOffset),
                new Vector3(_xOffset, _yOffset, 0),
                new Vector3(-_xOffset, _yOffset, 0)
            };
            
            foreach (Vector3 localCoverPoint in localCoverPoints)
            {
                Vector3 worldCoverPoint = transform.TransformPoint(localCoverPoint);
                CoverPoint coverPoint = 
                    Instantiate(_coverPointPrefab, worldCoverPoint, Quaternion.identity, transform).GetComponent<CoverPoint>();
                
                _coverPoints.Add(coverPoint);
            }
        }
        
        private bool IsValidCoverPoint(CoverPoint coverPoint, Transform enemyTransform)
        {
            if (coverPoint._occupied)
            {
                return false;
            }

            if (IsFurtherestFromPlayer(coverPoint) == false)
            {
                return false;
            }
            
            if (IsCoverCloseToPlayer(coverPoint))
            {
                return false;
            }
            
            if (IsCoverBehindPlayer(coverPoint, enemyTransform))
            {
                return false;
            }
            
            if (IsCoverCloseToLastCover(coverPoint, enemyTransform))
            {
                return false;
            }
            
            return true;
        }
        
        private bool IsCoverBehindPlayer(CoverPoint coverPoint, Transform enemyTransform)
        {
            float distanceToPlayer = Vector3.Distance(coverPoint.transform.position, _playerTransform.PlayerTransform);
            float distanceToEnemy = Vector3.Distance(coverPoint.transform.position, enemyTransform.position);
            
            return distanceToPlayer < distanceToEnemy;
        }
        
        private bool IsCoverCloseToPlayer(CoverPoint coverPoint)
        {
            float distanceToPlayer = Vector3.Distance(coverPoint.transform.position, _playerTransform.PlayerTransform);
            
            return distanceToPlayer < 2.0f;
        }
        
        private bool IsCoverCloseToLastCover(CoverPoint coverPoint, Transform enemyTransform)
        {
            CoverPoint lastCover = enemyTransform.GetComponent<EnemyRange>()._currentCover;
            
            return lastCover != null && 
                   Vector3.Distance(coverPoint.transform.position, lastCover.transform.position) < 3.0f;
        }
        
        private bool IsFurtherestFromPlayer(CoverPoint coverPoint)
        {
            CoverPoint furtherestCoverPoint = null;
            float furtherestDistance = 0.0f;
            
            foreach (CoverPoint point in _coverPoints)
            {
                float distance = Vector3.Distance(point.transform.position, _playerTransform.PlayerTransform);
                
                if (distance > furtherestDistance)
                {
                    furtherestCoverPoint = point;
                    furtherestDistance = distance;
                }
            }
            
            return furtherestCoverPoint == coverPoint;
        }
    }
}