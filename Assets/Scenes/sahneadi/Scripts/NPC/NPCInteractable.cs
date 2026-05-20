using UnityEngine;
using Argos.UI;

namespace Argos.NPC
{
    [RequireComponent(typeof(Collider2D))]
    public class NPCInteractable : MonoBehaviour
    {
        [SerializeField] public NPCData data;
        // Faz 13 placeholder ses: clip atanırsa yaklaşmada çalar, atanmazsa no-op.
        [SerializeField] private AudioSource approachAudio;
        private bool playerInRange;

        void Reset()
        {
            var col = GetComponent<Collider2D>();
            col.isTrigger = true;
        }

        void Awake()
        {
            if (approachAudio == null)
            {
                approachAudio = gameObject.AddComponent<AudioSource>();
                approachAudio.playOnAwake = false;
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            playerInRange = true;
            UIManager.Instance?.ShowInteractPrompt("Konuş [E]");
            if (approachAudio != null && approachAudio.clip != null) approachAudio.Play();
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            playerInRange = false;
            UIManager.Instance?.HideInteractPrompt();
        }

        void Update()
        {
            if (!playerInRange) return;
            if (UIManager.Instance != null && UIManager.Instance.IsAnyModalOpen) return;
            if (Input.GetKeyDown(KeyCode.E))
                UIManager.Instance?.OpenInterrogation(data, false);
        }
    }
}
