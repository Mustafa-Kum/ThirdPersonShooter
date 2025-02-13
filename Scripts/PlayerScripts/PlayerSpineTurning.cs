using UnityEngine;

namespace PlayerScripts
{
    public class PlayerSpineTurning : MonoBehaviour
    {
        [SerializeField] private Transform _cameraTarget;

        private void Update()
        {
            transform.position = new Vector3(_cameraTarget.position.x, _cameraTarget.position.y, _cameraTarget.position.z);
        }
    }
}