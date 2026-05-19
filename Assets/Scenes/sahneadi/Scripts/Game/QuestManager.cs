using UnityEngine;
using Argos.UI;

namespace Argos.Game
{
    public class QuestManager : MonoBehaviour
    {
        public static QuestManager Instance { get; private set; }

        public readonly string[] questTexts = new string[]
        {
            "Müzeyi keşfet ve şüpheli eserleri incele.",
            "Anomali tespit edilen esere yaklaş ve portalı kullan.",
            "Geçmişten kanıt topla ve müzeye dön.",
            "Dava panosundan suçluyu seç ve sorgula."
        };

        public int CurrentQuestIndex { get; private set; }
        public string CurrentQuestText =>
            CurrentQuestIndex >= 0 && CurrentQuestIndex < questTexts.Length
                ? questTexts[CurrentQuestIndex]
                : "";

        [SerializeField] private QuestBox questBox;

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

        void Start()
        {
            UpdateQuestBox();
        }

        public void AdvanceQuest()
        {
            if (CurrentQuestIndex < questTexts.Length - 1)
            {
                CurrentQuestIndex++;
                UpdateQuestBox();
            }
        }

        public void UpdateQuestBox()
        {
            if (questBox != null) questBox.SetText(CurrentQuestText);
        }

        public void SetQuestBox(QuestBox box)
        {
            questBox = box;
            UpdateQuestBox();
        }
    }
}
