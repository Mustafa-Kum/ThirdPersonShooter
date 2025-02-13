using System.Collections.Generic;
using Manager;
using ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UILogic
{
    public class UITransparentOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private PlayerWeaponIndexSO _weaponIndex;
        
        private Dictionary<Image, Color> originalImageColors = new Dictionary<Image, Color>();
        private Dictionary<TextMeshProUGUI, Color> originalTextColors = new Dictionary<TextMeshProUGUI, Color>();

        private void Start()
        {
            foreach (var image in GetComponentsInChildren<Image>(true))
            {
                originalImageColors[image] = image.color;
            }
            
            foreach (var text in GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                originalTextColors[text] = text.color;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            foreach (var image in originalImageColors.Keys)
            {
                var color = image.color;
                color.a = 0.15f;
                image.color = color;
            }
            
            foreach (var text in originalTextColors.Keys)
            {
                var color = text.color;
                color.a = 0.15f;
                text.color = color;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            foreach (var image in originalImageColors.Keys)
            {
                image.color = originalImageColors[image];
            }
            
            foreach (var text in originalTextColors.Keys)
            {
                text.color = originalTextColors[text];
            }
            
            EventManager.UIEvents.UIWeaponAlpha.Invoke(_weaponIndex);
        }
    }
}
