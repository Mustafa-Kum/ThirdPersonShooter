using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UILogic
{
    public class UIComicPanel : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private Image[] _comicImages;
        [SerializeField] private GameObject _buttonToEnable;
        [SerializeField] private float _screenStartDelay = 2f;
        
        private Image _myImage;
        private bool _comicShowOver;
        private int _imageIndex = 0;

        private void Start()
        {
            _myImage = GetComponent<Image>();
            StartCoroutine(ShowNextImageWithDelay());
        }
        
        private void ShowNextImage()
        {
            if (_comicShowOver)
                return;
            
            if (_imageIndex < _comicImages.Length)
                StartCoroutine(ChangeImageAlpha(1, 1, ShowNextImage));
        }
        
        private void FinishComicShow()
        {
            StopAllCoroutines();
            _comicShowOver = true;
            _buttonToEnable.SetActive(true);
            _myImage.raycastTarget = false;
        }
        
        private void ShowNextImageOnClick()
        {
            if (_imageIndex >= _comicImages.Length)
            {
                FinishComicShow();
            }
            
            if (_comicShowOver)
                return;
            
            _comicImages[_imageIndex].color = Color.white;
            _imageIndex++;
            
            ShowNextImage();
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            ShowNextImageOnClick();
        }
        
        private IEnumerator ChangeImageAlpha(float targetAlpha, float duration, System.Action onComplete = null)
        {
            float time = 0;
            Color currentColor = _comicImages[_imageIndex].color;
            float startAlpha = currentColor.a;
            
            while (time < duration)
            {
                time += Time.deltaTime;
                float alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
                _comicImages[_imageIndex].color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
                yield return null;
            }
            
            _comicImages[_imageIndex].color = new Color(currentColor.r, currentColor.g, currentColor.b, targetAlpha);
            
            _imageIndex++;
            
            if (_imageIndex >= _comicImages.Length)
            {
                FinishComicShow();
            }
            
            onComplete?.Invoke();
        }
        
        private IEnumerator ShowNextImageWithDelay()
        {
            yield return new WaitForSeconds(_screenStartDelay);
            ShowNextImage();
        }
    }
}