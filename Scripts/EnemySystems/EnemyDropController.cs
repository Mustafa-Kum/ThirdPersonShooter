using UnityEngine;

namespace EnemySystems
{
    public class EnemyDropController : MonoBehaviour
    {
        [SerializeField] private GameObject _missionObjectKey;
        
        public void GiveKey(GameObject newKey)
        {
            _missionObjectKey = newKey;
        }
        
        public void DropItem()
        {
            if (_missionObjectKey != null)
            {
                CreateItem(_missionObjectKey);
            }
        }
        
        private void CreateItem(GameObject item)
        {
            GameObject newItem = Instantiate(item, transform.position + Vector3.up, Quaternion.identity);
        }
    }
}