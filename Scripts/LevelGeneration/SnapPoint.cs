using System;
using UnityEngine;

namespace LevelGeneration
{
    public enum SnapPointType
    {
        Enter,
        Exit
    }
    
    public class SnapPoint : MonoBehaviour
    {
        public SnapPointType _snapPointType;

        private void Awake()
        {
            BoxCollider boxCollider = GetComponent<BoxCollider>();
            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
            
            if (boxCollider != null)
                boxCollider.enabled = false;
            
            if (meshRenderer != null)
                meshRenderer.enabled = false;
        }

        private void OnValidate()
        {
            gameObject.name = "SnapPoint - " + _snapPointType.ToString();
        }
    }
}