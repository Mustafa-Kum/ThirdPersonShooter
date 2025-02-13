using System;
using UnityEngine;

namespace UILogic
{
    public class UIHelpToolTipWidthSet : MonoBehaviour
    {
        [SerializeField] private RectTransform _helpToolTipRectTransform;
        [SerializeField] private RectTransform _infoRectTransform;

        private void Update()
        {
            Vector2 newSize = _helpToolTipRectTransform.sizeDelta;
            newSize.x = _infoRectTransform.sizeDelta.x;
            _helpToolTipRectTransform.sizeDelta = newSize;
        }
    }
}