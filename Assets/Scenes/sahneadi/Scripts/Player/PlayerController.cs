using UnityEngine;
using Argos.UI;

namespace Argos.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 3f;

        [Header("Directional sprites")]
        public Sprite spriteFront;
        public Sprite spriteBack;
        public Sprite spriteLeft;
        public Sprite spriteRight;

        [Header("Walk animations (sprite arrays)")]
        public Sprite[] walkRightFrames;
        public Sprite[] walkLeftFrames;
        public Sprite[] walkFrontFrames;
        public Sprite[] walkBackFrames;
        public Sprite[] walkRightFrontFrames;
        public Sprite[] walkRightBackFrames;
        public Sprite[] walkLeftFrontFrames;
        public Sprite[] walkLeftBackFrames;
        public float walkFps = 5f;

        private Rigidbody2D rb;
        private SpriteRenderer sr;
        private float animTimer;
        private int animFrame;
        private Sprite[] currentAnim;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            sr = GetComponent<SpriteRenderer>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        void Start()
        {
            LoadIfEmpty(ref walkRightFrames, "WalkRight");
            LoadIfEmpty(ref walkLeftFrames, "WalkLeft");
            LoadIfEmpty(ref walkFrontFrames, "WalkFront");
            LoadIfEmpty(ref walkBackFrames, "WalkBack");
            LoadIfEmpty(ref walkRightFrontFrames, "WalkRightFront");
            LoadIfEmpty(ref walkRightBackFrames, "WalkRightBack");
            LoadIfEmpty(ref walkLeftFrontFrames, "WalkLeftFront");
            LoadIfEmpty(ref walkLeftBackFrames, "WalkLeftBack");
        }

        void LoadIfEmpty(ref Sprite[] frames, string resourceFolder)
        {
            if (frames == null || frames.Length == 0)
                frames = Resources.LoadAll<Sprite>(resourceFolder);
        }

        void FixedUpdate()
        {
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

        void Update()
        {
            if (currentAnim == null || currentAnim.Length == 0) return;
            animTimer += Time.deltaTime;
            float interval = 1f / walkFps;
            if (animTimer >= interval)
            {
                animTimer -= interval;
                animFrame = (animFrame + 1) % currentAnim.Length;
                if (sr != null) sr.sprite = currentAnim[animFrame];
            }
        }

        void UpdateDirectionSprite(float x, float y)
        {
            if (sr == null) return;
            float ax = Mathf.Abs(x);
            float ay = Mathf.Abs(y);

            if (ax < 0.01f && ay < 0.01f)
            {
                StopAnim();
                return;
            }

            bool hasX = ax > 0.01f;
            bool hasY = ay > 0.01f;

            if (hasX && hasY)
            {
                if (x > 0f && y < 0f)      PlayAnim(walkRightFrontFrames, spriteFront);
                else if (x > 0f && y > 0f)  PlayAnim(walkRightBackFrames, spriteBack);
                else if (x < 0f && y < 0f)  PlayAnim(walkLeftFrontFrames, spriteFront);
                else                         PlayAnim(walkLeftBackFrames, spriteBack);
            }
            else if (hasX)
            {
                if (x > 0f) PlayAnim(walkRightFrames, spriteRight);
                else         PlayAnim(walkLeftFrames, spriteLeft);
            }
            else
            {
                if (y > 0f) PlayAnim(walkBackFrames, spriteBack);
                else         PlayAnim(walkFrontFrames, spriteFront);
            }
        }

        void PlayAnim(Sprite[] frames, Sprite fallback)
        {
            if (frames != null && frames.Length > 0)
            {
                if (currentAnim != frames)
                {
                    currentAnim = frames;
                    animFrame = 0;
                    animTimer = 0f;
                    if (sr != null) sr.sprite = frames[0];
                }
            }
            else
            {
                StopAnim();
                if (fallback != null && sr != null && sr.sprite != fallback)
                    sr.sprite = fallback;
            }
        }

        void StopAnim()
        {
            currentAnim = null;
            animFrame = 0;
            animTimer = 0f;
        }
    }
}
