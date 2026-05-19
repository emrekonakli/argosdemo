using UnityEngine;
using Argos.UI;

namespace Argos.NPC
{
    [RequireComponent(typeof(Collider2D))]
    public class NPCInteractable : MonoBehaviour
    {
        [SerializeField] public NPCData data;
        private bool playerInRange;

        void Reset()
        {
            var col = GetComponent<Collider2D>();
            col.isTrigger = true;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            playerInRange = true;
            UIManager.Instance?.ShowInteractPrompt("Konuş [E]");
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
            if (Input.GetKeyDown(KeyCode.E))
                UIManager.Instance?.OpenInterrogation(data, false);
        }
    }
}
