using System;
using Data;
using UILogic;
using UnityEngine;

namespace Manager
{
    public class ControlsManager : MonoBehaviour
    {
        public static ControlsManager instance;
        
        public PlayerControllerData _playerControllerData;
        
        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            SwitchToCharacterControls();
        }

        public void SwitchToCharacterControls()
        {
            _playerControllerData._playerController.Character.Enable();
            
            _playerControllerData._playerController.Car.Disable();
            _playerControllerData._playerController.UI.Disable();
            
            EventManager.PlayerEvents.PlayerControlBool(true);
            
            UI.instance._inGameUI.SwitchToCharacterUI();
        }
        
        public void SwitchToUIControls()
        {
            _playerControllerData._playerController.UI.Enable();
            
            _playerControllerData._playerController.Car.Disable();
            _playerControllerData._playerController.Character.Disable();
            
            EventManager.PlayerEvents.PlayerControlBool(false);
        }

        public void SwitchToCarControls()
        {
            _playerControllerData._playerController.Car.Enable();
            
            _playerControllerData._playerController.UI.Disable();
            _playerControllerData._playerController.Character.Disable();
            
            EventManager.PlayerEvents.PlayerControlBool(false);
            
            UI.instance._inGameUI.SwitchToCarUI();
        }
    }
}