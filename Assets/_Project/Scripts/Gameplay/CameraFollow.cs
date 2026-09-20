using UnityEngine;

namespace Rebonk.Gameplay
{
    /// <summary>Hard-follow (no smoothing) so the Pixel Perfect Camera can snap without jitter.</summary>
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;

        private void LateUpdate()
        {
            if (target == null)
                return;

            var p = target.position;
            transform.position = new Vector3(p.x, p.y, transform.position.z);
        }
    }
}
