using UnityEngine;
using Argos.Evidence;
using Argos.Game;
using Argos.Player;
using Argos.UI;

namespace Argos.Artifacts
{
    [RequireComponent(typeof(Collider2D))]
    public class ArtifactInteractable : MonoBehaviour
    {
        [SerializeField] public ArtifactData data;
        private bool playerInRange;
        private bool internalVoicePlayed;

        // Portal kullanım durumu GameManager'da global tutuluyor — sahne
        // reload sonrası da korunsun diye.
        public bool PortalUsed => GameManager.Instance != null && GameManager.Instance.IsPortalUsed(data);

        void Reset()
        {
            var col = GetComponent<Collider2D>();
            col.isTrigger = true;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            playerInRange = true;

            bool anomaly = data != null && data.isPortalTrigger && !PortalUsed;

            if (anomaly)
            {
                UIManager.Instance?.ShowInteractPrompt("Portal Aç [E]");
                UIManager.Instance?.ShowGadgetWarning("Anomali Tespit Edildi!");
                // Anomalili eserde içsel ses yaklaşmada tetiklenir (incelenemiyor çünkü
                // E direkt portal açıyor).
                TriggerInternalVoiceIfNeeded();
            }
            else
            {
                UIManager.Instance?.ShowInteractPrompt("İncele [E]");
            }
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            playerInRange = false;
            UIManager.Instance?.HideInteractPrompt();
            UIManager.Instance?.HideGadgetWarning();
        }

        void Update()
        {
            if (!playerInRange) return;
            if (Input.GetKeyDown(KeyCode.E)) OnInteract();
        }

        void OnInteract()
        {
            if (data == null) return;

            // Adım 0 → 1: ilk artifact etkileşimi (portal trigger veya normal fark etmez).
            QuestManager.Instance?.TryAdvance(0);

            if (data.isPortalTrigger && !PortalUsed)
            {
                GameManager.Instance?.MarkPortalUsed(data);
                Argos.Portal.PortalManager.Instance?.OpenPortal();
                return;
            }

            UIManager.Instance?.OpenArtifactInspect(data);
            TriggerInternalVoiceIfNeeded();
        }

        void TriggerInternalVoiceIfNeeded()
        {
            if (internalVoicePlayed) return;
            if (data == null || string.IsNullOrEmpty(data.internalVoiceLine)) return;
            internalVoicePlayed = true;
            UIManager.Instance?.PlayInternalVoice(data.internalVoiceLine);
            PlayerInventory.Instance?.AddNote(new EvidenceNote
            {
                title = data.artifactName,
                description = data.internalVoiceLine,
                thumbnail = data.artifactSprite
            });
        }
    }
}
