using UnityEngine;
using Argos.UI;

namespace Argos.Game
{
    [RequireComponent(typeof(Collider2D))]
    public class CaseBoardInteractable : MonoBehaviour
    {
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
            UIManager.Instance?.ShowInteractPrompt("Dava Panosu [E]");
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
                UIManager.Instance?.OpenSuspectSelection();
        }
    }
}
