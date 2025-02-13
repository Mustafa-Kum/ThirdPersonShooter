using Manager;
using UnityEngine;

namespace PlayerScripts
{
    public class PlayerControlBool : MonoBehaviour
    {
        private bool _controlEnabled;

        private void OnEnable()
        {
            EventManager.PlayerEvents.PlayerControlBool += SetControlsEnabled;
        }
        
        private void OnDisable()
        {
            EventManager.PlayerEvents.PlayerControlBool -= SetControlsEnabled;
        }

        private void SetControlsEnabled(bool enabled)
        {
            _controlEnabled = enabled;
        }
    }
}