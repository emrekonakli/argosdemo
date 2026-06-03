using UnityEngine;
using UnityEngine.SceneManagement;

namespace Argos.Game
{
    /// <summary>
    /// Oyuncu bu trigger bölgesine girince Ofis sahnesine ışınlar.
    /// MuseumSceneBootstrap tarafından müzenin alt-orta sınırında
    /// (0, -5.5, 0) konumunda kurulur.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class OfficeExitTrigger : MonoBehaviour
    {
        private bool triggered;

        void OnTriggerEnter2D(Collider2D other)
        {
            if (triggered) return;
            if (!other.CompareTag("Player")) return;
            triggered = true;
            SceneManager.LoadScene(Scenes.Office);
        }
    }
}
