using UnityEngine;
using Argos.Scenarios;
using Argos.NPC;

namespace Argos.Game
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private ScenarioData currentScenario;
        public ScenarioData CurrentScenario => currentScenario;

        public int WrongSuspectCount { get; private set; }
        public bool CaseSolved { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
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
        }
    }
}
