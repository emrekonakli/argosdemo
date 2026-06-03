using UnityEngine;
using UnityEngine.SceneManagement;
using Argos.UI;

namespace Argos.Game
{
    /// <summary>
    /// Oyuncu bu trigger bölgesine girince ekranda "Olay Yerini İncele [E]"
    /// promptu gösterir; oyuncu E'ye basınca Olay Yeri (flashback) sahnesine
    /// ışınlar. PortalSceneBootstrap tarafından (-3, 2.5, 0) konumunda kurulur.
    /// MuseumEntryTrigger gibi otomatik yüklemez — NPCInteractable'daki
    /// "menzile gir + E ile onayla" desenini izler.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class CrimeSceneEntryTrigger : MonoBehaviour
    {
        [SerializeField] private string promptText = "Olay Yerini İncele [E]";
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
            UIManager.Instance?.ShowInteractPrompt(promptText);
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
            {
                UIManager.Instance?.HideInteractPrompt();
                SceneManager.LoadScene(Scenes.CrimeScene);
            }
        }
    }
}
