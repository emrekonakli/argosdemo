using UnityEngine;
using UnityEngine.SceneManagement;

namespace Argos.Game
{
    /// <summary>
    /// Oyuncu bu trigger bölgesine (ofis kapısı, ~6.26, 2.51) girince Müze
    /// sahnesine ışınlar. OfficeSceneBootstrap tarafından kapı konumunda kurulur.
    /// MuseumButton ile aynı kapı: yalnızca dava kabul edilmişse (CaseAccepted)
    /// müzeye geçişe izin verir.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class MuseumEntryTrigger : MonoBehaviour
    {
        private bool triggered;

        void OnTriggerEnter2D(Collider2D other)
        {
            if (triggered) return;
            if (!other.CompareTag("Player")) return;
            if (GameManager.Instance == null || !GameManager.Instance.CaseAccepted) return;
            triggered = true;
            SceneManager.LoadScene(Scenes.Museum);
        }
    }
}
