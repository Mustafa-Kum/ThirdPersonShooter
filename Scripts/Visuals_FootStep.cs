using UnityEngine;

namespace DefaultNamespace
{
    public class Visuals_FootStep : MonoBehaviour
    {
        [SerializeField] private LayerMask _whatIsGround;
        [SerializeField] private TrailRenderer _leftFootTrail;
        [SerializeField] private TrailRenderer _rightFootTrail;
        [Range(0.001f, 0.3f)] [SerializeField] private float _checkRadius = 0.05f;
        [Range(-0.15f, 0.15f)] [SerializeField] private float _rayDistance = -0.05f;

        private void Update()
        {
            CheckFootStep(_leftFootTrail);
            CheckFootStep(_rightFootTrail);
        }

        private void CheckFootStep(TrailRenderer footTrail)
        {
            Vector3 checkPosition = footTrail.transform.position + Vector3.down * _rayDistance;
            bool touchingGround = Physics.CheckSphere(checkPosition, _checkRadius, _whatIsGround);

            footTrail.emitting = touchingGround;
        }

        private void OnDrawGizmos()
        {
            DrawFootGizmos(_leftFootTrail.transform, Color.green);
            DrawFootGizmos(_rightFootTrail.transform, Color.blue);
        }

        private void DrawFootGizmos(Transform foot, Color color)
        {
            if (foot == null)
                return;

            Gizmos.color = color;
            Vector3 checkPosition = foot.position + Vector3.down * _rayDistance;
            Gizmos.DrawWireSphere(checkPosition, _checkRadius);
        }
    }
}
