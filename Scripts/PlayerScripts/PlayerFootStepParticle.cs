using Manager;
using UnityEngine;

namespace PlayerScripts
{
    public class PlayerFootStepParticle : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _leftFootParticle;
        [SerializeField] private ParticleSystem _rightFootParticle;

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            EventManager.PlayerEvents.PlayerLeftFootStepParticle += OnLeftFootStepParticle;
            EventManager.PlayerEvents.PlayerRightFootStepParticle += OnRightFootStepParticle;
        }

        private void UnsubscribeFromEvents()
        {
            EventManager.PlayerEvents.PlayerLeftFootStepParticle -= OnLeftFootStepParticle;
            EventManager.PlayerEvents.PlayerRightFootStepParticle -= OnRightFootStepParticle;
        }

        private void OnLeftFootStepParticle()
        {
            PlayParticle(_leftFootParticle);
        }

        private void OnRightFootStepParticle()
        {
            PlayParticle(_rightFootParticle);
        }

        /// <summary>
        /// Belirtilen partikül efektini çalıştırır.
        /// </summary>
        private void PlayParticle(ParticleSystem particleSystem)
        {
            if (particleSystem != null)
            {
                particleSystem.Play();
            }
        }
    }
}