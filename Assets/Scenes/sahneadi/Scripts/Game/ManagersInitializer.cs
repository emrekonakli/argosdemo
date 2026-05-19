using UnityEngine;
using Argos.AI;
using Argos.Portal;

namespace Argos.Game
{
    /// <summary>
    /// Tüm sahneler için singleton manager'ları oyun başında otomatik kurar.
    /// Önceden her sahne kendi Bootstrap'inde Manager yaratıyordu — şimdi tek
    /// merkezden, RuntimeInitializeOnLoadMethod ile.
    /// </summary>
    public static class ManagersInitializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
            if (GameManager.Instance != null) return;

            var root = new GameObject("Managers");
            Object.DontDestroyOnLoad(root);
            root.AddComponent<GameManager>();
            root.AddComponent<QuestManager>();
            root.AddComponent<ScoreManager>();
            root.AddComponent<PortalManager>();
            var ai = root.AddComponent<AIManager>();
            var fb = root.AddComponent<FallbackResponseManager>();
            ai.SetFallback(fb);
            root.AddComponent<AITester>();
        }
    }
}
