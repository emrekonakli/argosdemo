using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Argos.AI;
using Argos.Evidence;
using Argos.Game;
using Argos.NPC;
using Argos.Player;

namespace Argos.UI
{
    public class InterrogationUI : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Image npcPortrait;
        [SerializeField] private TMP_Text npcName;
        [SerializeField] private TMP_Text patienceLabel;
        [SerializeField] private TMP_Text chatHistory;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Button sendButton;
        [SerializeField] private Button evidenceButton;
        [SerializeField] private Button summaryButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private GameObject preInterrogationPanel;
        [SerializeField] private GameObject evidencePickerPanel;

        private NPCData currentNpc;
        private bool culpritMode;
        private int remainingPatience;
        private readonly List<ChatMessage> history = new List<ChatMessage>();
        private readonly List<EvidencePhoto> shownEvidence = new List<EvidencePhoto>();

        void Awake()
        {
            if (sendButton != null) sendButton.onClick.AddListener(OnSend);
            if (exitButton != null) exitButton.onClick.AddListener(Hide);
        }

        public void Show(NPCData npc, bool culprit)
        {
            currentNpc = npc;
            culpritMode = culprit;
            history.Clear();
            shownEvidence.Clear();
            remainingPatience = npc != null ? npc.patienceCount : 6;
            if (root != null) root.SetActive(true);
            if (npcName != null) npcName.text = npc != null ? npc.npcName : "";
            if (npcPortrait != null && npc != null) npcPortrait.sprite = npc.portrait;
            if (exitButton != null) exitButton.gameObject.SetActive(!culprit);
            UpdatePatienceLabel();
            // TODO Faz 9: önce preInterrogationPanel'i göster (özet).
        }

        public void Hide()
        {
            if (root != null) root.SetActive(false);
        }

        async void OnSend()
        {
            if (inputField == null || string.IsNullOrWhiteSpace(inputField.text)) return;
            string userMsg = inputField.text;
            inputField.text = "";
            AppendChat("Sen", userMsg);
            remainingPatience = Mathf.Max(0, remainingPatience - 1);
            UpdatePatienceLabel();

            string systemPrompt = BuildSystemPrompt();
            string reply = AIManager.Instance != null
                ? await AIManager.Instance.SendChat(systemPrompt, history, userMsg)
                : "...";
            AppendChat(currentNpc != null ? currentNpc.npcName : "NPC", reply);

            if (culpritMode && reply.StartsWith("İTİRAF:"))
            {
                GameManager.Instance?.SolveCase(currentNpc);
                // TODO Faz 12: skor + newspaper ending akışı.
            }

            if (remainingPatience <= 0 && !culpritMode) Hide();
        }

        void AppendChat(string speaker, string line)
        {
            history.Add(new ChatMessage { role = speaker, content = line });
            if (chatHistory != null) chatHistory.text += $"\n<b>{speaker}:</b> {line}";
        }

        void UpdatePatienceLabel()
        {
            if (patienceLabel != null) patienceLabel.text = $"Sabır: {remainingPatience}";
        }

        string BuildSystemPrompt()
        {
            if (currentNpc == null) return "";
            string s = $"Sen {currentNpc.npcName} adlı karaktersin. {currentNpc.period}. Kişilik: {currentNpc.personality}";
            s += $"\nBildiğin bilgiler: {string.Join(", ", currentNpc.knownFacts)}";
            s += $"\nSakladıkların: {string.Join(", ", currentNpc.hiddenFacts)}";
            if (remainingPatience <= 1) s += $"\nZorunlu son bilgi: {currentNpc.mandatoryFinalFact}";
            if (culpritMode) s += "\nSen suçlusun. Yeterli kanıt varsa 'İTİRAF:' ile başla.";
            return s;
        }
    }
}
