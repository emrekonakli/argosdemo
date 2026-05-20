using UnityEngine;
using Argos.UI;

namespace Argos.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 3f;

        // Yön bazlı sprite'lar. Bootstrap Resources üzerinden default'larını yükler.
        [Header("Directional sprites")]
        public Sprite spriteFront;
        public Sprite spriteBack;
        public Sprite spriteLeft;
        public Sprite spriteRight;

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
            // Modal panel açıkken WASD player'ı hareket ettirmesin.
            if (UIManager.Instance != null && UIManager.Instance.IsAnyModalOpen)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }

            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");
            Vector2 input = new Vector2(x, y);
            if (input.sqrMagnitude > 1f) input.Normalize();
            rb.linearVelocity = input * moveSpeed;

            UpdateDirectionSprite(x, y);
        }

        void UpdateDirectionSprite(float x, float y)
        {
            if (sr == null) return;
            float ax = Mathf.Abs(x);
            float ay = Mathf.Abs(y);
            // Hareket yoksa sprite'ı değiştirme (son yön korunsun).
            if (ax < 0.01f && ay < 0.01f) return;

            Sprite next;
            if (ax >= ay) next = x > 0f ? spriteRight : spriteLeft;
            else          next = y > 0f ? spriteBack  : spriteFront;
            if (next != null && sr.sprite != next) sr.sprite = next;
        }
    }
}
