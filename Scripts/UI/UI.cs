using System;
using System.Collections;
using Manager;
using UnityEngine;
using UnityEngine.UI;

namespace UILogic
{
    public class UI : MonoBehaviour
    {
        public static UI instance;
        
        [SerializeField] private GameObject[] _UIScreens;
        [SerializeField] private Image _imageFade;
        
        public GameObject _pauseUI;
        public GameObject _victoryUI;
        
        public UIInGame _inGameUI { get; private set; }
        public UIGameOver _gameOverUI { get; private set; }
        public UISettings _settingsUI { get; private set; }
        
        private void Awake()
        {
            instance = this;
            
            _inGameUI = GetComponentInChildren<UIInGame>(true);
            _gameOverUI = GetComponentInChildren<UIGameOver>(true);
            _settingsUI = GetComponentInChildren<UISettings>(true);
        }

        private void OnEnable()
        {
            EventManager.UIEvents.UIFadeIn += FadeIn;
            EventManager.UIEvents.UIFadeOut += FadeOut;
            EventManager.GameEvents.LevelCompleted += ShowVictoryUI;
        }

        private void OnDisable()
        {
            EventManager.UIEvents.UIFadeIn -= FadeIn;
            EventManager.UIEvents.UIFadeOut -= FadeOut;
            EventManager.GameEvents.LevelCompleted -= ShowVictoryUI;
        }

        private void Start()
        {
            AssignUIInput();
            FadeIn();
        }

        public void SwitchTo(GameObject uiToSwitchOn)
        {
            foreach (GameObject gameObjectUI in _UIScreens)
            {
                gameObjectUI.SetActive(false);
            }
            
            uiToSwitchOn.SetActive(true);
            
            if (uiToSwitchOn == _settingsUI.gameObject)
                _settingsUI.LoadSettings();
        }
        
        public void SwitchToInGameUI()
        {
            SwitchTo(_inGameUI.gameObject);
        }
        
        public void PauseSwitch()
        {
            bool gamePaused = _pauseUI.activeSelf;
            
            if (gamePaused)
            {
                SwitchTo(_inGameUI.gameObject);
                ControlsManager.instance.SwitchToCharacterControls();
                TimeManager.instance.ResumeTime();
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                SwitchTo(_pauseUI);
                ControlsManager.instance.SwitchToUIControls();
                TimeManager.instance.PauseTime();
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }
        
        public void ShowGameOverUI()
        {
            SwitchTo(_gameOverUI.gameObject);
        }
        
        public void ShowVictoryUI()
        {
            SwitchTo(_victoryUI.gameObject);
            ControlsManager.instance._playerControllerData._playerController.Character.Disable();
        }
        
        public void FadeInAndOut()
        {
            StartCoroutine(FadeInAndOutCoroutine());
        }

        private IEnumerator FadeInAndOutCoroutine()
        {
            yield return StartCoroutine(ChangeImageAlpha(1, 1));
            SwitchToInGameUI();
            yield return StartCoroutine(ChangeImageAlpha(0, 1));
        }

        private void AssignUIInput()
        {
            if (ControlsManager.instance == null) 
                return;
            
            PlayerController playerController = ControlsManager.instance._playerControllerData._playerController;
            
            playerController.UI.UIPause.performed += ctx => PauseSwitch();
        }
        
        private void FadeIn()
        {
            StartCoroutine(ChangeImageAlpha(0, 1));
        }
        
        private void FadeOut()
        {
            StartCoroutine(ChangeImageAlpha(1, 1));
        }
        
        private IEnumerator ChangeImageAlpha(float targetAlpha, float duration, System.Action onComplete = null)
        {
            float time = 0;
            Color currentColor = _imageFade.color;
            float startAlpha = currentColor.a;
            
            while (time < duration)
            {
                time += Time.deltaTime;
                float alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
                _imageFade.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
                yield return null;
            }
            
            _imageFade.color = new Color(currentColor.r, currentColor.g, currentColor.b, targetAlpha);
            
            onComplete?.Invoke();
        }
    }
}