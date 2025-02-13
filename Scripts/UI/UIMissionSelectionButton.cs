using System;
using Manager;
using MissionLogic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UILogic
{
    public class UIMissionSelectionButton : UIButton
    {
        [SerializeField] private Mission _mission;

        private TextMeshProUGUI _missionNameText;
        private UIMissionSelection _missionUI;

        private void OnValidate()
        {
            gameObject.name = "Button - Select Mission: " + _mission._missionName;
        }

        public override void Start()
        {
            base.Start();
            
            _missionUI = GetComponentInParent<UIMissionSelection>();
            _missionNameText = GetComponentInChildren<TextMeshProUGUI>();
            _missionNameText.text = _mission._missionName;
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            
            _missionUI.UpdateMissionDescription(_mission._missionDescription);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            
            _missionUI.UpdateMissionDescription("Choose a mission to start");
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            
            MissionManager.instance.SetCurrentMission(_mission);
            
            UI.instance.SwitchToInGameUI();
        }
    }
}