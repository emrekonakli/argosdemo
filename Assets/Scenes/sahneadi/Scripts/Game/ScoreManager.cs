using UnityEngine;
using Argos.Player;

namespace Argos.Game
{
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        // Sorgu sırasında InterrogationUI kalan ortalama sabırı buraya yazar
        // (canlı bir yerde tutulmadığı için cached değer).
        public int LastInterrogationRemainingPatience { get; set; }

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

        public int CalculateScore()
        {
            const int baseScore = 1000;
            int evidenceCount = PlayerInventory.Instance != null
                ? PlayerInventory.Instance.Photos.Count + PlayerInventory.Instance.Notes.Count
                : 0;
            int wrongSuspectCount = GameManager.Instance != null
                ? GameManager.Instance.WrongSuspectCount
                : 0;
            int evidenceBonus = evidenceCount * 50;
            int wrongSuspectPenalty = wrongSuspectCount * 200;
            int patienceBonus = LastInterrogationRemainingPatience * 30;
            return baseScore + evidenceBonus - wrongSuspectPenalty + patienceBonus;
        }
    }
}
