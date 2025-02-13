using TMPro;
using UnityEngine;

namespace UILogic
{
    public class UIMissionSelection : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _missonDescriptionText;
        
        public void UpdateMissionDescription(string description)
        {
            _missonDescriptionText.text = description;
        }
    }
}