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
        public bool PortalUsed { get; private set; }

        void Reset()
        {
            var col = GetComponent<Collider2D>();
            col.isTrigger = true;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            playerInRange = true;
            string label = (data != null && data.isPortalTrigger && !PortalUsed) ? "Portal Aç [E]" : "İncele [E]";
            UIManager.Instance?.ShowInteractPrompt(label);
            if (data != null && data.isPortalTrigger && !PortalUsed)
                UIManager.Instance?.ShowGadgetWarning("Anomali Tespit Edildi!");
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

            if (data.isPortalTrigger && !PortalUsed)
            {
                PortalUsed = true;
                QuestManager.Instance?.AdvanceQuest();
                Argos.Portal.PortalManager.Instance?.OpenPortal();
                return;
            }

            UIManager.Instance?.OpenArtifactInspect(data);

            if (!internalVoicePlayed && !string.IsNullOrEmpty(data.internalVoiceLine))
            {
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
}
