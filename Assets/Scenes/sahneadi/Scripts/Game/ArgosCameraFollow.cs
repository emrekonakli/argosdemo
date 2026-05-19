using System.Collections;
using UnityEngine;

namespace Argos.Game
{
    [RequireComponent(typeof(Camera))]
    public class ArgosCameraFollow : MonoBehaviour
    {
        [SerializeField] public Transform target;
        [SerializeField] public Vector2 minBounds;
        [SerializeField] public Vector2 maxBounds;
        [SerializeField] public float smoothTime = 0.15f;

        private Vector3 velocity = Vector3.zero;
        private Vector3 shakeOffset = Vector3.zero;
        private Camera cachedCamera;

        void Awake()
        {
            cachedCamera = GetComponent<Camera>();
        }

        void LateUpdate()
        {
            if (target == null || cachedCamera == null) return;

            float vertExtent = cachedCamera.orthographicSize;
            float horzExtent = vertExtent * cachedCamera.aspect;

            Vector3 desired = target.position;
            desired.x = Mathf.Clamp(desired.x, minBounds.x + horzExtent, maxBounds.x - horzExtent);
            desired.y = Mathf.Clamp(desired.y, minBounds.y + vertExtent, maxBounds.y - vertExtent);
            desired.z = transform.position.z;

            Vector3 basePos = transform.position - shakeOffset;
            Vector3 smoothed = Vector3.SmoothDamp(basePos, desired, ref velocity, smoothTime);
            transform.position = smoothed + shakeOffset;
        }

        public void Shake(float duration, float magnitude)
        {
            StopCoroutine(nameof(ShakeRoutine));
            StartCoroutine(ShakeRoutine(duration, magnitude));
        }

        public void Shake()
        {
            Shake(0.3f, 0.2f);
        }

        IEnumerator ShakeRoutine(float duration, float magnitude)
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                Vector2 r = Random.insideUnitCircle * magnitude;
                shakeOffset = new Vector3(r.x, r.y, 0f);
                yield return null;
            }
            shakeOffset = Vector3.zero;
        }
    }
}
