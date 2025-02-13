using UnityEngine;

namespace CoverLogic
{
    public class CoverPoint : MonoBehaviour
    {
        public bool _occupied;
        
        public void SetOccupied(bool occupied)
        {
            _occupied = occupied;
        }
    }
}