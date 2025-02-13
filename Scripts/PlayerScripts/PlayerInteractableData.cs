using System.Collections.Generic;
using Interactable;
using UnityEngine;

namespace Data
{
    [System.Serializable]
    
    public class PlayerInteractableData
    {
        [SerializeField] private List<InteractableObject> _interactableObjects = new List<InteractableObject>();
        
        private InteractableObject _closestInteractableObject;
        
        public List<InteractableObject> InteractableObjects
        {
            get { return _interactableObjects; }
            set { _interactableObjects = value; }
        }
        
        public InteractableObject ClosestInteractableObject
        {
            get { return _closestInteractableObject; }
            set { _closestInteractableObject = value; }
        }
    }
}