using System.Collections.Generic;
using UnityEngine;
using Argos.Artifacts;
using Argos.Scenarios;
using Argos.NPC;

namespace Argos.Game
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        // Sahne geçişinde NewspaperScene'in ending mi intro mu olarak açılacağını
        // sinyaller. NewspaperSceneBootstrap okuduktan sonra sıfırlar.
        public static bool PendingNewspaperEnding { get; set; }

        [SerializeField] private ScenarioData currentScenario;
        public ScenarioData CurrentScenario => currentScenario;

        public int WrongSuspectCount { get; private set; }
        public bool CaseSolved { get; private set; }
        public bool CaseAccepted { get; set; }

        private readonly HashSet<ArtifactData> usedPortals = new HashSet<ArtifactData>();
        private readonly List<string> shelvedCases = new List<string>();
        public IReadOnlyList<string> ShelvedCases => shelvedCases;

        public void ShelveCase()
        {
            if (currentScenario != null)
            {
                string title = !string.IsNullOrEmpty(currentScenario.scenarioTitle)
                    ? currentScenario.scenarioTitle : currentScenario.name;
                if (!shelvedCases.Contains(title))
                    shelvedCases.Add(title);
            }
            CaseAccepted = false;
            WrongSuspectCount = 0;
            CaseSolved = false;
            usedPortals.Clear();
        }
        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void SetScenario(ScenarioData scenario)
        {
            currentScenario = scenario;
            WrongSuspectCount = 0;
            CaseSolved = false;
        }

        public void RegisterWrongSuspect() => WrongSuspectCount++;

        public void SolveCase(NPCData accused)
        {
            CaseSolved = currentScenario != null && accused == currentScenario.culprit;
        }

        public void RestartScenario()
        {
            WrongSuspectCount = 0;
            CaseSolved = false;
            CaseAccepted = false;
            usedPortals.Clear();
        }

        public void MarkPortalUsed(ArtifactData artifact)
        {
            if (artifact != null) usedPortals.Add(artifact);
        }

        public bool IsPortalUsed(ArtifactData artifact)
        {
            return artifact != null && usedPortals.Contains(artifact);
        }
    }
}
