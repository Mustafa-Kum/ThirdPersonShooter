using Manager;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UILogic
{
    public class UIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        [Header("Mouse Hover Settings")] 
        public float _scaleSpeed = 5;
        public float _scaleRate = 1.2f;
        
        private Vector3 _originalScale;
        private Vector3 _targetScale;
        private Image _buttonImage;
        private TextMeshProUGUI _buttonText;
        
        public virtual void Start()
        {
            _originalScale = transform.localScale;
            _targetScale = _originalScale;
            _buttonImage = GetComponent<Button>().image;
            _buttonText = GetComponentInChildren<TextMeshProUGUI>();
        }

        public virtual void Update()
        {
            if (Mathf.Abs(transform.lossyScale.x - _targetScale.x) > 0.01f)
            {
                float scaleValue = Mathf.Lerp(transform.localScale.x, _targetScale.x, _scaleSpeed * Time.deltaTime);
                
                transform.localScale = new Vector3(scaleValue, scaleValue, scaleValue);
            }
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            _targetScale = _originalScale * _scaleRate;
            EventManager.AudioEvents.AudioUISFX?.Invoke(0);
            
            if (_buttonImage != null)
                _buttonImage.color = Color.yellow;
            
            if (_buttonText != null)
                _buttonText.color = Color.yellow;
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            ReturnOriginalLook();
        }
        
        public virtual void OnPointerDown(PointerEventData eventData)
        {
            ReturnOriginalLook();
            EventManager.AudioEvents.AudioUISFX?.Invoke(1);
        }

        private void ReturnOriginalLook()
        {
            _targetScale = _originalScale;
            
            if (_buttonImage != null)
                _buttonImage.color = Color.white;
            
            if (_buttonText != null)
                _buttonText.color = Color.white;
        }
    }
}