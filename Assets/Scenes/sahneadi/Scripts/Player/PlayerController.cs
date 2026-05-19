using UnityEngine;

namespace Argos.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 3f;

        private Rigidbody2D rb;
        private SpriteRenderer sr;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            sr = GetComponent<SpriteRenderer>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        void FixedUpdate()
        {
            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");
            Vector2 input = new Vector2(x, y);
            if (input.sqrMagnitude > 1f) input.Normalize();
            rb.linearVelocity = input * moveSpeed;

            if (sr != null && Mathf.Abs(x) > 0.01f) sr.flipX = x < 0f;
        }
    }
}
