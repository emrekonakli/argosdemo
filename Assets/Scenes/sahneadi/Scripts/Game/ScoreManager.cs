using UnityEngine;

namespace Argos.Game
{
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        public int EvidenceCount { get; set; }
        public int WrongSuspectCount { get; set; }
        public int RemainingPatience { get; set; }

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

        public int CalculateScore()
        {
            const int baseScore = 1000;
            int evidenceBonus = EvidenceCount * 50;
            int wrongSuspectPenalty = WrongSuspectCount * 200;
            int patienceBonus = RemainingPatience * 30;
            return baseScore + evidenceBonus - wrongSuspectPenalty + patienceBonus;
        }
    }
}
