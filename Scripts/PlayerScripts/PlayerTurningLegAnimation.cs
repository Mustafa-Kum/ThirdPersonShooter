using UnityEngine;

namespace PlayerScripts
{
    public class PlayerTurningLegAnimation : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        
        private CharacterController _characterController;
        private float _turningVelocity;
        
        private void Start()
        {
            _characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            Turning();
        }

        private void Turning()
        {
            if (_characterController.velocity.magnitude < 0.2f)
                return;
            
            float rotationY = transform.rotation.eulerAngles.y;
            
            _turningVelocity = Mathf.Clamp(rotationY / 360f, 0.2f, 0.2f);
            
            _animator.SetFloat("TurningVelocity", _turningVelocity);
        }
    }
}